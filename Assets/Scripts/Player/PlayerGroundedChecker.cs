using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Every frame, checks if the player is just above a collider (i.e. is grounded).
 */
public class PlayerGroundedChecker : MonoBehaviour {
    
    public bool IsGrounded {
        get;
        private set;
    }

    private bool isInCollider;
    private Collider standingOnCollider;

    private void FixedUpdate() {
        IsGrounded = isInCollider;
        isInCollider = false;
    }

    private void Start() {
        IsGrounded = false;
        isInCollider = false;
        standingOnCollider = null;
    }

    private void OnTriggerStay(Collider other) {
        isInCollider = true;
        standingOnCollider = other;
    }

    public void AddForceToTouchingCollider(Vector3 force) {
        Rigidbody rb = standingOnCollider.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

}
