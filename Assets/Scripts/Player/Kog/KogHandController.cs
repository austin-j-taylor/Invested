using UnityEngine;
using System.Collections;

/// <summary>
/// Controls Kog grabbing and holding items pulled into her hand.
/// </summary>
public class KogHandController : MonoBehaviour {

    #region constants
    private const float grabberRadius = 0.25f;
    #endregion

    private enum State { Empty, Grabbed };

    [SerializeField, Range(0.0f, 1.0f)]
    private float speed = 1;

    private State state;
    private Rigidbody rb;
    [SerializeField]
    private Transform grabber = null;

    private Magnetic currentTarget = null;

    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
    }

    public void Clear() {
        currentTarget = null;
        state = State.Empty;
    }

    private void Update() {
        switch (state) {
            case State.Empty:
                break;
            case State.Grabbed:
                if(currentTarget == null || !currentTarget.isActiveAndEnabled)
                    Release();
                Release();
                state = State.Empty;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (Kog.IronSteel.State == KogPullPushController.PullpushMode.Pullpushing && state == State.Empty) {
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
        //Debug.Log("Caught at " + relativeVelocity.magnitude + ", momentum: " + relativeVelocity.magnitude * target.Rb.mass);
        currentTarget = target;
    }

    private void Release() {
        currentTarget = null;
    }
}
