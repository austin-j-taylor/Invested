using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment_StormForce : MonoBehaviour {
    private const int exitSpeed = 100;
    private const float restitution = 1.25f;

    private readonly Vector3 upForce = new Vector3(0, exitSpeed, 0);

    private void OnTriggerStay(Collider other) {
        if (!other.isTrigger && other.attachedRigidbody) {
            Vector3 force = upForce;
            other.attachedRigidbody.AddForce(force, ForceMode.Acceleration);
            Debug.Log("forcedvan " + force);
            if (Vector3.Dot(other.attachedRigidbody.velocity, Vector3.down) > 0) {
                // if object is falling fast, bounce that velocity
                force = 2 * restitution * Vector3.Project(-other.attachedRigidbody.velocity, Vector3.down);
                other.attachedRigidbody.AddForce(force, ForceMode.Acceleration);
                Debug.Log("forcedex " + force);
            } else {
                force = 2 * restitution * Vector3.Project(other.attachedRigidbody.velocity, Vector3.down);
                other.attachedRigidbody.AddForce(force, ForceMode.Acceleration);
                Debug.Log("forced ex up" + force);
            }
        }
    }
}
