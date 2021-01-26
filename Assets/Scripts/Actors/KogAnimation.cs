﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Controls the scripted/procedural animation for Kog.
/// Includes the leg and arm animations while walking, sprinting, and anchored.
/// Controls inverse kinematics for limbs.
/// </summary>
public class KogAnimation : MonoBehaviour {

    private enum AnimationState { Keyframed, Blending, Scripted }

    public bool IsGrounded => leftLeg.walkingState != Leg.WalkingState.Airborne || rightLeg.walkingState != Leg.WalkingState.Airborne;
    public Rigidbody StandingOnRigidbody => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnRigidbody : rightLeg.standingOnRigidbody;
    public Vector3 StandingOnPoint => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnPoint : rightLeg.standingOnPoint;
    public Vector3 StandingOnNormal => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnNormal : rightLeg.standingOnNormal;

    private bool IsRunning => speedRatio > kogAnimation_SO.Sprint_speedRatioThreshold;
    private float TopSpeed => Kog.MovementController.topSpeedSprinting;


    private Quaternion leftShoulderRestRotation, rightShoulderRestRotation;
    private Vector3 waistRestPosition;
    private float legRestLength;

    private AnimationState animationState;
    public float currentSpeed, stepTime, speedRatio, stepHeight;
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
    [SerializeField]
    private KogAnimation_SO kogAnimation_SO = null;
    [SerializeField, Range(0.0f, 1.0f)]
    private float crouching = 0;
    [SerializeField]
    private Rig rig = null;
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
    private Vector3 headLookAtTarget = Vector3.zero;

    void Start() {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponentInParent<Rigidbody>();

        animationState = AnimationState.Keyframed;
        rig.weight = 0;
        waistRestPosition = waist.localPosition;
        leftShoulderRestRotation = leftArm.shoulder.rotation;
        rightShoulderRestRotation = rightArm.shoulder.rotation;
        legRestLength = Vector3.Distance(leftLeg.foot.position, leftLeg.thigh.position);

        leftLeg.Initialize(this, kogAnimation_SO, rightLeg, true);
        rightLeg.Initialize(this, kogAnimation_SO, leftLeg, false);
        leftArm.Initialize(kogAnimation_SO, leftLeg, true);
        rightArm.Initialize(kogAnimation_SO, rightLeg, false);

        //anim.Play("Armature|I-Pose");
    }

    public void Clear() {
        leftLeg.Clear();
        rightLeg.Clear();
        speedForCrouching = 0;
    }

    private void Update() {
        // Blend between keyframed and scripted animations, depending on different factors
        switch (animationState) {
            case AnimationState.Keyframed:
                if (Kog.MovementController.State == KogMovementController.WalkingState.Anchored
                    || speedRatio > 0) {
                    animationState = AnimationState.Blending;
                }
                break;
            case AnimationState.Blending:
                // Increase or decrease the current weight of all scripted animation controllers
                // towards scripted or keyframed animations
                if (Kog.MovementController.State == KogMovementController.WalkingState.Anchored
                    || speedRatio > 0) {
                    rig.weight += Time.deltaTime / kogAnimation_SO.KeyframeToScriptingTime;
                    // If at 0% or 100%, enter that state
                    if (rig.weight >= 1) {
                        rig.weight = 1;
                        animationState = AnimationState.Scripted;
                    }
                } else {
                    rig.weight -= Time.deltaTime / kogAnimation_SO.ScriptingToKeyframeTime;
                    if (rig.weight <= 0) {
                        rig.weight = 0;
                        animationState = AnimationState.Keyframed;
                    }
                }

                break;
            case AnimationState.Scripted:
                if (Kog.MovementController.State == KogMovementController.WalkingState.Idle
                    && speedRatio == 0) {
                    animationState = AnimationState.Blending;
                }
                break;
        }
    }

    public void SetLegTarget(Vector3 movement, float currentSpeed) {
        speedRatio = currentSpeed / TopSpeed;
        if (speedRatio < 0.01f)
            speedRatio = 0;


        Vector3 crouchSpread = waist.right * kogAnimation_SO.Crouch_legSpreadMax * crouching;
        Vector3 legForwardsMovement = Vector3.down * legRestLength + movement * kogAnimation_SO.Leg_forwards_withSpeed;
        Vector3 leftMove = legForwardsMovement - crouchSpread;
        Vector3 rightMove = legForwardsMovement + crouchSpread;

        float legPoleOffset = crouching * kogAnimation_SO.Crouch_LegPoleSpreadMax;
        Vector3 leftLegPoleOffset = leftLeg.footPoleRestLocalPosition + Vector3.left * legPoleOffset;
        Vector3 rightLegPoleOffset = rightLeg.footPoleRestLocalPosition + Vector3.right * legPoleOffset;
        leftLeg.footTargetPole.localPosition = leftLegPoleOffset;
        rightLeg.footTargetPole.localPosition = rightLegPoleOffset;

        leftLeg.raycastDirection = leftMove.normalized;
        rightLeg.raycastDirection = rightMove.normalized;
        leftLeg.raycastDistance = leftMove.magnitude;
        rightLeg.raycastDistance = rightMove.magnitude;
        this.currentSpeed = currentSpeed;

        //stepTime = step_defaultTime + speedRatio * step_time_withSpeed;

        distanceBetweenSteps = kogAnimation_SO.Step_defaultDistance + (speedRatio * kogAnimation_SO.Step_distance_withSpeed);

        stepTime = kogAnimation_SO.StepTime_h * (speedRatio - kogAnimation_SO.StepTime_a) * (speedRatio - kogAnimation_SO.StepTime_b) * (speedRatio - kogAnimation_SO.StepTime_c);
        distanceBetweenSteps = kogAnimation_SO.Distance_h * (speedRatio - kogAnimation_SO.Distance_a) * (speedRatio - kogAnimation_SO.Distance_b);


        //Debug.Log("speed: " + speedRatio + ", " + stepTime + ", distance: " + distanceBetweenSteps);

        // set head target to be in front of movement
        if (movement.sqrMagnitude > 0f) {
            headLookAtTarget = head.position + movement.normalized * 10;
        }
    }

    private void TrackHead() {
        Vector3 towardHeadTarget = headLookAtTarget - head.position;

        head.localRotation = Quaternion.identity;
        Vector3 localLookDirection = head.InverseTransformDirection(towardHeadTarget);

        Quaternion localLookRotation = Quaternion.LookRotation(localLookDirection, transform.up);

        // Limit the angle of rotation
        Vector3 eulers = localLookRotation.eulerAngles;
        if (eulers.x > 180)
            eulers.x -= 360;
        if (eulers.y > 180)
            eulers.y -= 360;
        if (eulers.z > 180)
            eulers.z -= 360;
        eulers.x = Mathf.Clamp(eulers.x, kogAnimation_SO.HeadMinX, kogAnimation_SO.HeadMaxX);
        eulers.y = Mathf.Clamp(eulers.y, kogAnimation_SO.HeadMinY, kogAnimation_SO.HeadMaxY);
        eulers.z = Mathf.Clamp(eulers.z, kogAnimation_SO.HeadMinZ, kogAnimation_SO.HeadMaxZ);

        localLookRotation = Quaternion.Euler(eulers);

        Quaternion targetRotation = head.parent.rotation * localLookRotation;

        headAnchor.rotation = Quaternion.Slerp(headAnchor.rotation, targetRotation, 1 - Mathf.Exp(-kogAnimation_SO.Head_lookAt_lerp * Time.deltaTime));
    }

    private void OnAnimatorMove() {
        // Get the current movement relative to the the body's forward direction
        Vector3 movement = rb.velocity;
        movement = waist.parent.InverseTransformDirection(movement);
        movement.y = 0;

        // Get the current speed ratio (0 for not moving, 1 for max speed)
        float speedRatio = movement.magnitude / TopSpeed;
        if (speedRatio > 1)
            speedRatio = 1;
        else if (speedRatio < 0.05f)
            movement = Vector3.forward;

        // Set crouching amount and step height
        if (Kog.MovementController.State == KogMovementController.WalkingState.Anchored) {
            crouching = kogAnimation_SO.Crouch_anchored_amount;
            stepHeight = kogAnimation_SO.Crouch_anchored_stepHeight;
        } else {
            crouching = 0;
            stepHeight = kogAnimation_SO.Step_defaultHeight + speedRatio * kogAnimation_SO.Move_height_withSpeed;
        }

        // Rotate the waist to reflect the current velocity, like you're leaning into the movement
        Vector3 cross = Vector3.Cross(transform.up, movement);
        float waistDownAngle = speedRatio * kogAnimation_SO.Waist_lean_withSpeed + crouching * kogAnimation_SO.Waist_lean_withCrouch;
        if (Kog.MovementController.State == KogMovementController.WalkingState.Sprinting)
            waistDownAngle += kogAnimation_SO.Waist_lean_withSprinting;
        Quaternion waistRotationFromSpeed = Quaternion.AngleAxis(waistDownAngle, cross);

        // Rotate the waist to reflect the positions of the legs and feet.
        // The angle formed by the feet should be opposite of the angle formed by the waist.
        float feetAngle = IMath.AngleBetween_Signed(rightLeg.foot.position - leftLeg.foot.position, waist.parent.right, transform.up, false) * Mathf.Rad2Deg * kogAnimation_SO.Waist_rotate_withLegAngle;
        Quaternion waistRotationFromLegs = Quaternion.AngleAxis(feetAngle, transform.up);
        // Apply the final rotation
        Quaternion desiredWaistRotation = waist.parent.rotation * waistRotationFromSpeed * waistRotationFromLegs;
        waistAnchor.rotation = Quaternion.Slerp(waistAnchor.rotation, desiredWaistRotation, 1 - Mathf.Exp(-kogAnimation_SO.Waist_rotate_lerp * Time.deltaTime));
        // Also rotate the shoulders or torso to oppose that
        //leftArm.shoulderAnchor.rotation = leftArm.shoulder.parent.rotation * leftShoulderRestRotation * waistRotationFromLegs;
        //rightArm.shoulderAnchor.rotation = rightArm.shoulder.parent.rotation * rightShoulderRestRotation * waistRotationFromLegs;

        // Set the height of the waist so that the whole body bobs with your leg movement, bobbing more at higher speed
        // get the angle of the leg that's the most bent
        float legAngle = kogAnimation_SO.Waist_bob_legAngleMax;
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
        float waistBobAmount = -legAngle / kogAnimation_SO.Waist_bob_legAngleMax * (kogAnimation_SO.Waist_bob + speedRatio * kogAnimation_SO.Waist_bob_withSpeed);


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
        ////pos.y = Mathf.Lerp(pos.y, pos.y + waistBobAmount, Time.deltaTime * waist_bob_lerp);
        //pos.y = pos.y - footToAnchorHeight;
        ////waistAnchor.transform.position = pos;
        ////Debug.Log("Waist new height " + waistAnchor.transform.position.y);
        //lifterCollider.center = new Vector3(0, footToAnchorHeight, 0);

        /*
        float legMaxLength = legRestLength * (1 - crouching * .5f);
        float height = 0;
        if (leftLeg.walkingState == Leg.WalkingState.Support && leftLeg.groundedState == GroundedState.Grounded) {
            Vector3 leftThighToFoot = leftLeg.foot.position - leftLeg.thigh.position;
            if (Physics.Raycast(leftLeg.thigh.position, leftThighToFoot, out RaycastHit hit, 1000, GameManager.Layer_IgnorePlayer)) {

                Vector3 cast = hit.point - leftLeg.thigh.position;
                height = cast.magnitude - legMaxLength;
                //Debug.Log("Hit length: " + hit.distance + ", thigh to foot: " + leftThighToFoot.magnitude + ", height: " + height + ", waist: " + pos.y);

            }
        }

        if (rightLeg.walkingState == Leg.WalkingState.Support && rightLeg.groundedState == GroundedState.Grounded) {
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
            height = -height * Time.deltaTime * waist_fall_lerp;
        }
        */
        Vector3 pos = waistRestPosition;
        //float height = 0;
        //Debug.Log(waistBobAmount);
        pos.y = pos.y + waistBobAmount;
        //pos.y = Mathf.Lerp(pos.y, pos.y + waistBobAmount + height, Time.deltaTime * waist_bob_lerp);
        waistAnchor.position = Vector3.Lerp(waistAnchor.position, waist.parent.position + pos, Time.deltaTime * kogAnimation_SO.Waist_bob_lerp); ;
        // Crouching
        //crouching = speedRatio;
        speedForCrouching = Mathf.Lerp(speedForCrouching, speedRatio * kogAnimation_SO.Move_crouch_withSpeed, Time.deltaTime * kogAnimation_SO.Move_crouch_lerp);
        bodyCollider.transform.localPosition = new Vector3(0, crouching * kogAnimation_SO.Crouch_max + speedForCrouching, 0);

        leftLeg.LegUpdate();
        rightLeg.LegUpdate();
        leftArm.ArmUpdate(speedRatio, waist);
        rightArm.ArmUpdate(speedRatio, waist);

        TrackHead();

        //// Lower the lifter to keep both Idle feet where their anchors are
        //// Set the lifter's position to be at the same height as the heighest Idle foot.
        //Vector3 pos = transform.position;

        //float heighest = 0;
        //if (leftLeg.walkingState == Leg.WalkingState.Support) {
        //    float height = Vector3.Distance(leftLeg.foot.position, leftLeg.footAnchor);
        //    Debug.Log("Left leg height: " + height);
        //    if (height > heighest)
        //        heighest = height;
        //}
        //if (rightLeg.walkingState == Leg.WalkingState.Support) {
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

        public enum WalkingState { Support, Floating, Airborne }
        //public enum GroundedState { Grounded, Airborne }

        public WalkingState walkingState = WalkingState.Support;
        //public GroundedState groundedState = GroundedState.Grounded;
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

        private bool AnchorOutOfRange => (foot.position - footAnchor).magnitude > 0.05f;

        private KogAnimation parent;
        private KogAnimation_SO kogAnimation_SO;
        private Leg otherLeg;
        private bool isLeft;
        public Vector3 footAnchor = Vector3.zero, footLastAnchor = Vector3.zero, footNextAnchor = Vector3.zero;
        private Quaternion footAnchorRotation, footLastAnchorRotation, footNextAnchorRotation;
        private Quaternion anchorRestLocalRotation;
        private Quaternion footRestLocalRotation;
        private Vector3 footColliderPosition;
        public Vector3 footPoleRestLocalPosition;

        private float tInStep = 0;

        public void Initialize(KogAnimation parent, KogAnimation_SO kogAnimation_SO, Leg otherLeg, bool isLeft) {
            this.parent = parent;
            this.kogAnimation_SO = kogAnimation_SO;
            this.otherLeg = otherLeg;
            this.isLeft = isLeft;

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
            walkingState = WalkingState.Support;
        }

        public void LegUpdate() {
            footCollider.localPosition = footColliderPosition + new Vector3(0, -kogAnimation_SO.Crouch_max * parent.crouching, 0);

            switch (walkingState) {
                case WalkingState.Support:
                    // Foot is stationary, not having made a step.
                    if (Physics.SphereCast(footRaycastSource.position, kogAnimation_SO.Leg_raycast_radius, raycastDirection, out RaycastHit hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        // The desired foot position exists on the ground.

                        standingOnRigidbody = hit.rigidbody;
                        standingOnPoint = hit.point;
                        standingOnNormal = hit.normal;
                    } else {
                        // The foot is too far from the ground, even though it should be supporting. It is considered airborne
                        // Take a step towards a where the foot would go if there were ground there.

                        walkingState = WalkingState.Airborne;
                        standingOnRigidbody = null;
                        hit.point = footRaycastSource.position + raycastDirection * kogAnimation_SO.Leg_airborne_length;
                    }

                    // The foot is too far from its desired foot position. Start a step.
                    // Keep one foot supporting at all times - unless moving quickly and this leg has reached the end of its propulsion (i.e. is stretched)
                    Vector3 hitToFoot = foot.position - hit.point;
                    hitToFoot.y = 0;
                    float calfAngle = calf.localEulerAngles.x;
                    if (calfAngle > 180)
                        calfAngle -= 360;
                    bool isStretched = calfAngle < 0.01f;
                    //Debug.Log("Stretched:  " + isStretched +  ", "  + calf.localEulerAngles.x, foot.gameObject);
                    if (hitToFoot.magnitude > parent.distanceBetweenSteps
                                && (otherLeg.walkingState == WalkingState.Support || parent.IsRunning && isStretched && otherLeg.tInStep > kogAnimation_SO.Leg_kickoff_tInStep)
                    ) {
                        // Take a step. The Last anchor are where the foot is now. The Next anchor are where the desired foot position is now.
                        footLastAnchor = footTarget.position;
                        footLastAnchorRotation = footTarget.rotation;
                        footNextAnchor = hit.point;
                        walkingState = WalkingState.Floating;
                        tInStep = 0;
                    }

                    // Debug
                    Debug.DrawLine(footRaycastSource.position, footRaycastSource.position + raycastDirection * raycastDistance, Color.red);
                    debugBall_HitPoint.gameObject.SetActive(true);
                    debugBall_currentAnchor.gameObject.SetActive(true);
                    debugBall_HitPoint.position = hit.point;
                    debugBall_currentAnchor.position = footAnchor;
                    Debug.DrawLine(hit.point, hit.point + hitToFoot.normalized * parent.distanceBetweenSteps);

                    break;
                case WalkingState.Floating:
                    // The foot is currently mid-step.
                    if (Physics.SphereCast(footRaycastSource.position, kogAnimation_SO.Leg_raycast_radius, raycastDirection, out hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        // The desired foot position still exists. Update where the foot's moving to reflect this new desired position.
                        standingOnRigidbody = hit.rigidbody;
                        standingOnPoint = hit.point;
                        standingOnNormal = hit.normal;
                    } else {
                        // Now is airborner
                        walkingState = WalkingState.Airborne;
                        standingOnRigidbody = null;
                        hit.point = footRaycastSource.position + raycastDirection * kogAnimation_SO.Leg_airborne_length;
                    }

                    // Lerp the old foot anchor to the new one.
                    Vector3 dir = hit.point - footNextAnchor;
                    float delta = Time.deltaTime * kogAnimation_SO.Step_ToTarget_Delta;
                    if (dir.magnitude > delta)
                        footNextAnchor = footNextAnchor + dir.normalized * Time.deltaTime * kogAnimation_SO.Step_ToTarget_Delta;
                    else
                        footNextAnchor = hit.point;
                    // Calculate the foot rotation for the new ground normal.
                    Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.transform.rotation * anchorRestLocalRotation;
                    // Lerp the old foot desired rotation to the new desired rotation.
                    footNextAnchorRotation = Quaternion.Slerp(footNextAnchorRotation, targetRotation, Time.deltaTime * kogAnimation_SO.Step_ToTargetRotation_lerp);
                    hitToFoot = foot.position - hit.point;
                    hitToFoot.y = 0;

                    // Debug
                    Debug.DrawLine(footRaycastSource.position, footRaycastSource.position + raycastDirection * raycastDistance, Color.yellow);
                    debugBall_HitPoint.gameObject.SetActive(true);
                    debugBall_currentAnchor.gameObject.SetActive(true);
                    debugBall_HitPoint.position = hit.point;
                    debugBall_HitPoint.rotation = targetRotation;
                    debugBall_currentAnchor.position = footNextAnchor;
                    debugBall_currentAnchor.rotation = footNextAnchorRotation;
                    Debug.DrawLine(hit.point, hit.point + hitToFoot.normalized * parent.distanceBetweenSteps);


                    tInStep += Time.deltaTime / parent.stepTime;

                    if (tInStep >= 1) {
                        // Done stepping
                        // If the foot would be in the air, maintain this position untila foot position could be found
                        footAnchor = footNextAnchor;
                        footAnchorRotation = footNextAnchorRotation;
                        walkingState = WalkingState.Support;
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

                case WalkingState.Airborne:
                    // The leg is airborne, falling through the air.
                    // If the other leg isn't leading yet, set this leg to lead, meaning it will move in front to brace for the landing.
                    // If this leg isn't leading, keep it closer to the back.

                    if (Physics.SphereCast(footRaycastSource.position, kogAnimation_SO.Leg_raycast_radius, raycastDirection, out hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        // The ground is close. move foot anchor towards it until we've landed.

                        // (tried doing this with the above spherecast distance but it was pretty inconsistent...
                        if (Physics.Raycast(foot.position, Vector3.down, out RaycastHit footHit, 1000, GameManager.Layer_IgnorePlayer)) {

                            if (footHit.distance < 0.02f) {
                                footAnchor = footNextAnchor;
                                footAnchorRotation = footNextAnchorRotation;
                                walkingState = WalkingState.Support;
                            }
                        }

                    } else {
                        // Still airborne
                        standingOnRigidbody = null;
                        hit.point = footRaycastSource.position + raycastDirection * kogAnimation_SO.Leg_airborne_length;
                    }

                    // Lerp the old foot anchor to the new one.
                    //dir = hit.point - footNextAnchor;
                    //delta = Time.deltaTime * parent.step_ToTarget_Delta;
                    //if (dir.magnitude > delta)
                    //    footNextAnchor = footNextAnchor + dir.normalized * Time.deltaTime * parent.step_ToTarget_Delta;
                    //else
                    footNextAnchor = hit.point;
                    // Calculate the foot rotation for the new ground normal.
                    targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.transform.rotation * anchorRestLocalRotation;
                    // Lerp the old foot desired rotation to the new desired rotation.
                    footNextAnchorRotation = Quaternion.Slerp(footNextAnchorRotation, targetRotation, Time.deltaTime * kogAnimation_SO.Step_ToTargetRotation_lerp);
                    hitToFoot = foot.position - hit.point;
                    hitToFoot.y = 0;

                    // Debug
                    Debug.DrawLine(footRaycastSource.position, footRaycastSource.position + raycastDirection * raycastDistance, Color.yellow);
                    debugBall_HitPoint.gameObject.SetActive(true);
                    debugBall_currentAnchor.gameObject.SetActive(true);
                    debugBall_HitPoint.position = hit.point;
                    debugBall_HitPoint.rotation = targetRotation;
                    debugBall_currentAnchor.position = footNextAnchor;
                    debugBall_currentAnchor.rotation = footNextAnchorRotation;
                    Debug.DrawLine(hit.point, hit.point + hitToFoot.normalized * parent.distanceBetweenSteps);

                    tInStep += Time.deltaTime / parent.stepTime;

                    if (tInStep >= 1) {
                        // Done stepping
                        // If the foot would be in the air, maintain this position until a foot position could be found
                        footAnchor = footNextAnchor;
                        footAnchorRotation = footNextAnchorRotation;
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
            footTarget.position = footAnchor;
            footTarget.rotation = footAnchorRotation;
        }

        /// <summary>
        /// Gets the angle between the waist and the foot, where 0 degrees would be the leg directly beneath the waist
        /// </summary>
        /// <returns>the angle in degrees</returns>
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

        [SerializeField]
        public Transform shoulder = null, forearm = null, upperarm = null, handAnchor = null, handAnchorPole = null;

        private KogAnimation_SO kogAnimation_SO;
        private Leg leg;
        private bool isLeft;

        public void Initialize(KogAnimation_SO kogAnimation_SO, Leg leg, bool isLeft) {
            this.leg = leg;
            this.isLeft = isLeft;
            this.kogAnimation_SO = kogAnimation_SO;
        }
        public void ArmUpdate(float speedRatio, Transform waist) {
            //handAnchor.position = shoulder.position + Quaternion.AngleAxis(leg.GetAngle(), parent.waist.right) * Vector3.down * Height;
            float X_d = kogAnimation_SO.ArmX_d;
            float X_f = kogAnimation_SO.ArmX_f;
            float X_g = kogAnimation_SO.ArmX_g;
            float Y_a = kogAnimation_SO.ArmY_a;
            float Y_b = kogAnimation_SO.ArmY_b;
            float Y_c = kogAnimation_SO.ArmY_c;
            float Z_o = kogAnimation_SO.ArmZ_o;
            float h = kogAnimation_SO.ArmPoleX_h;
            float j = kogAnimation_SO.ArmPoleX_j;
            float k = kogAnimation_SO.ArmPoleY_k;
            float l = kogAnimation_SO.ArmPoleY_l;
            float m = kogAnimation_SO.ArmPoleZ_m;
            float n = kogAnimation_SO.ArmPoleZ_n;
            if (isLeft) {
                X_d = -X_d;
                X_g = -X_g;
                h = -h;
            }
            // Position arm such that:
            // the hand anchor follows a 3D parobolic arc around the body that is a function of the leg angle
            // The length of the arm depends on the speed (higher speed = closer handAnchor)
            float t = leg.GetAngle();
            float X = X_d * (t - X_f) * (t - X_f) + X_g;
            float Y = Y_a * (t - Y_b) * (t - Y_b) + Y_c + (-kogAnimation_SO.ArmHeight - Y_c) * (1 - speedRatio); // When speed is 0, the arm should hang at armHeight
            float Z = Z_o * kogAnimation_SO.ArmHeight * Mathf.Sin(-leg.GetAngle() * Mathf.Deg2Rad);
            handAnchor.position = upperarm.transform.position + waist.TransformDirection(new Vector3(X, Y, Z));
            handAnchor.rotation = forearm.rotation;

            //Debug.Log("Angle: " + t + " XYZ: " + new Vector3(X, Y, Z));

            X = h * (t - j);
            Y = k * (t - l);
            Z = -m * (t - n);
            handAnchorPole.position = upperarm.transform.position + waist.parent.TransformDirection(new Vector3(X, Y, Z));
        }
    }
}
