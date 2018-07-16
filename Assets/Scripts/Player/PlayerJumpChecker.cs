using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpChecker : MonoBehaviour {
    
    public bool IsGrounded {
        get;
        private set;
    }
    private bool isInCollider;

    private void FixedUpdate() {
        IsGrounded = isInCollider;
        isInCollider = false;
    }

    private void Start() {
        IsGrounded = false;
        isInCollider = false;
}
    private void OnTriggerStay(Collider other) {
        isInCollider = true;
    }
}
