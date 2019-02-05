using UnityEngine;

/*
 * Oversees a series of TriggerBeadPopups.
 *      Assigns TriggerBeadPopup section and index fields based on their order in the scene,
 *      which correspond to strings stored in MessageOverlayController.
 */
public class TriggerBeadOverhead : MonoBehaviour {

    public enum Section { tutorial1 = 0, tutorial2 = 3 };

    public Section section;
    
    void Start() {
        //TriggerBeadPopupMessage[] beadMessages = GetComponentsInChildren<TriggerBeadPopupMessage>();

        //// Assume messages.length = number of TriggerBeads in scene with same section number
        //for (int i = 0; i < beadMessages.Length; i++) {
        //    beadMessages[i].section = (int)section;
        //    beadMessages[i].index = 1 + i;
        //    beadMessages[i].overhead = this;
        //}

        TriggerBeadPopupListener[] beadListeners = GetComponentsInChildren<TriggerBeadPopupListener>(true);
        for (int i = 0; i < beadListeners.Length; i++) {
            beadListeners[i].section = (int)section + i;
            beadListeners[i].index = 1;
            beadListeners[i].overhead = this;
        }
    }
}
