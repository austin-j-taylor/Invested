using UnityEngine;
using System.Collections;

/*
 * When this is attached to a gameobject with a collider, it will play the Message with the given name.
 * Then, the collider is disabeld.
 */

[RequireComponent(typeof(Collider))]
public class TriggerMessage : TriggerEnvironment {

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            StartCoroutine(routine);
            GetComponent<Collider>().enabled = false;
        }
    }
}
