using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDescriptiveClose : MonoBehaviour
{
    private TriggerDescriptiveMessage messageTrigger;

    private void Start() {
        messageTrigger = GetComponentInChildren<TriggerDescriptiveMessage>();
    }

    private void OnTriggerExit(Collider other) {
        if (Player.IsPlayerTrigger(other) && !messageTrigger.isActiveAndEnabled) {
            // messageTrigger is disabled -> enable it, and clear the overlay
            messageTrigger.gameObject.SetActive(true);
            HUD.MessageOverlayDescriptive.Clear();
        }
    }
}
