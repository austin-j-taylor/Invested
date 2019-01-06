using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * Displays a message over the HUD. Stores strings that represent these messages.
 *      TriggerBeadPopups send these messages.
 *      The strings must be dynamically stored such that they can change with the Control Scheme.
 */
public class MessageOverlayController : MonoBehaviour {

    public Text MessageText { get; private set; }

    public List<string>[] TriggerBeadMessages { get; private set; }

    private TriggerBeadPopup currentPopup;
    public TriggerBeadPopup CurrentPopup {
        get {
            return currentPopup;
        }
        set {
            if(currentPopup)
                currentPopup.Close();
            currentPopup = value;
            MessageText.text = TriggerBeadMessages[currentPopup.section][currentPopup.index];
        }
    }
    
    void Awake() {
        MessageText = GetComponentInChildren<Text>();
        // TriggerBeadPopup strings
        TriggerBeadMessages = new List<string>[4];
        UpdateMessages();
    }

    public void Clear() {
        MessageText.text = "";
    }

    public void UpdateText() {
        if(currentPopup) {
            MessageText.text = currentPopup.GetText();
        }
    }

    // The string "constants" used by TriggerBeadPopups.
    public void UpdateMessages() {
        // TriggerBeadPopup strings
        TriggerBeadMessages[0] = new List<string> {
            TextCodes.KeyPull + " to " + TextCodes.Pull + ".",
            TextCodes.KeyPush + " to " + TextCodes.Push + "."
        };
        TriggerBeadMessages[1] = new List<string> {
            TextCodes.KeyLook + " to look around.\n\n" + TextCodes.KeyMove + " to move.",
            TextCodes.KeyJump + " to jump.",

        };
        TriggerBeadMessages[2] = new List<string> {
            TextCodes.KeyStartBurning + "\n\t\tto start burning " + TextCodes.Iron + " or " + TextCodes.Steel + ".",
            TextCodes.s_Press_ + TextCodes.KeySelect + "\n\t\tto select a metal to be a " + TextCodes.Pull_target + ".\n" +
                TextCodes.s_Press_ + TextCodes.KeySelectAlternate + "\n\t\tto select a metal to be a " + TextCodes.Push_target + ".",
            TextCodes.KeyPull + " to " + TextCodes.Pull + ".\n" +
                TextCodes.KeyPush + " to " + TextCodes.Push + "."
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
