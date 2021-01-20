using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the scripted/procedural animation for Kog.
/// Includes the leg and arm animations while walking, sprinting, and anchored.
/// Controls inverse kinematics for limbs.
/// </summary>
public class KogAnimation : MonoBehaviour {

    #region constants
    private const float zeroThresholdSqr = 0.05f * 0.05f;
    private const float radiusForRaycast = 0.125f;
    public float waistLegRotationFactor = 0.5f;
    public float waistReactionTime = 1;
    public float leaningMax = 25f;
    public float crouchingMax = 0.035f;
    public float crouchingLeanMax = 25f;
    public float crouchingLegSpreadMax = 0.25f;
    public float defaultRaycastDistance = 2;
    public float defaultDistanceBetweenSteps = .25f;
    public float defaultStepTime = 0.3f;
    public float stepHeight = .5f;
    public float legForwardsFactor = 0.1f;
    public float stepToTargetPositonDelta = 10;
    public float stepToTargetRotationLerpFactor = 10;
    public float armHeight = 1.25f;
    #endregion

    private enum GroundedState { Grounded, Airborne }

    public bool IsGrounded => leftLeg.groundedState == GroundedState.Grounded || rightLeg.groundedState == GroundedState.Grounded;
    public Rigidbody StandingOnRigidbody => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnRigidbody : rightLeg.standingOnRigidbody;
    public Vector3 StandingOnPoint => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnPoint : rightLeg.standingOnPoint;
    public Vector3 StandingOnNormal => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnNormal : rightLeg.standingOnNormal;

    private float distanceBetweenSteps = .25f;
    private Quaternion leftShoulderRestRotation, rightShoulderRestRotation;

    //[SerializeField]
    //private Transform target = null;
    //[SerializeField]
    //private Transform hand = null;
    //[SerializeField]
    //private Transform pole = null;

    private Animator anim;
    private Rigidbody rb;
    [SerializeField, Range(0.0f, 1.0f)]
    private float crouching = 0;
    [SerializeField]
    private Leg leftLeg = null;
    [SerializeField]
    private Leg rightLeg = null;
    [SerializeField]
    private Arm leftArm = null;
    [SerializeField]
    private Arm rightArm = null;
    [SerializeField]
    private Transform waist = null, waistAnchor = null, head = null, headAnchor = null, shoulderLeft = null, shoulderLeftAnchor = null, shoulderRight = null, shoulderRightAnchor = null;
    [SerializeField]
    private CapsuleCollider bodyCollider = null;
    [SerializeField]
    private SphereCollider lifterCollider = null;

    void Start() {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInParent<Rigidbody>();

        leftShoulderRestRotation = shoulderLeft.rotation;
        rightShoulderRestRotation = shoulderRight.rotation;

        leftLeg.Initialize(this, rightLeg);
        rightLeg.Initialize(this, leftLeg);
        leftArm.Initialize(this, leftLeg);
        rightArm.Initialize(this, rightLeg);

        //anim.Play("Armature|I-Pose");
    }

    public void Clear() {
        leftLeg.Clear();
        rightLeg.Clear();
    }

    private void OnAnimatorMove() {

        // Rotate the waist to reflect the current velocity, like you're leaning into the movement
        Vector3 movement = rb.velocity;
        movement = waist.parent.InverseTransformDirection(movement);
        movement.y = 0;
        float ratio = movement.magnitude / KogMovementController.topSpeedSprinting;
        if (ratio > 1)
            ratio = 1;
        else if (ratio < 0.05f)
            movement = Vector3.forward;
        Vector3 cross = Vector3.Cross(transform.up, movement);
        Quaternion waistRotationFromSpeed = Quaternion.AngleAxis(ratio * leaningMax + crouching * crouchingLeanMax, cross);

        // Rotate the waist to reflect the positions of the legs and feet.
        // The angle formed by the feet should be opposite of the angle formed by the waist.
        float angle = IMath.AngleBetween_Signed(rightLeg.foot.position - leftLeg.foot.position, waist.parent.right, transform.up, false) * Mathf.Rad2Deg * waistLegRotationFactor;
        Quaternion waistRotationFromLegs = Quaternion.AngleAxis(angle, transform.up);
        // Apply the final rotation
        Quaternion desiredWaistRotation = waist.parent.rotation * waistRotationFromSpeed * waistRotationFromLegs;
        waistAnchor.rotation = Quaternion.Slerp(waistAnchor.rotation, desiredWaistRotation, 1 - Mathf.Exp(-waistReactionTime * Time.deltaTime));
        // Also rotate the shoulders or torso to oppose that
        //shoulderLeftAnchor.rotation = shoulderLeft.parent.rotation * leftShoulderRestRotation * waistRotationFromLegs;
        //shoulderRightAnchor.rotation = shoulderRight.parent.rotation * rightShoulderRestRotation * waistRotationFromLegs;

        // Rotate the head to face forwards

        // Crouching
        bodyCollider.transform.localPosition = new Vector3(0, crouching * crouchingMax, 0);

        leftLeg.LegUpdate();
        rightLeg.LegUpdate();
        leftArm.ArmUpdate();
        rightArm.ArmUpdate();

        //// Lower the lifter to keep both Idle feet where their anchors are
        //// Set the lifter's position to be at the same height as the heighest Idle foot.
        //Vector3 pos = transform.position;

        //float heighest = 0;
        //if (leftLeg.state == Leg.WalkingState.Idle) {
        //    float height = Vector3.Distance(leftLeg.foot.position, leftLeg.footAnchor);
        //    Debug.Log("Left leg height: " + height);
        //    if (height > heighest)
        //        heighest = height;
        //}
        //if (rightLeg.state == Leg.WalkingState.Idle) {
        //    float height = Vector3.Distance(rightLeg.foot.position, rightLeg.footAnchor);
        //    Debug.Log("Right leg height: " + height);
        //    if (height > heighest)
        //        heighest = height;
        //}
        //pos.y += lifterCollider.radius - heighest;
        //lifterCollider.transform.position = pos;
    }

    public void SetLegTarget(Vector3 movement, float currentSpeed) {
        Vector3 crouchSpread = waist.right * crouchingLegSpreadMax * crouching;
        Vector3 legForwardsMovement = Vector3.down + movement * legForwardsFactor;
        Vector3 leftMove = (-crouchSpread + legForwardsMovement) * defaultRaycastDistance;
        Vector3 rightMove = (crouchSpread + legForwardsMovement) * defaultRaycastDistance;

        float length = leftMove.magnitude;
        leftLeg.raycastDirection = leftMove.normalized;
        rightLeg.raycastDirection = rightMove.normalized;
        leftLeg.raycastDistance = length;
        rightLeg.raycastDistance = length;
        leftLeg.currentSpeed = currentSpeed;
        rightLeg.currentSpeed = currentSpeed;
        //leftLeg.stepTime = defaultStepTime / length;
        //rightLeg.stepTime = defaultStepTime / length;
        //Debug.Log(currentSpeed);

        distanceBetweenSteps = defaultDistanceBetweenSteps * length;
    }

    [System.Serializable]
    private class Leg {

        public enum WalkingState { Idle, Stepping }

        public WalkingState state = WalkingState.Idle;
        public GroundedState groundedState = GroundedState.Grounded;
        public Rigidbody standingOnRigidbody = null;
        public Vector3 standingOnPoint = Vector3.zero;
        public Vector3 standingOnNormal = Vector3.zero;
        public Vector3 raycastDirection = Vector3.zero;
        public float raycastDistance, currentSpeed, stepTime;

        [SerializeField]
        public Transform foot = null, thigh = null, calf = null;
        [SerializeField]
        public Transform footTarget = null;
        [SerializeField]
        public Transform footRaycastSource = null;
        [SerializeField]
        public Transform footCollider = null;
        [SerializeField]
        public Transform debugBall_HitPoint = null, debugBall_currentAnchor = null;

        private bool AnchorOutOfRange => (foot.position - footAnchor).magnitude> 0.05f;

        private KogAnimation parent;
        private Leg otherLeg;
        public Vector3 footAnchor = Vector3.zero, footLastAnchor = Vector3.zero, footNextAnchor = Vector3.zero;
        private Quaternion footAnchorRotation, footLastAnchorRotation, footNextAnchorRotation;
        private Quaternion anchorRestLocalRotation;
        private Quaternion footRestLocalRotation;
        private Vector3 footColliderPosition;

        private float currentDistance = 0;
        private float tInStep = 0;

        public void Initialize(KogAnimation parent, Leg otherLeg) {
            this.otherLeg = otherLeg;
            this.parent = parent;
            raycastDistance = parent.defaultRaycastDistance;
            stepTime = parent.defaultStepTime;

            footAnchor = footTarget.position;
            footAnchorRotation = footTarget.rotation;
            footLastAnchorRotation = footAnchorRotation;
            footNextAnchorRotation = footAnchorRotation;
            anchorRestLocalRotation = footTarget.localRotation;
            footRestLocalRotation = foot.localRotation;
            footColliderPosition = footCollider.localPosition;
        }

        public void Clear() {
            state = WalkingState.Idle;
        }

        public void LegUpdate() {
            footCollider.localPosition = footColliderPosition + new Vector3(0, -parent.crouchingMax * parent.crouching, 0);
            footTarget.position = footAnchor;
            footTarget.rotation = footAnchorRotation;

            switch (state) {
                case WalkingState.Idle:
                    // Foot is stationary, not having made a step.
                    if (Physics.SphereCast(footRaycastSource.position, radiusForRaycast, raycastDirection, out RaycastHit hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        // The desired foot position exists on the ground.

                        groundedState = GroundedState.Grounded;
                        standingOnRigidbody = hit.rigidbody;
                        standingOnPoint = hit.point;
                        standingOnNormal = hit.normal;
                        Debug.DrawLine(footRaycastSource.position, footRaycastSource.position + raycastDirection * raycastDistance, Color.red);
                        debugBall_HitPoint.gameObject.SetActive(true);
                        debugBall_currentAnchor.gameObject.SetActive(true);
                        debugBall_HitPoint.position = hit.point;
                        debugBall_currentAnchor.position = footAnchor;

                        // The foot is too far from its desired foot position. Start a step.
                        currentDistance = (foot.transform.position - hit.point).magnitude;
                        if ((currentDistance > parent.distanceBetweenSteps && otherLeg.state == WalkingState.Idle))/* || AnchorOutOfRange)*/ {
                            // Take a step. The Last anchor are where the foot is now. The Next anchor are where the desired foot position is now.
                            footLastAnchor = footTarget.position;
                            footLastAnchorRotation = footTarget.rotation;
                            footNextAnchor = hit.point;
                            state = WalkingState.Stepping;
                            tInStep = 0;
                        }
                    } else {
                        // The foot is too far from the ground. It is considered airborne

                        groundedState = GroundedState.Airborne;
                        standingOnRigidbody = null;
                        debugBall_HitPoint.gameObject.SetActive(false);
                        debugBall_currentAnchor.gameObject.SetActive(false);
                    }
                    break;
                case WalkingState.Stepping:
                    // The foot is currently mid-step.
                    if (Physics.SphereCast(footRaycastSource.position, radiusForRaycast, raycastDirection, out hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        // The desired foot position still exists. Update where the foot's moving to reflect this new desired position.
                        groundedState = GroundedState.Grounded;
                        standingOnRigidbody = hit.rigidbody;
                        standingOnPoint = hit.point;
                        standingOnNormal = hit.normal;

                        // Lerp the old foot anchor to the new one.
                        Vector3 dir = hit.point - footNextAnchor;
                        float delta = Time.deltaTime * parent.stepToTargetPositonDelta;
                        if (dir.magnitude > delta)
                            footNextAnchor = footNextAnchor + dir.normalized * Time.deltaTime * parent.stepToTargetPositonDelta;
                        else
                            footNextAnchor = hit.point;
                        // Calculate the foot rotation for the new ground normal.
                        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.waist.rotation * anchorRestLocalRotation;
                        // Lerp the old foot desired rotation to the new desired rotation.
                        footNextAnchorRotation = Quaternion.Slerp(footNextAnchorRotation, targetRotation, Time.deltaTime * parent.stepToTargetRotationLerpFactor);
                        currentDistance = (foot.transform.position - hit.point).magnitude;

                        // Debug
                        Debug.DrawLine(footRaycastSource.position, footRaycastSource.position + raycastDirection * raycastDistance, Color.yellow);
                        debugBall_HitPoint.gameObject.SetActive(true);
                        debugBall_currentAnchor.gameObject.SetActive(true);
                        debugBall_HitPoint.position = hit.point;
                        debugBall_HitPoint.rotation = targetRotation;
                        debugBall_currentAnchor.position = footNextAnchor;
                        debugBall_currentAnchor.rotation = footNextAnchorRotation;
                    } else {
                        // A new desired position couldn't be found. Keep moving towards the old one.
                        groundedState = GroundedState.Airborne;
                        standingOnRigidbody = null;
                        debugBall_HitPoint.gameObject.SetActive(false);
                        debugBall_currentAnchor.gameObject.SetActive(false);
                    }

                    // Set leg position along parabola between anchors
                    if (currentDistance > parent.distanceBetweenSteps) {
                        tInStep += Time.deltaTime / stepTime * currentDistance / parent.distanceBetweenSteps;
                    } else {
                        tInStep += Time.deltaTime / stepTime;
                    }

                    if (tInStep >= 1) {
                        // Done stepping
                        footAnchor = footNextAnchor;
                        footAnchorRotation = footNextAnchorRotation;
                        state = WalkingState.Idle;
                    } else {
                        float y = (-2 * (tInStep - 0.5f) * (tInStep - 0.5f) + 0.5f) * 2;

                        Vector3 pos = footLastAnchor + tInStep * (footNextAnchor - footLastAnchor);
                        pos.y += y * parent.stepHeight;
                        // Set the foot anchor position to be along that parabola
                        footAnchor = pos;
                        // Set the foot to rotate side-to-side such that it starts facing where it did when last touching the ground
                        // and it will face where it should when it lands on the ground...
                        Quaternion floorFootRotation = Quaternion.Slerp(footLastAnchorRotation, footNextAnchorRotation, tInStep);
                        // ...and set the foot's pitch such that it's flat when on the ground but kicks off when in the air
                        y = Mathf.Sqrt(y);
                        footAnchorRotation = Quaternion.Slerp(floorFootRotation, calf.rotation * footRestLocalRotation, y);
                        //Debug.Log(calf.rotation +", " + footAnchorRotation  + ", " + floorFootRotation +", " + legRotation);
                    }
                    break;
            }
        }

        /// <summary>
        /// Gets the angle between the waist and the foot, where 0 degrees would be the leg directly beneath the waist
        /// </summary>
        /// <returns>the angle</returns>
        public float GetAngle() {
            float x = foot.position.y - thigh.position.y;


            Vector3 V = foot.position - thigh.position;
            V = Vector3.Project(V, parent.waist.forward);
            V.y = 0;
            float y = V.magnitude * Mathf.Sign(Vector3.Dot(V, parent.waist.forward));
            float angle = Mathf.Atan2(y, -x) * Mathf.Rad2Deg;
            //Debug.Log(angle + ", " + V + "," + x );
            return angle;
        }
    }

    [System.Serializable]
    private class Arm {

        private KogAnimation parent;

        [SerializeField]
        private Transform shoulder = null, forearm = null, handAnchor = null;

        private Leg leg;

        public void Initialize(KogAnimation parent, Leg leg) {
            this.parent = parent;
            this.leg = leg;
        }

        public void ArmUpdate() {
            handAnchor.position = shoulder.position + Quaternion.AngleAxis(leg.GetAngle(), parent.waist.right) * Vector3.down * parent.armHeight;
            handAnchor.rotation = forearm.rotation;
        }
    }
}
