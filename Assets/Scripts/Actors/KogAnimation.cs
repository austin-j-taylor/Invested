using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scripted animation for Kog
public class KogAnimation : MonoBehaviour {

    #region constants
    private const float zeroThresholdSqr = 0.05f * 0.05f;
    private const float radiusForRaycast = 0.125f;
    public float crouchingMax = 0.035f;
    public float crouchingLegSpreadMax = 0.25f;
    public float defaultRaycastDistance = 2;
    public float defaultDistanceBetweenSteps = .25f;
    public float stepTime = 0.4f;
    public float stepHeight = .5f;
    public float legForwardsFactor = 10;
    public float stepMovementLerpFactor = 10;
    public float armHeight = 1.25f;
    #endregion

    private enum GroundedState { Grounded, Airborne }

    public bool IsGrounded => leftLeg.groundedState == GroundedState.Grounded || rightLeg.groundedState == GroundedState.Grounded;
    public Rigidbody StandingOnRigidbody => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnRigidbody : rightLeg.standingOnRigidbody;
    public Vector3 StandingOnPoint => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnPoint : rightLeg.standingOnPoint;
    public Vector3 StandingOnNormal => leftLeg.standingOnRigidbody != null ? leftLeg.standingOnNormal : rightLeg.standingOnNormal;

    private float distanceBetweenSteps = .25f;

    //[SerializeField]
    //private Transform target = null;
    //[SerializeField]
    //private Transform hand = null;
    //[SerializeField]
    //private Transform pole = null;

    private Animator anim;
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
    private Transform waist = null;

    void Start() {
        anim = GetComponentInChildren<Animator>();

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
        leftLeg.LegUpdate();
        rightLeg.LegUpdate();
        leftArm.ArmUpdate();
        rightArm.ArmUpdate();
    }

    public void SetLegTarget(Vector3 movement, float currentSpeed) {
        Vector3 crouchSpread = waist.right * crouchingLegSpreadMax * crouching;
        Vector3 legForwardsMovement = Vector3.down + movement / legForwardsFactor;
        Vector3 leftMove = (-crouchSpread + legForwardsMovement) * defaultRaycastDistance;
        Vector3 rightMove = (crouchSpread + legForwardsMovement) * defaultRaycastDistance;

        float length = leftMove.magnitude;
        leftLeg.raycastDirection = leftMove.normalized;
        rightLeg.raycastDirection = rightMove.normalized;
        leftLeg.raycastDistance = length;
        rightLeg.raycastDistance = length;

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
        public float raycastDistance;

        [SerializeField]
        public Transform foot = null, thigh = null;
        [SerializeField]
        public Transform footTarget = null;
        [SerializeField]
        public Transform footRaycastSource = null;
        [SerializeField]
        public Transform footCollider = null;
        [SerializeField]
        public Transform debugBall_HitPoint = null, debugBall_currentAnchor = null;

        private KogAnimation parent;
        private Leg otherLeg;
        private Vector3 footAnchor = Vector3.zero, footLastAnchor = Vector3.zero, footNextAnchor = Vector3.zero;
        private Quaternion footAnchorRotation, footLastAnchorRotation = Quaternion.identity, footNextAnchorRotation = Quaternion.identity;
        private Vector3 footRestRotation;
        private Vector3 footColliderPosition;

        private float currentDistance = 0;
        private float tInStep = 0;

        public void Initialize(KogAnimation parent, Leg otherLeg) {
            this.otherLeg = otherLeg;
            this.parent = parent;
            raycastDistance = parent.defaultRaycastDistance;

            footAnchor = footTarget.position;
            footAnchorRotation = footTarget.rotation;
            footRestRotation = footTarget.localRotation.eulerAngles;
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
                    if (Physics.SphereCast(footRaycastSource.position, radiusForRaycast, raycastDirection, out RaycastHit hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        groundedState = GroundedState.Grounded;
                        standingOnRigidbody = hit.rigidbody;
                        standingOnPoint = hit.point;
                        standingOnNormal = hit.normal;
                        Debug.DrawLine(footRaycastSource.position, footRaycastSource.position + raycastDirection * raycastDistance, Color.red);
                        debugBall_HitPoint.gameObject.SetActive(true);
                        debugBall_currentAnchor.gameObject.SetActive(true);
                        debugBall_HitPoint.position = hit.point;
                        debugBall_currentAnchor.position = hit.point;

                        currentDistance = (foot.transform.position - hit.point).magnitude;
                        if (currentDistance > parent.distanceBetweenSteps && otherLeg.state == WalkingState.Idle) {
                            // Take a step
                            footLastAnchor = footTarget.position;
                            footLastAnchorRotation = footTarget.rotation;
                            footNextAnchor = hit.point;
                            state = WalkingState.Stepping;
                            tInStep = 0;
                        }
                    } else {
                        groundedState = GroundedState.Airborne;
                        standingOnRigidbody = null;
                        debugBall_HitPoint.gameObject.SetActive(false);
                        debugBall_currentAnchor.gameObject.SetActive(false);
                    }
                    break;
                case WalkingState.Stepping:
                    // Get new leg anchor target
                    if (Physics.SphereCast(footRaycastSource.position, radiusForRaycast, raycastDirection, out hit, raycastDistance, GameManager.Layer_IgnorePlayer)) {
                        groundedState = GroundedState.Grounded;
                        standingOnRigidbody = hit.rigidbody;
                        standingOnPoint = hit.point;
                        standingOnNormal = hit.normal;

                        //footNextAnchor = hit.point;
                        footNextAnchor = Vector3.Lerp(footNextAnchor, hit.point, Time.deltaTime * parent.stepMovementLerpFactor);
                        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.waist.rotation * Quaternion.Euler(footRestRotation);
                        footNextAnchorRotation = Quaternion.Slerp(footNextAnchorRotation, targetRotation, Time.deltaTime * parent.stepMovementLerpFactor);
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
                        groundedState = GroundedState.Airborne;
                        standingOnRigidbody = null;
                        debugBall_HitPoint.gameObject.SetActive(false);
                        debugBall_currentAnchor.gameObject.SetActive(false);
                    }

                    // Set leg position along parabola between anchors
                    if (currentDistance > parent.distanceBetweenSteps) {
                        tInStep += Time.deltaTime / parent.stepTime * currentDistance / parent.distanceBetweenSteps;
                    } else {
                        tInStep += Time.deltaTime / parent.stepTime;
                    }

                    if (tInStep >= 1) {
                        footAnchor = footNextAnchor;
                        footAnchorRotation = footNextAnchorRotation;
                        state = WalkingState.Idle;
                    } else {
                        float y = (-2 * (tInStep - 0.5f) * (tInStep - 0.5f) + 0.5f) * parent.stepHeight;

                        Vector3 pos = footLastAnchor + tInStep * (footNextAnchor - footLastAnchor);
                        pos.y += y;
                        footAnchor = pos;
                        footAnchorRotation = Quaternion.Slerp(footLastAnchorRotation, footNextAnchorRotation, tInStep);
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
