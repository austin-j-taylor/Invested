using UnityEngine;
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
    [SerializeField]
    private Transform grabber = null;

    private Magnetic currentTarget = null;

    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
    }

    public void Clear() {
        currentTarget = null;
        State = GrabState.Empty;
    }

    private void LateUpdate() {
        switch (State) {
            case GrabState.Empty:
                break;
            case GrabState.Grabbed:
                if(currentTarget == null || !currentTarget.isActiveAndEnabled) {
                    Release();
                }
                break;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (Kog.IronSteel.State == KogPullPushController.PullpushMode.Pullpushing && State == GrabState.Empty) {
            if (collision.rigidbody != null) {
                if (Vector3.Distance(grabber.position, collision.GetContact(0).point) < grabberRadius) {
                    Magnetic magnetic = collision.gameObject.GetComponent<Magnetic>();
                    if (magnetic == Kog.IronSteel.MainTarget) {
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
    }

    /// <summary>
    /// Release the target and return the target that was released.
    /// </summary>
    /// <returns></returns>
    public Magnetic Release() {
        Magnetic target = currentTarget;
        currentTarget = null;
        State = GrabState.Empty;
        Debug.Log("Released");
        return target;
    }
}
