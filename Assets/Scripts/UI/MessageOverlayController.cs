using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * Displays a message over the HUD. Stores strings that represent these messages.
 *      TriggerBeadPopups send these messages.
 *      The strings must be dynamically stored such that they can change with the Control Scheme.
 */
public class MessageOverlayController : MonoBehaviour {

    private const int numberOfMessages = 4;

    public Text HeaderText { get; private set; }
    public Text MessageText { get; private set; }

    public List<string>[] TriggerBeadMessages { get; private set; }

    private TriggerBeadPopup currentPopup;
    public TriggerBeadPopup CurrentPopup {
        get {
            return currentPopup;
        }
        set {
            if (value != null) {
                // Entering a new popup
                if (currentPopup)
                    currentPopup.Close();
                currentPopup = value;
                HeaderText.text = currentPopup.GetHeader();
                MessageText.text = currentPopup.GetText();
            } else {
                currentPopup = null;
                Clear();
            }
        }
    }

    void Awake() {
        HeaderText = transform.Find("Header").GetComponent<Text>();
        MessageText = transform.Find("Message").GetComponent<Text>();
        // TriggerBeadPopup strings
        TriggerBeadMessages = new List<string>[numberOfMessages];
        UpdateMessages();
    }

    public void Clear() {
        HeaderText.text = string.Empty;
        MessageText.text = string.Empty;
    }

    public void UpdateText() {
        if (currentPopup) {
            HeaderText.text = currentPopup.GetHeader();
            MessageText.text = currentPopup.GetText();
        }
    }

    // The string "constants" used by TriggerBeadPopups.
    public void UpdateMessages() {
        // TriggerBeadPopup strings
        TriggerBeadMessages[0] = new List<string> {
            "Movement",
            TextCodes.KeyLook + " to look around.\n\n" + TextCodes.KeyMove + " to move.",
            TextCodes.KeyJump + " to jump.\n\n\tCollect the glowing bead.",

        };
        TriggerBeadMessages[1] = new List<string> {
            "Pushing & Pulling Basics",
            TextCodes.KeyStartBurning + "\n\t\tto start burning " + TextCodes.Iron + " or " + TextCodes.Steel + ".",
            TextCodes.s_Press_ + TextCodes.KeySelect + "\n\t\tto select a metal to be a " + TextCodes.Pull_target + ".\n" +
                TextCodes.s_Press_ + TextCodes.KeySelectAlternate + "\n\t\tto select a metal to be a " + TextCodes.Push_target + ".",
            TextCodes.KeyPull + " to " + TextCodes.Pull + ".\n" +
                TextCodes.KeyPush + " to " + TextCodes.Push + ".",
            "While holding " + TextCodes.KeyNegate + ", " + TextCodes.s_Press_ + TextCodes.KeySelect + "\n\t\t to deselect a " + TextCodes.Pull_target +
                ".\nLikewise for " + TextCodes.KeySelectAlternate + " and " + TextCodes.Push_targets + ".",
            TextCodes.KeyHelp + " to toggle the Help Overlay."
        };
        TriggerBeadMessages[2] = new List<string> {
            "Targeting Multiple Metals",
            TextCodes.KeyNumberOfTargets + " to change your " + TextCodes.Gray("max number of " + TextCodes.Push_Pull_targets + ".")
        };
        // after teaching to deselect
        //"If you have both " + TextCodes.Pull_targets + " and " + TextCodes.Push_targets + ", " + TextCodes.Pulling + " and " + TextCodes.Pushing + " will only operate on their respective targets.\n" +
        //    "If you have selected only " + TextCodes.Pull_targets + " or only " + TextCodes.Push_targets +", " + TextCodes.Pulling + " and " + TextCodes.Pushing + "will operate on any target."
        TriggerBeadMessages[3] = new List<string> {
            "Vier " + TextCodes.Pull,
            "Funf " + TextCodes.WASD,
            "Sechs "  + TextCodes.KeyCoinshotMode
        };

        UpdateText();
    }
}
