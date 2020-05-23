using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargMobileController : Entity {

    protected override void Start() {
        base.Start();
       
        MaxHealth = 15;
    }
    void Update() {
        if (!isDead) {
            if (rb.velocity.magnitude > .1f || true) {
                anim.SetBool("IsMoving", true);
            } else {
                anim.SetBool("IsMoving", false);
            }
            if (Health <= 0) {
                // die
                isDead = true;
                anim.SetTrigger("Killed");
                //rb.constraints = RigidbodyConstraints.None;
            } else {
                    //rb.velocity = new Vector3(-5, 0, 0);
            }
        }
    }
}
