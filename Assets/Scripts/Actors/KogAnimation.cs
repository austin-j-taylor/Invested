using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scripted animation for Kog
public class KogAnimation : MonoBehaviour {

    #region constants
    private const float zeroThresholdSqr = 0.05f * 0.05f;
    private const float defaultRaycastDistance = 2;
    private const float radiusForRaycast = 0.125f;
    public float defaultDistanceBetweenSteps = .25f;
    public float stepTime = 1f;
    public float stepHeight = .5f;
    public float legForwardsFactor = 10;
    public float stepMovementLerpFactor = 10;
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
    [SerializeField]
    private Leg leftLeg = null;
    [SerializeField]
    private Leg rightLeg = null;

    void Start() {
        anim = GetComponentInChildren<Animator>();

        leftLeg.Initialize(this, rightLeg);
        rightLeg.Initialize(this, leftLeg);

        anim.Play("Armature|I-Pose");
    }

    public void Clear() {
        leftLeg.Clear();
        rightLeg.Clear();
    }

    private void OnAnimatorMove() {
        leftLeg.LegUpdate();
        rightLeg.LegUpdate();
    }

    public void SetLegTarget(Vector3 movement) {
        movement = (Vector3.down + movement / legForwardsFactor) * defaultRaycastDistance;
        float length = movement.magnitude;
        movement.Normalize();
        leftLeg.raycastDirection = movement;
        rightLeg.raycastDirection = movement;
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
        public float raycastDistance = defaultRaycastDistance;

        [SerializeField]
        public Transform foot = null;
        [SerializeField]
        public Transform footTarget = null;
        [SerializeField]
        public Transform footRaycastSource = null;
        [SerializeField]
        public Transform debugBall_HitPoint = null, debugBall_currentAnchor = null;

        public KogAnimation parent;
        private Leg otherLeg;
        private Vector3 footAnchor = Vector3.zero, footLastAnchor = Vector3.zero, footNextAnchor = Vector3.zero;
        private Quaternion footAnchorRotation, footLastAnchorRotation = Quaternion.identity, footNextAnchorRotation = Quaternion.identity;
        private Vector3 footRestRotation;

        private float currentDistance = 0;
        private float tInStep = 0;

        public void Initialize(KogAnimation parent, Leg otherLeg) {
            this.parent = parent;
            this.otherLeg = otherLeg;

            footAnchor = footTarget.position;
            footAnchorRotation = footTarget.rotation;
            footRestRotation = footTarget.localRotation.eulerAngles;
        }

        public void Clear() {
            state = WalkingState.Idle;
        }

        public void LegUpdate() {
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
                        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.transform.rotation * Quaternion.Euler(footRestRotation);
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
    }
}
