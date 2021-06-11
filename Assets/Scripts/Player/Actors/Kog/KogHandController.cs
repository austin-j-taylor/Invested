using UnityEngine;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls Kog grabbing and holding items pulled into her hand.
/// Controls tracking of enemies to aim and throw objects at.
/// </summary>
public class KogHandController : MonoBehaviour {

    #region constants
    private const float grabberRadius = 0.25f;
    private const float throwForce = 10;
    private const float maxSelectionRadius = 0.25f;
    #endregion

    public enum GrabState { Empty, Grabbed, LockedOn };

    [SerializeField, Range(0.0f, 1.0f)]

    public GrabState State;
    private Rigidbody rb;
    private ParentConstraint grabJoint;
    private ConstraintSource grabberConstraintSource;
    [SerializeField]
    private Transform grabber = null;
    [SerializeField]
    private Collider[] collidersToIgnore = null;

    public Vector3 ReachLocation { get; private set; } = Vector3.zero;
    public Magnetic GrabbedTarget { get; private set; } = null;
    public Entity MarkedEntity { get; private set; } = null;

    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        grabberConstraintSource = new ConstraintSource {
            sourceTransform = grabber,
            weight = 1
        };
    }

    public void Clear() {
        GrabbedTarget = null;
        MarkedEntity = null;
        if (State == GrabState.Grabbed)
            Release();
        State = GrabState.Empty;
    }

    private void LateUpdate() {
        // Transitions
        switch (State) {
            case GrabState.Empty:
                break;
            case GrabState.Grabbed:
                if (GrabbedTarget == null || !GrabbedTarget.gameObject.activeSelf) {
                    Release();
                }
                break;
            case GrabState.LockedOn:
                // Stay locked onto the current entity until target is released
                if (!Kog.IronSteel.SteelPushing) {
                    State = GrabState.Empty;
                    MarkedEntity = null;
                }
                break;
        }
        //Actions
        switch (State) {
            case GrabState.Empty:
                // Set reach target location: look towards the main Push/Pull target, or just in front
                if (Kog.IronSteel.MainTarget == null) {
                    // Rotate hand to look towards what the crosshairs are looking at
                    if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 1000, GameManager.Layer_IgnorePlayer)) {
                        // Aim at that point
                        ReachLocation = hit.point;
                    } else {
                        // Aim at a point 20 units in front of the camera
                        ReachLocation = CameraController.ActiveCamera.transform.position + CameraController.ActiveCamera.transform.forward * 20;
                    }
                } else {
                    ReachLocation = Kog.IronSteel.MainTarget.transform.position;
                }
                break;
            case GrabState.Grabbed:
                // Look for targets to lock on to.
                Entity closestHostile = null;
                float closestHostileRadius = float.PositiveInfinity;
                for (int i = 0; i < GameManager.EntitiesInScene.Count; i++) {
                    if (GameManager.EntitiesInScene[i].Hostile) {

                        // Get position on screen
                        Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(GameManager.EntitiesInScene[i].FuzzyGlobalCenterOfMass);
                        // make the center be 0
                        screenPosition.x -= .5f;
                        screenPosition.y -= .5f;
                        // Pretend the screen is a square for radial distance. Scale down X.
                        screenPosition.x = screenPosition.x * Screen.width / Screen.height;

                        float radialDistance = Mathf.Sqrt(
                            (screenPosition.x) * (screenPosition.x) +
                            (screenPosition.y) * (screenPosition.y)
                        );
                        // If it's in front of the screen, close to the center of the screen, and closer than any other target
                        if (screenPosition.z > 0 &&
                                radialDistance < maxSelectionRadius &&
                                radialDistance < closestHostileRadius) {
                            closestHostile = GameManager.EntitiesInScene[i];
                            closestHostileRadius = radialDistance;
                        }
                    }
                }
                // Lock on to that target.
                MarkedEntity = closestHostile;

                // Set reach location: aim towards the marked entity, or in front if there is none
                if (MarkedEntity == null) {
                    // Rotate hand to look towards what the crosshairs are looking at
                    if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 1000, GameManager.Layer_IgnorePlayer)) {
                        // Aim at that point
                        ReachLocation = hit.point;
                    } else {
                        // Aim at a point 20 units in front of the camera
                        ReachLocation = CameraController.ActiveCamera.transform.position + CameraController.ActiveCamera.transform.forward * 20;
                    }
                } else {
                    ReachLocation = MarkedEntity.transform.position;
                }

                break;
            case GrabState.LockedOn:
                // Stay locked on to current marked entity while maintaining the Push
                ReachLocation = Kog.IronSteel.MainTarget.transform.position;
                break;
        }
    }
    // Keep the colliders for the held object and the body from interfering
    private void FixedUpdate() {
        if (State == GrabState.Grabbed) {
            // definitely could be improved
            foreach (Collider col1 in collidersToIgnore)
                foreach (Collider col2 in GrabbedTarget.Colliders)
                    Physics.IgnoreCollision(col1, col2, true);
        }
    }

    private void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }
    private void OnCollisionEnter(Collision collision) {
        if (Kog.IronSteel.IronPulling && Kog.IronSteel.State == KogPullPushController.PullpushMode.Pullpushing && State == GrabState.Empty) {
            if (collision.rigidbody != null) {
                if (Vector3.Distance(grabber.position, collision.GetContact(0).point) < grabberRadius) {
                    if (Kog.IronSteel.MainTarget && collision.rigidbody == Kog.IronSteel.MainTarget.Rb) {
                        Grab(Kog.IronSteel.MainTarget);
                    }
                }
            }
        }
    }

    private void Grab(Magnetic target) {

        Vector3 relativeVelocity = Vector3.Project(rb.velocity - target.Rb.velocity, target.transform.position - grabber.position);
        //Debug.Log("Caught at " + relativeVelocity.magnitude + ", momentum: " + relativeVelocity.magnitude * target.Rb.mass);
        GrabbedTarget = target;
        State = GrabState.Grabbed;
        target.enabled = false;
        target.Rb.isKinematic = true;
        foreach (Collider col1 in collidersToIgnore)
            foreach (Collider col2 in target.Colliders)
                Physics.IgnoreCollision(col1, col2, true);
        grabJoint = target.Rb.gameObject.AddComponent<ParentConstraint>();
        grabJoint.AddSource(grabberConstraintSource);
        if (target.prop_SO != null) {
            grabJoint.SetTranslationOffset(0, new Vector3(0, target.prop_SO.Grab_pos_y, target.prop_SO.Grab_pos_z));
            grabJoint.SetRotationOffset(0, new Vector3(0, 0, target.prop_SO.Grab_rotation_z));
        }
        grabJoint.constraintActive = true;

    }

    /// <summary>
    /// Release the target and return the target that was released.
    /// </summary>
    /// <returns>the released target</returns>
    public Magnetic Release() {
        Magnetic target = Drop();
        //if (target != null)
        //    target.Rb.AddForce(throwForce * (target.CenterOfMass - grabber.position).normalized, ForceMode.VelocityChange);
        return target;
    }

    /// <summary>
    /// Release the target gently.
    /// </summary>
    /// <returns>the dropped target</returns>
    public Magnetic Drop() {
        Magnetic target = GrabbedTarget;
        GrabbedTarget = null;
        if (Kog.IronSteel.SteelPushing) {
            State = GrabState.LockedOn;
        } else {
            State = GrabState.Empty;
            MarkedEntity = null;
        }
        Destroy(grabJoint);
        if (target != null) {
            target.enabled = true;
            target.Rb.isKinematic = false;
            target.Rb.AddForce(rb.velocity - target.Rb.velocity, ForceMode.VelocityChange);
            foreach (Collider col1 in collidersToIgnore)
                foreach (Collider col2 in target.Colliders)
                    Physics.IgnoreCollision(col1, col2, false);
        }
        //Debug.Log("Released");
        return target;
    }
}