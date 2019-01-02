using UnityEngine;
using System.Collections;

/*
 * A TriggerBead that opens a single message over the HUD when triggered,
 *      then destroys itself.
 */
public class TriggerBeadPopupMessage : TriggerBeadPopup {
    
    protected override void Trigger() {
        base.Trigger();
        Destroy(gameObject);
    }
}
