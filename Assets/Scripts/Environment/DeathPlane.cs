using UnityEngine;
using System.Collections;

// When the collider is triggered, the player is sent back to the respawn point.
[RequireComponent(typeof(Collider))]
public class DeathPlane : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if(Player.IsPlayerTrigger(other)) {
            Player.PlayerInstance.Respawn();
        }
    }
}
