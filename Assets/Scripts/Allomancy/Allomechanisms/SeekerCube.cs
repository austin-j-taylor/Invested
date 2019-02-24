using UnityEngine;
using System.Collections;

/*
 * An object that responds to burning metals near it.
 * 
 */
public class SeekerCube : MonoBehaviour {

    private const float radius = 10;
    private const float maxRotationSpeedLow = .3f;
    private const float maxRotationSpeedHigh = 30f;
    private const float highDrag = 1f;
    private const float lowDrag = 0f;
    private readonly Vector3 rotation = new Vector3(50, 50, 50);

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        Debug.Log(rb.angularVelocity.magnitude);
        rb.AddTorque(transform.rotation * rotation);

        if (rb.angularVelocity.magnitude < .5f) {
            rb.drag = highDrag;
        } else {

        }
    }

}
