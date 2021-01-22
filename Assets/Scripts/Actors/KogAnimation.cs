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
    public float waistBobMax = 0.2f, waistBobLegAngleMax = 90, waistBobLerp = 12, waistFallLerp = 10;
    public float leaningMax = 25f;
    public float crouchingMax = 0.035f;
    public float movingLeanMax = 15f;
    public float movingSpeedCrouchMax = .4f;
    public float crouchingLegSpreadMax = 0.25f, crouchingLegPoleSpreadMax = 0.5f;
    public float sprintingCrouchMax = 0.5f;
    public float sprintingCrouchLerp = 10;
    public float sprintingSpeedRatioThreshold = 0.5f;
    public float defaultRaycastDistance = 2;
    public float defaultDistanceBetweenSteps = .25f, distancePerStepSpeedFactor = 0.25f;
    public float defaultStepTime = 0.3f, stepTimeSpeedFactor = 2;
    public float stepHeight = .5f;
    public float legForwardsFactor = 0.1f;
    public float stepToTargetPositonDelta = 10;
    public float stepToTargetRotationLerpFactor = 10;
    public float armHeight = 1.25f;
    public float stepTime_h = 1.27f;
    public float stepTime_a = -0.05f;
    public float stepTime_b = 1.5f;
    public float stepTime_c = 1.93f;
    public float distance_h = -0.65f;
    public float distance_a = 2.35f;
    public float distance_b = -0.1f;
    #endregion

    private enum GroundedState { Grounded, Airborne }

    public bool IsGrounded => leftLeg.groundedState == GroundedState.Grounded || rightLeg.groundedState == GroundedState.Grounded;
    public Rigidbody StandingOnRigidbody => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnRigidbody : rightLeg.standingOnRigidbody;
    public Vector3 StandingOnPoint => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnPoint : rightLeg.standingOnPoint;
    public Vector3 StandingOnNormal => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnNormal : rightLeg.standingOnNormal;

    private bool IsSprinting => speedRatio > sprintingSpeedRatioThreshold;
    private float TopSpeed => Kog.MovementController.topSpeedSprinting;

    private Quaternion leftShoulderRestRotation, rightShoulderRestRotation;
    private Vector3 waistRestPosition;
    private float legRestLength;

    public float currentSpeed, stepTime, speedRatio;
    public float distanceBetweenSteps = .25f;
    private float speedForCrouching;

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
    private Transform waist = null, waistAnchor = null, head = null, headAnchor = null;
    [SerializeField]
    private CapsuleCollider bodyCollider = null;
    [SerializeField]
    private SphereCollider lifterCollider = null;

    void Start() {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInParent<Rigidbody>();

        waistRestPosition = waist.localPosition;
        leftShoulderRestRotation = leftArm.shoulder.rotation;
        rightShoulderRestRotation = rightArm.shoulder.rotation;
        legRestLength = Vector3.Distance(leftLeg.foot.position, leftLeg.thigh.position);

        leftLeg.Initialize(this, rightLeg);
        rightLeg.Initialize(this, leftLeg);
        leftArm.Initialize(this, leftLeg);
        rightArm.Initialize(this, rightLeg);

        //anim.Play("Armature|I-Pose");
    }

    public void Clear() {
        leftLeg.Clear();
        rightLeg.Clear();
        speedForCrouching = 0;
    }

    public void SetLegTarget(Vector3 movement, float currentSpeed) {
        speedRatio = currentSpeed / TopSpeed;
        if (speedRatio < 0.01f)
            speedRatio = 0;


        Vector3 crouchSpread = waist.right * crouchingLegSpreadMax * crouching;
        Vector3 legForwardsMovement = Vector3.down + movement * legForwardsFactor;
        Vector3 leftMove = (-crouchSpread + legForwardsMovement) * defaultRaycastDistance;
        Vector3 rightMove = (crouchSpread + legForwardsMovement) * defaultRaycastDistance;

        float legPoleOffset = crouching * crouchingLegPoleSpreadMax;
        Vector3 leftLegPoleOffset = leftLeg.footPoleRestLocalPosition + Vector3.left * legPoleOffset;
        Vector3 rightLegPoleOffset = rightLeg.footPoleRestLocalPosition + Vector3.right * legPoleOffset;
        leftLeg.footTargetPole.localPosition = leftLegPoleOffset;
        rightLeg.footTargetPole.localPosition = rightLegPoleOffset;

        float length = leftMove.magnitude;
        leftLeg.raycastDirection = leftMove.normalized;
        rightLeg.raycastDirection = rightMove.normalized;
        leftLeg.raycastDistance = length;
        rightLeg.raycastDistance = length;
        this.currentSpeed = currentSpeed;

        //stepTime = defaultStepTime + speedRatio * stepTimeSpeedFactor;

        distanceBetweenSteps = defaultDistanceBetweenSteps + (speedRatio * distancePerStepSpeedFactor);

        stepTime = stepTime_h * (speedRatio - stepTime_a) * (speedRatio - stepTime_b) * (speedRatio - stepTime_c);
        distanceBetweenSteps = distance_h * (speedRatio - distance_a) * (speedRatio - distance_b);


        Debug.Log("speed: " + speedRatio + ", " + stepTime + ", distance: " + distanceBetweenSteps);
    }

    private void OnAnimatorMove() {

        // Set crouching amount
        if (Kog.MovementController.State == KogMovementController.WalkingState.Anchored)
            crouching = movingSpeedCrouchMax;
        else
            crouching = 0;


        // Rotate the waist to reflect the current velocity, like you're leaning into the movement
        Vector3 movement = rb.velocity;
        movement = waist.parent.InverseTransformDirection(movement);
        movement.y = 0;
        float speedRatio = movement.magnitude / TopSpeed;
        if (speedRatio > 1)
            speedRatio = 1;
        else if (speedRatio < 0.05f)
            movement = Vector3.forward;
        Vector3 cross = Vector3.Cross(transform.up, movement);
        Quaternion waistRotationFromSpeed = Quaternion.AngleAxis(speedRatio * leaningMax + crouching * movingLeanMax, cross);

        // Rotate the waist to reflect the positions of the legs and feet.
        // The angle formed by the feet should be opposite of the angle formed by the waist.
        float feetAngle = IMath.AngleBetween_Signed(rightLeg.foot.position - leftLeg.foot.position, waist.parent.right, transform.up, false) * Mathf.Rad2Deg * waistLegRotationFactor;
        Quaternion waistRotationFromLegs = Quaternion.AngleAxis(feetAngle, transform.up);
        // Apply the final rotation
        Quaternion desiredWaistRotation = waist.parent.rotation * waistRotationFromSpeed * waistRotationFromLegs;
        waistAnchor.rotation = Quaternion.Slerp(waistAnchor.rotation, desiredWaistRotation, 1 - Mathf.Exp(-waistReactionTime * Time.deltaTime));
        // Also rotate the shoulders or torso to oppose that
        //leftArm.shoulderAnchor.rotation = leftArm.shoulder.parent.rotation * leftShoulderRestRotation * waistRotationFromLegs;
        //rightArm.shoulderAnchor.rotation = rightArm.shoulder.parent.rotation * rightShoulderRestRotation * waistRotationFromLegs;

        // Set the height of the waist so that the whole body bobs with your leg movement
        // get the angle of the leg that's the most bent
        float legAngle = waistBobLegAngleMax;
        float leftAngle = leftLeg.calf.localEulerAngles.x;
        float rightAngle = rightLeg.calf.localEulerAngles.x;
        if (leftAngle > 180)
            leftAngle -= 360;
        if (rightAngle > 180)
            rightAngle -= 360;
        if (leftAngle < legAngle)
            legAngle = leftAngle;
        if (rightAngle < legAngle)
            legAngle = rightAngle;
        //Debug.Log(legAngle + "   left: " + legAngle + " right: " + rightAngle);
        float waistBobAmount = -legAngle / waistBobLegAngleMax * waistBobMax;

        //// Lower the waist if a foot could be touching its anchor but it is too far vertically
        //float footToAnchorHeightLeft = Vector3.Distance(leftLeg.foot.position, leftLeg.footAnchor);
        //float footToAnchorHeightRight = Vector3.Distance(rightLeg.foot.position, rightLeg.footAnchor);
        //float footToAnchorHeight = Mathf.Max(footToAnchorHeightLeft, footToAnchorHeightRight);
        ////footToAnchorHeight -= 0.01f;
        //if (footToAnchorHeight < 0) {
        //    footToAnchorHeight = 0;
        //}
        //Debug.Log(footToAnchorHeight);
        ////Debug.Log("Waist old height " + pos.y);
        ////pos.y = Mathf.Lerp(pos.y, pos.y + waistBobAmount, Time.deltaTime * waistBobLerp);
        //pos.y = pos.y - footToAnchorHeight;
        ////waistAnchor.transform.position = pos;
        ////Debug.Log("Waist new height " + waistAnchor.transform.position.y);
        //lifterCollider.center = new Vector3(0, footToAnchorHeight, 0);

        /*
        float legMaxLength = legRestLength * (1 - crouching * .5f);
        float height = 0;
        if (leftLeg.state == Leg.WalkingState.Support && leftLeg.groundedState == GroundedState.Grounded) {
            Vector3 leftThighToFoot = leftLeg.foot.position - leftLeg.thigh.position;
            if (Physics.Raycast(leftLeg.thigh.position, leftThighToFoot, out RaycastHit hit, 1000, GameManager.Layer_IgnorePlayer)) {

                Vector3 cast = hit.point - leftLeg.thigh.position;
                height = cast.magnitude - legMaxLength;
                //Debug.Log("Hit length: " + hit.distance + ", thigh to foot: " + leftThighToFoot.magnitude + ", height: " + height + ", waist: " + pos.y);

            }
        }

        if (rightLeg.state == Leg.WalkingState.Support && rightLeg.groundedState == GroundedState.Grounded) {
            Vector3 rightThighToFoot = rightLeg.foot.position - rightLeg.thigh.position;
            if (Physics.Raycast(rightLeg.thigh.position, rightThighToFoot, out RaycastHit hit, 1000, GameManager.Layer_IgnorePlayer)) {

                Vector3 cast = hit.point - rightLeg.thigh.position;
                float rightHeight = cast.magnitude - legMaxLength;
                //Debug.Log("Hit length: " + hit.distance + ", thigh to foot: " + rightThighToFoot.magnitude + ", height: " + rightHeight + ", waist: " + pos.y);
                if (rightHeight > height)
                    height = rightHeight;
            }
        }
        height = Mathf.Clamp(height, -legMaxLength / 2, legMaxLength / 2);
        
        if (height != 0) {
            height = -height * Time.deltaTime * waistFallLerp;
        }
        */
        Vector3 pos = waist.position;
        float height = 0;

        waistBobAmount = 0;
        pos.y = Mathf.Lerp(pos.y, pos.y + waistBobAmount + height, Time.deltaTime * waistBobLerp);
        waistAnchor.position = pos;
        // Crouching
        //crouching = speedRatio;
        speedForCrouching = Mathf.Lerp(speedForCrouching, speedRatio * sprintingCrouchMax, Time.deltaTime * sprintingCrouchLerp);
        bodyCollider.transform.localPosition = new Vector3(0, crouching * crouchingMax + speedForCrouching, 0);

        leftLeg.LegUpdate();
        rightLeg.LegUpdate();
        leftArm.ArmUpdate();
        rightArm.ArmUpdate();

        //// Lower the lifter to keep both Idle feet where their anchors are
        //// Set the lifter's position to be at the same height as the heighest Idle foot.
        //Vector3 pos = transform.position;

        //float heighest = 0;
        //if (leftLeg.state == Leg.WalkingState.Support) {
        //    float height = Vector3.Distance(leftLeg.foot.position, leftLeg.footAnchor);
        //    Debug.Log("Left leg height: " + height);
        //    if (height > heighest)
        //        heighest = height;
        //}
        //if (rightLeg.state == Leg.WalkingState.Support) {
        //    float height = Vector3.Distance(rightLeg.foot.position, rightLeg.footAnchor);
        //    Debug.Log("Right leg height: " + height);
        //    if (height > heighest)
        //        heighest = height;
        //}
        //pos.y += lifterCollider.radius - heighest;
        //lifterCollider.transform.position = pos;
    }

    [System.Serializable]
    private class Leg {

        public enum WalkingState { Support, Floating }

        public WalkingState state = WalkingState.Support;
        public GroundedState groundedState = GroundedState.Grounded;
        public Rigidbody standingOnRigidbody = null;
        public Vector3 standingOnPoint = Vector3.zero;
        public Vector3 standingOnNormal = Vector3.zero;
        public Vector3 raycastDirection = Vector3.zero;
        public float raycastDistance;

        [SerializeField]
        public Transform foot = null, thigh = null, calf = null;
        [SerializeField]
        public Transform footTarget = null, footTargetPole = null;
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
        public Vector3 footPoleRestLocalPosition;

        private float currentDistance = 0;
        private float tInStep = 0;

        public void Initialize(KogAnimation parent, Leg otherLeg) {
            this.otherLeg = otherLeg;
            this.parent = parent;
            raycastDistance = parent.defaultRaycastDistance;

            footAnchor = footTarget.position;
            footAnchorRotation = footTarget.rotation;
            footLastAnchorRotation = footAnchorRotation;
            footNextAnchorRotation = footAnchorRotation;
            anchorRestLocalRotation = footTarget.localRotation;
            footRestLocalRotation = foot.localRotation;
            footColliderPosition = footCollider.localPosition;
            footPoleRestLocalPosition = footTargetPole.localPosition;
        }

        public void Clear() {
            state = WalkingState.Support;
        }

        public void LegUpdate() {
            footCollider.localPosition = footColliderPosition + new Vector3(0, -parent.crouchingMax * parent.crouching, 0);
            footTarget.position = footAnchor;
            footTarget.rotation = footAnchorRotation;

            switch (state) {
                case WalkingState.Support:
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
                        // Keep one foot supporting at all times - unless moving quickly and this leg has reached the end of its propulsion (i.e. is stretched)
                        currentDistance = (foot.transform.position - hit.point).magnitude;
                        bool isStretched = parent.legRestLength - Vector3.Distance(thigh.position, foot.position) < 0.01f;
                        if (currentDistance > parent.distanceBetweenSteps && (otherLeg.state == WalkingState.Support || parent.IsSprinting && isStretched)) {
                            // Take a step. The Last anchor are where the foot is now. The Next anchor are where the desired foot position is now.
                            footLastAnchor = footTarget.position;
                            footLastAnchorRotation = footTarget.rotation;
                            footNextAnchor = hit.point;
                            state = WalkingState.Floating;
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
                case WalkingState.Floating:
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
                        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.transform.rotation * anchorRestLocalRotation;
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
                    //if (currentDistance > parent.distanceBetweenSteps) {
                    //    tInStep += Time.deltaTime / stepTime * currentDistance / parent.distanceBetweenSteps;
                    //} else {
                    tInStep += Time.deltaTime / parent.stepTime;
                    //}

                    if (tInStep >= 1) {
                        // Done stepping
                        footAnchor = footNextAnchor;
                        footAnchorRotation = footNextAnchorRotation;
                        state = WalkingState.Support;
                    } else {
                        float y = Mathf.Sqrt((-2 * (tInStep - 0.5f) * (tInStep - 0.5f) + 0.5f) * 2);

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
        public Transform shoulder = null, forearm = null, handAnchor = null, handAnchorPole = null;

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
