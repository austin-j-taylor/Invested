using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment_StormForce : MonoBehaviour {

    [SerializeField]
    private Vector3 forceDirection = new Vector3(0, 1, 0);

    [SerializeField]
    private int exitSpeed = 100;
    [SerializeField]
    private float restitution = 10;

    private float barrier;

    private void Awake() {
        barrier = transform.position.y + transform.localScale.y * forceDirection.y / 2;
    }

    private void OnTriggerStay(Collider other) {
        if (!other.isTrigger && other.attachedRigidbody) {
            Vector3 force = forceDirection * exitSpeed;
            other.attachedRigidbody.AddForce(force, ForceMode.Acceleration);

            // apply force like a spring from the barrier
            float depth = barrier - other.transform.position.y;
            if (depth < 0)
                depth = -depth;
            force = restitution * depth * forceDirection;
            other.attachedRigidbody.AddForce(force, ForceMode.Acceleration);
        }
    }
}
