using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A small bead that, when touched, triggers some action.
 */
public abstract class TriggerBead : Magnetic {

    private const float distanceThreshold = .001f;
    private const float forceConstant = 20f;

    private void FixedUpdate() {
        // Pull sphere towards center
        Vector3 distance = transform.parent.position - transform.position;
        float sqrDistance = distance.sqrMagnitude;
        if (sqrDistance > distanceThreshold) {
            // Square relationship between force and distance
            Rb.AddForce(forceConstant * distance.normalized * sqrDistance, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Destroy(transform.parent.gameObject);
            Trigger();
        }
    }

    protected abstract void Trigger();
}
