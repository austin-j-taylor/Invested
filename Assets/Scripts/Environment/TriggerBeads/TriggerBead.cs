using UnityEngine;

/*
 * A small bead that, when touched, triggers some action.
 */
public abstract class TriggerBead : MonoBehaviour {

    private const float distanceThreshold = .001f;
    private const float forceConstant = 20f;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        // Pull sphere towards center
        Vector3 distance = transform.parent.position - transform.position;
        float sqrDistance = distance.sqrMagnitude;
        if (sqrDistance > distanceThreshold) {
            // Square relationship between force and distance
            rb.AddForce(forceConstant * distance.normalized * sqrDistance, ForceMode.Acceleration);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Trigger();
        }
    }

    protected abstract void Trigger();
}
