using UnityEngine;

/*
 * A TriggerBead that opens a single message over the in the Message Overlay when triggered.
 */
public class TriggerBeadPopup : TriggerBead {

    [HideInInspector]
    public TriggerBeadOverhead overhead;
    [HideInInspector]
    public int section;
    [HideInInspector]
    public int index;

    protected override void Trigger() {
        HUD.MessageOverlayController.CurrentPopup = this;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Light>().enabled = false;
        GetComponent<Magnetic>().enabled = false;
    }

    public virtual string GetHeader() {
        return HUD.MessageOverlayController.TriggerBeadMessages[section][0];
    }

    public virtual string GetText() {
        return HUD.MessageOverlayController.TriggerBeadMessages[section][index];
    }

    public void Close() {
        Destroy(transform.parent.gameObject);
        HUD.MessageOverlayController.CurrentPopup = null;
    }
}
