using UnityEngine;
using System.Collections;

// Controls and commands for an autonomous drone.
public class Drone : MonoBehaviour {

    /*
     * Raycasts down for ground effect?
     * Apply Thrust towards transform.up
     * 
     * 
     * the attitude of the drone is set to look like 
     * 
     * 
     INPUTS:
        x, y of the drone (unity coords: x, z)
        height of the drone (unity coord: y)
        heading
        speed

    OUTPUTS:
        
        vertical thrust
            an acceleration
            ASSERT: Thrust can never be negative
        angle/heading/yaw/pitch/roll
            a torque


    // Global Planner
        // ignore for now

    // Local Planner
    P(ID) control to reach target waypoints for:
      position (2D)
        x (1D)
        y (1D)
      heading (1D)
      speed (1D)
      height (1D)
     
     */

    // COMPENSATOR PARAMETERS
    [SerializeField]
    private float x_p = 1, x_i = 0, x_d = 0,
                  head_p = 1, head_i = 0, head_d = 0,
                  height_p = 20, height_i = 0, height_d = 470,
                  horiz_p = 1, horiz_i = 0, horiz_d = 0;


    // PLANT PARAMETERS
    [SerializeField]
    private float motor_saturation = 100; // max acceleration per frame


    private PIDController_float pidX, pidY, pidHead, pidHeight, pidHoriz;
    [SerializeField]
    private Transform targetPosObject = null;
    private Vector3 targetPos;
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        pidX = new PIDController_float(x_p, x_i, x_d);
        pidY = new PIDController_float(x_p, x_i, x_d);
        pidHead = new PIDController_float(head_p, head_i, head_d);
        pidHeight = new PIDController_float(height_p, height_i, height_d);
        pidHoriz = new PIDController_float(horiz_p, horiz_i, horiz_d);

        if(Player.PlayerInstance != null) {
            targetPosObject = Player.PlayerInstance.transform;
        }

        SetTarget(targetPosObject.position);
    }

    private void Update() {
        SetTarget(targetPosObject.position);

        if (Input.GetKeyDown(KeyCode.G)) {
            transform.position = Vector3.zero;
            rb.velocity = Vector3.zero;
            pidHeight.Clear();
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
            pidHead.Clear();
        }
    }

    private void FixedUpdate() {

        Vector3 thrust = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        Vector3 distance = targetPos - transform.position;
        float distanceAngle = Mathf.Atan2(distance.z, distance.x);
        Debug.DrawRay(transform.position, distance, Color.gray);

        // thrust is now relative to frame of drone

        pidHeight.SetParams(height_p, height_i, height_d);
        thrust.y += StepHeight(distance);
        
        Debug.DrawRay(transform.position, thrust.normalized * motor_saturation, Color.black); // the maximum possible thrust we could have
        Debug.DrawRay(transform.position, thrust, Color.cyan); // unclamped vertical thrust

        // Idea: form desired thrust vector.
        pidHoriz.SetParams(horiz_p, horiz_i, horiz_d);

        float stepHoriz =  StepHoriz(distance);
        thrust.x += Mathf.Cos(distanceAngle) * stepHoriz;
        thrust.z += Mathf.Sin(distanceAngle) * stepHoriz;

        Debug.DrawRay(transform.position, new Vector3(thrust.x, 0, thrust.z), Color.cyan); // unclamped horizontal thrust
        Debug.Log("Horizontal angle: " + distanceAngle + "x: " + thrust.x + " z: " + thrust.z);

        Debug.DrawRay(transform.position, thrust, Color.yellow); // desired
        //float desiredY = thrust.y;
        // saturate the force we can apply
        if (motor_saturation != 0) {
            thrust = Vector3.ClampMagnitude(thrust, motor_saturation);

            //if(thrust.y > 0) {
            //    if(thrust.y > motor_saturation) {
            //        thrust.y = motor_saturation;
            //        Debug.DrawRay(transform.position, thrust, Color.blue); // saturated
            //    }
            //}
        }
        pidHead.SetParams(head_p, head_i, head_d);
        float angle = StepHeading(distance, thrust);
        Debug.Log("Angle acceleration: " + angle + "(was " + rb.angularVelocity.magnitude * 180 / Mathf.PI + ")");

        rotation = angle * Vector3.Cross(Vector3.up, targetPos - transform.position).normalized;
        Debug.DrawRay(transform.position, Vector3.up, Color.green);
        //Debug.DrawRay(transform.position, rotation, Color.red);

        thrust = thrust.magnitude * transform.up; // make thrust only act up/down from the drone's perspective
        Debug.DrawRay(transform.position, thrust, Color.blue); // rotated

        // thrust is now relative to world


        rb.AddForce(thrust, ForceMode.Acceleration);
        if(!float.IsNaN(rotation.x))
            rb.AddTorque(rotation, ForceMode.Acceleration);
    }

    public void SetTarget(Vector3 targetPosition) {
        targetPos = targetPosition;
        //startPos = transform.position;
    }

    // INPUT: ANGLE of drone to get desired thrust
    // OUTPUT: TORQUE to apply to frame
    private float StepHeading(Vector3 distance, Vector3 desiredThrust) {
        float feedback = Mathf.Acos(transform.up.y / 1); // angle of drone
        // If the thrust is on the opposite side of the drone, the feedback is considered a negative angle
        Vector3 desiredHorizThrust = desiredThrust;
        desiredHorizThrust.y = 0;
        Vector3 distanceHoriz = distance;
        distanceHoriz.y = 0;
        // We want to rotate to an angle such that
        // the horizontal component of the thrust is in the direction towards the target, and
        // the vertical component of the thrust = the desired vertical thrust

        // Desired angle: the angle that is the most steep
        // that would not saturate the vertical thrust
        // The angle is measured from the vertical (i.e. 0 degrees for a stable hover)

        // if can't reach desired angle, refer to perfectly straight up
        //float ratio = desiredY / motor_saturation;
        //float referencePitch = 0;
        //if(ratio > -1 && ratio < 1) {
        //    referencePitch = Mathf.Acos(desiredY / motor_saturation);
        //}

        Vector3 forwardsProjection = desiredThrust;
        forwardsProjection.y = 0;
        Vector3 pitchProjection = transform.up;
        pitchProjection.y = 0;

        float referencePitch = Mathf.Acos(desiredThrust.y / desiredThrust.magnitude);
        
        // make TOWARDS TARGET be POSITIVE
        if(Vector3.Dot(forwardsProjection, distance) < 0) {
            referencePitch = -referencePitch;
        }
        if (Vector3.Dot(pitchProjection, distance) < 0) {
            feedback = -feedback;
        }

        //if (Vector3.Dot(forwardsProjection, pitchProjection) < 0) {
        //    referencePitch = -referencePitch;
        //}
        Debug.Log("DOT of FORWARDS on PITCH: " + Vector3.Dot(forwardsProjection, pitchProjection));
        Debug.Log("DOT of FORWARDS on DiSTANCE: " + Vector3.Dot(forwardsProjection, distance));
        Debug.Log("DOT of PITCH on DISTANCE: " + Vector3.Dot(pitchProjection, distance));
        Debug.DrawRay(transform.position, forwardsProjection, Color.white);
        Debug.DrawRay(transform.position, pitchProjection, Color.black);

        Debug.Log("ERROR: " + (referencePitch - feedback) + " = " + referencePitch + " - " + feedback);

        return pidHead.Step(feedback, referencePitch);
    }

    // INPUT: DISTANCE to target
    // OUTPUT: DESIRED VERTICAL THRUST
    private float StepHeight(Vector3 distance) {
        float feedback = -(distance.y); // vertical distance from drone to target
        float reference = 0; // we want that distance to be zero

        float y = Mathf.Max(0, pidHeight.Step(feedback, reference) + -Physics.gravity.y);


        return y;
    }

    // Thrust PID:
    // INPUT: DISTANCE to target
    // OUTPUT: DESIRED HORIZONTAL THRUST

    private float StepHoriz(Vector3 distance) {
        distance.y = 0;
        float feedback = -(distance.magnitude); // horizontal distance from drone to target
        float reference = 0; // we want that distance to be zero

        float x = /*Mathf.Max(0, */pidHoriz.Step(feedback, reference)/*)*/;


        return x;
    }

    private float VRef(float A, float B, float R, float x) {
        if (A < x && x < A + R) {
            return x;
        } else if (A + R < x && x < B - R) {
            return 1;
        } else {
            return B - x;
        }
    }
}
