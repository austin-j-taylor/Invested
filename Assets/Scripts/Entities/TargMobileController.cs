using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargMobileController : Targ {
    
    private NavMeshAgent mesh;

    protected override void Start() {
        base.Start();
       
        mesh = GetComponent<NavMeshAgent>();
        MaxHealth = 15;
    }
    void Update() {
        if (!isDead) {
            if (mesh.velocity.magnitude > .1f || true) {
                anim.SetBool("IsMoving", true);
            } else {
                anim.SetBool("IsMoving", false);
            }
            if (Health <= 0) {
                // die
                mesh.isStopped = true;
                isDead = true;
                anim.SetTrigger("Killed");
            } else {
                if (mesh.enabled) {
                    mesh.SetDestination(Player.PlayerInstance.transform.position);
                }
            }
        }
    }
}
