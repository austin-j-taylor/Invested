using UnityEngine;

/*
 * A TriggerBead that opens a message over the HUD when triggered.
 */
public class TriggerBeadPopup : TriggerBead {

    [HideInInspector]
    public string message;

    protected override void Trigger() {
        HUD.MessageOverlayController.Text.text = (message);
    }
}
