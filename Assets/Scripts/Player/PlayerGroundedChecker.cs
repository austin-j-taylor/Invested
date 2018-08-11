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
    private Rigidbody standingOnRigidbody;

    private void FixedUpdate() {
        IsGrounded = isInCollider;
        isInCollider = false;
    }

    private void Start() {
        IsGrounded = false;
        isInCollider = false;
        standingOnRigidbody = null;
    }

    private void OnTriggerEnter(Collider other) {

        standingOnRigidbody = other.GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other) {
        isInCollider = true;
    }

    public void AddForceToTouchingCollider(Vector3 force) {
        if(standingOnRigidbody != null) {
            standingOnRigidbody.AddForce(force, ForceMode.Impulse);
        }
    }

}
