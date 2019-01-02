using UnityEngine;

/*
 * Oversees a series of TriggerBeadPopups. 
 */
public class TriggerBeadOverhead : MonoBehaviour {

    public enum Section { tutorial1 = 0, tutorial2 = 3 };

    public Section section;

    protected TriggerBeadPopupMessage[] beadMessages;
    protected TriggerBeadPopupListener[] beadListeners;

    // Use this for initialization
    void Start() {
        beadMessages = GetComponentsInChildren<TriggerBeadPopupMessage>();

        // Assume messages.length = number of TriggerBeads in scene with same section number
        if (beadMessages.Length == GameManager.TriggerBeadMessages[(int)section].Count)
            for (int i = 0; i < beadMessages.Length; i++) {
                beadMessages[i].message = GameManager.TriggerBeadMessages[(int)section][i];
                beadMessages[i].overhead = this;
            } else {
            Debug.LogError("Error: beads.Length != messages.Length: " + beadMessages.Length + " != " + GameManager.TriggerBeadMessages[(int)section].Count);
        }

        beadListeners = GetComponentsInChildren<TriggerBeadPopupListener>();
        for (int i = 0; i < beadListeners.Length; i++) {
            int messagesIndex = 0;
            beadListeners[i].message = GameManager.TriggerBeadMessages[(int)section + 1 + i][messagesIndex];
            beadListeners[i].overhead = this;
            messagesIndex++;
            for (int j = 0; j < beadListeners[i].actions.Length; j++) {
                beadListeners[i].messages[j] = GameManager.TriggerBeadMessages[(int)section + 1 + i][messagesIndex];
                messagesIndex++;
            }
        }
    }

    // When one TriggerBeadPopup is entered, make sure no other TriggerBeadListeners are still running to update the MessageOverlay later on.
    public void StopTriggerCoroutines() {
        for (int i = 0; i < beadListeners.Length; i++) {
            beadListeners[i].StopActionCoroutine();
        }
    }
}
