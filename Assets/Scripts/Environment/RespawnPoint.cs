using UnityEngine;
using System.Collections;

// When the player touches a respawn point, if they die/void out, teleport to here
[RequireComponent(typeof(Collider))]
public class RespawnPoint : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !other.isTrigger) {
            Player.PlayerInstance.RespawnPoint = transform.position;
        }
    }
}
