using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scripted animation for Kog
public class KogAnimation : MonoBehaviour {

    #region constants
    private const float zeroThresholdSqr = 0.05f * 0.05f;
    private const float distanceForRaycast = 4;
    private const float radiusForRaycast = 0.125f;
    public float distanceBetweenSteps = .25f;
    public float stepTime = 1f;
    public float stepHeight = .5f;
    #endregion

    [SerializeField]
    private Transform target = null;
    [SerializeField]
    private Transform hand = null;
    [SerializeField]
    private Transform pole = null;

    private Animator anim;
    [SerializeField]
    private Leg leftLeg = null;
    [SerializeField]
    private Leg rightLeg = null;

    void Start() {
        anim = GetComponentInChildren<Animator>();
        target = Player.PlayerInstance.transform;

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

    [System.Serializable]
    private class Leg {

        private enum WalkingState { Idle, Stepping }

        private WalkingState state = WalkingState.Idle;

        [SerializeField]
        public Transform foot;
        [SerializeField]
        public Transform footTarget;
        [SerializeField]
        public Transform footRaycastSource;

        public KogAnimation parent;
        private Leg otherLeg;
        private Vector3 footAnchor = Vector3.zero, footLastAnchor = Vector3.zero, footNextAnchor = Vector3.zero;
        private Quaternion footAnchorRotation, footLastAnchorRotation = Quaternion.identity, footNextAnchorRotation = Quaternion.identity;
        private Vector3 footRestRotation;

        public float currentDistance = 0;
        public float tInStep = 0;

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
                    // Left foot
                    if (Physics.SphereCast(footRaycastSource.position, radiusForRaycast, Vector3.down, out RaycastHit hit, distanceForRaycast, GameManager.Layer_IgnorePlayer)) {
                        Debug.DrawLine(footRaycastSource.position, hit.point, Color.red);

                        currentDistance = (foot.transform.position - hit.point).magnitude;
                        if (currentDistance > parent.distanceBetweenSteps && otherLeg.state == WalkingState.Idle) {
                            // Take a step
                            footLastAnchor = footTarget.position;
                            footLastAnchorRotation = footTarget.rotation;
                            state = WalkingState.Stepping;
                            tInStep = 0;
                        }
                    }
                    break;
                case WalkingState.Stepping:
                    // Get new leg anchor target
                    if (Physics.SphereCast(footRaycastSource.position, radiusForRaycast, Vector3.down, out hit, distanceForRaycast, GameManager.Layer_IgnorePlayer)) {
                        Debug.DrawLine(footRaycastSource.position, hit.point, Color.red);

                        footNextAnchor = hit.point;
                        footNextAnchorRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * parent.transform.rotation * Quaternion.Euler(footRestRotation);
                        //footNextAnchorRotation = parent.transform.rotation * Quaternion.Euler(footNextNormal);
                        //footNextAnchorRotation = Quaternion.LookRotation(footNextNormal);
                        currentDistance = (foot.transform.position - hit.point).magnitude;
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
