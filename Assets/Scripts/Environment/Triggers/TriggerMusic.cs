using UnityEngine;
using System.Collections;

// Changes the music when leaving and entering the trigger, which encompasses a large area.
[RequireComponent(typeof(Collider))]
public class TriggerMusic : MonoBehaviour {

    [SerializeField]
    EnvironmentalTransitionManager musicManager = null;

    private void OnTriggerEnter(Collider other) {
        if(Player.IsPlayerTrigger(other)) {
            musicManager.EnterInterior(this);
        }
    }
    private void OnTriggerExit(Collider other) {
        if (Player.IsPlayerTrigger(other)) {
            musicManager.ExitInterior(this);
        }
    }
}
