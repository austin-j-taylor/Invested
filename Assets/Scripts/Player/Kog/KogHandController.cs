using UnityEngine;
using UnityEngine.Animations;
using System.Collections;

/// <summary>
/// Controls Kog grabbing and holding items pulled into her hand.
/// </summary>
public class KogHandController : MonoBehaviour {

    #region constants
    private const float grabberRadius = 0.25f;
    #endregion

    public enum GrabState { Empty, Grabbed };

    [SerializeField, Range(0.0f, 1.0f)]
    private float speed = 1;

    public GrabState State;
    private Rigidbody rb;
    private ParentConstraint grabJoint;
    private ConstraintSource grabberConstraintSource;
    [SerializeField]
    private Transform grabber = null;
    [SerializeField]
    private Collider[] collidersToIgnore = null;

    private Magnetic currentTarget = null;

    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        grabberConstraintSource = new ConstraintSource {
            sourceTransform = grabber,
            weight = 1
        };
    }

    public void Clear() {
        currentTarget = null;
        if(State == GrabState.Grabbed)
            Release();
        State = GrabState.Empty;
    }

    private void LateUpdate() {
        switch (State) {
            case GrabState.Empty:
                break;
            case GrabState.Grabbed:
                if (currentTarget == null || !currentTarget.isActiveAndEnabled) {
                    Release();
                }
                break;
        }
    }

    private void FixedUpdate() {
        if(State == GrabState.Grabbed) {
            foreach (Collider col in collidersToIgnore)
                Physics.IgnoreCollision(currentTarget.MainCollider, col, true);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (Kog.IronSteel.IronPulling && Kog.IronSteel.State == KogPullPushController.PullpushMode.Pullpushing && State == GrabState.Empty) {
            if (collision.rigidbody != null) {
                if (Vector3.Distance(grabber.position, collision.GetContact(0).point) < grabberRadius) {
                    Magnetic magnetic = collision.gameObject.GetComponent<Magnetic>();
                    if (magnetic == Kog.IronSteel.MainTarget && magnetic.Rb != null) {
                        Grab(Kog.IronSteel.MainTarget);
                    }
                }
            }
        }
    }

    private void Grab(Magnetic target) {

        Vector3 relativeVelocity = Vector3.Project(rb.velocity - target.Rb.velocity, target.transform.position - grabber.position);
        Debug.Log("Caught at " + relativeVelocity.magnitude + ", momentum: " + relativeVelocity.magnitude * target.Rb.mass);
        currentTarget = target;
        State = GrabState.Grabbed;
        target.Rb.isKinematic = true;
        foreach (Collider col in collidersToIgnore)
            Physics.IgnoreCollision(currentTarget.MainCollider, col, true);
        grabJoint = target.gameObject.AddComponent<ParentConstraint>();
        grabJoint.AddSource(grabberConstraintSource);
        grabJoint.SetTranslationOffset(0, Vector3.up * (target.prop_SO == null ? 0 : target.prop_SO.Grab_radius));
        grabJoint.constraintActive = true;

    }

    /// <summary>
    /// Release the target and return the target that was released.
    /// </summary>
    /// <returns></returns>
    public Magnetic Release() {
        Magnetic target = currentTarget;
        currentTarget = null;
        State = GrabState.Empty;
        Destroy(grabJoint);
        target.Rb.isKinematic = false;
        target.Rb.velocity = rb.velocity;
        foreach (Collider col in collidersToIgnore)
            Physics.IgnoreCollision(target.MainCollider, col, false);
        Debug.Log("Released");
        return target;
    }
}
