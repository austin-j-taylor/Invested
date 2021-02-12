using UnityEngine;
using UnityEngine.Animations;
using System.Collections;

/// <summary>
/// Controls Kog grabbing and holding items pulled into her hand.
/// </summary>
public class KogHandController : MonoBehaviour {

    #region constants
    private const float grabberRadius = 0.25f;
    private const float throwForce = 10;
    #endregion

    public enum GrabState { Empty, Grabbed };

    [SerializeField, Range(0.0f, 1.0f)]

    public GrabState State;
    private Rigidbody rb;
    private ParentConstraint grabJoint;
    private ConstraintSource grabberConstraintSource;
    [SerializeField]
    private Transform grabber = null;
    [SerializeField]
    private Collider[] collidersToIgnore = null;

    public Magnetic GrabbedTarget { get; private set; } = null;

    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        grabberConstraintSource = new ConstraintSource {
            sourceTransform = grabber,
            weight = 1
        };
    }

    public void Clear() {
        GrabbedTarget = null;
        if(State == GrabState.Grabbed)
            Release();
        State = GrabState.Empty;
    }

    private void LateUpdate() {
        switch (State) {
            case GrabState.Empty:
                break;
            case GrabState.Grabbed:
                if (GrabbedTarget == null || !GrabbedTarget.isActiveAndEnabled) {
                    Release();
                }
                break;
        }
    }

    private void FixedUpdate() {
        if(State == GrabState.Grabbed) {
            // definitely could be improved
            foreach (Collider col1 in collidersToIgnore)
                foreach (Collider col2 in GrabbedTarget.Colliders)
                    Physics.IgnoreCollision(col1, col2, true);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (Kog.IronSteel.IronPulling && Kog.IronSteel.State == KogPullPushController.PullpushMode.Pullpushing && State == GrabState.Empty) {
            if (collision.rigidbody != null) {
                if (Vector3.Distance(grabber.position, collision.GetContact(0).point) < grabberRadius) {
                    if (collision.rigidbody == Kog.IronSteel.MainTarget.Rb) {
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
        target.Rb.isKinematic = true;
        foreach (Collider col1 in collidersToIgnore)
            foreach (Collider col2 in target.Colliders)
                Physics.IgnoreCollision(col1, col2, true);
        grabJoint = target.Rb.gameObject.AddComponent<ParentConstraint>();
        grabJoint.AddSource(grabberConstraintSource);
        if(target.prop_SO != null) {
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
        target.Rb.AddForce(throwForce * (target.CenterOfMass - grabber.position).normalized, ForceMode.VelocityChange);

        return target;
    }

    /// <summary>
    /// Release the target gently.
    /// </summary>
    /// <returns>the dropped target</returns>
    public Magnetic Drop() {
        Magnetic target = GrabbedTarget;
        GrabbedTarget = null;
        State = GrabState.Empty;
        Destroy(grabJoint);
        target.Rb.isKinematic = false;
        //target.Rb.velocity = Vector3.zero;// rb.velocity;
        foreach (Collider col1 in collidersToIgnore)
            foreach (Collider col2 in target.Colliders)
                Physics.IgnoreCollision(col1, col2, false);
        //Debug.Log("Released");
        return target;
    }
}
