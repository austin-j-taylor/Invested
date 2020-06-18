using UnityEngine;

/*
 * A TriggerBead that opens a single message over the in the Descriptive Message Overlay  when triggered.
 */
public class TriggerDescriptiveMessage : MonoBehaviour {

    [SerializeField]
    private string header = "";
    [SerializeField, TextArea(10, 20)]
    private string content = "";

    private void OnTriggerEnter(Collider other) {
        if(Player.IsPlayerTrigger(other)) {
            // trigger is active -> disable the bead, and activate the outer collider
            gameObject.SetActive(false);
            HUD.MessageOverlayDescriptive.SetHeader(header);
            HUD.MessageOverlayDescriptive.SetMessage(content);
        }
    }
}
