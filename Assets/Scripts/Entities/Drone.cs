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
                  speed_p = 1, speed_i = 0, speed_d = 0;


    // PLANT PARAMETERS
    [SerializeField]
    private float motor_saturation = 100; // max acceleration per frame


    private PIDController_float pidX, pidY, pidHead, pidHeight, pidSpeed;
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
        pidSpeed = new PIDController_float(speed_p, speed_i, speed_d);
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

        // thrust is now relative to frame of drone

        pidHeight.SetParams(height_p, height_i, height_d);
        thrust.y += StepHeight();
        Debug.DrawRay(transform.position, thrust.normalized * motor_saturation, Color.black); // the maximum possible thrust we could have
        Debug.DrawRay(transform.position, thrust, Color.cyan); // unclamped

        float desiredY = thrust.y;
        // saturate the force we can apply
        if (motor_saturation != 0) {
            if(thrust.y > 0) {
                if(thrust.y > motor_saturation) {
                    thrust.y = motor_saturation;
                    Debug.DrawRay(transform.position, thrust, Color.blue); // saturated
                }
            }
        }
        pidHead.SetParams(head_p, head_i, head_d);
        float angle = StepHeading(desiredY);
        //Debug.Log("Angle: " + angle);

        rotation = angle * Vector3.Cross(Vector3.up, targetPos - transform.position).normalized;
        Debug.DrawRay(transform.position, Vector3.up, Color.green);
        Debug.DrawRay(transform.position, targetPos - transform.position, Color.blue);
        Debug.DrawRay(transform.position, rotation, Color.red);

        thrust = thrust.magnitude * transform.up; // make thrust only act up/down from the drone's perspective
        Debug.DrawRay(transform.position, thrust, Color.green); // rotated

        // thrust is now relative to world


        rb.AddForce(thrust, ForceMode.Acceleration);
        rb.AddTorque(rotation, ForceMode.Acceleration);
    }

    public void SetTarget(Vector3 targetPosition) {
        targetPos = targetPosition;
        //startPos = transform.position;
    }

    private float StepHeading(float desiredY) {
        //float feedback = -(targetPos.y - transform.position.y); // distance from drone to target
        float feedback = Mathf.Acos(transform.up.y / 1) * Mathf.Sign(Vector3.Dot(targetPos - transform.position, transform.up)); // angle of drone
        // We want to rotate to an angle such that
        // the horizontal component of the thrust is in the direction towards the target, and
        // the vertical component of the thrust = the desired vertical thrust

        // Desired angle: the angle that is the most steep
        // that would not saturate the vertical thrust
        // The angle is measured from the vertical (i.e. 0 degrees for a stable hover)

        // if can't reach desired angle, refer to perfectly straight up
        float ratio = desiredY / motor_saturation;
        float referencePitch = 0;
        if(ratio > -1 && ratio < 1) {
            referencePitch = Mathf.Acos(desiredY / motor_saturation);
        }

        //Debug.Log("ERROR: " + (referencePitch - feedback) + " = " + referencePitch + " - " + feedback + " where trans.up.y = " + transform.up.y);

        return pidHead.Step(feedback, referencePitch);
    }

    private float StepHeight() {
        float feedback = -(targetPos.y - transform.position.y); // distance from drone to target
        float reference = 0; // we want that distance to be zero

        float y = Mathf.Max(0, pidHeight.Step(feedback, reference) + -Physics.gravity.y);


        return y;
    }
    private void StepSpeed() {
        //    float xVRef = VRef(startPos.x, targetPos.x, 1, transform.position.x - startPos.x);
        //    float yVRef = VRef(startPos.z, targetPos.z, 1, transform.position.z - startPos.z);

        //Vector3 temp = transform.position;
        //temp.x += xVRef;
        //temp.z += yVRef;
        //transform.position = temp;
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
