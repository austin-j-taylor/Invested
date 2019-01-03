using UnityEngine;
using System.Collections.Generic;

/*
 * Stores string constants used throughout the project.
 */
public class Messages : ScriptableObject {

    public List<string>[] TriggerBeadMessages { get; private set; }

    private void Awake() {
        // TriggerBeadPopup strings
        TriggerBeadMessages = new List<string>[4];
        TriggerBeadMessages[0] = new List<string> {
            TextCodes.KeyPull + " to " + TextCodes.Pull + ".",
            TextCodes.KeyPush + " to " + TextCodes.Push + "."
        };
        TriggerBeadMessages[1] = new List<string> {
            TextCodes.KeyLook + " to look around.\n\n" + TextCodes.KeyMove + " to move.",
            TextCodes.KeyJump + " to jump.\n\n\n\tCollect that small bead up over the ledge.",

        };
        TriggerBeadMessages[2] = new List<string> {
            TextCodes.KeyStartBurning + "\n\tto start burning " + TextCodes.Iron + " or " + TextCodes.Steel + ".",
            TextCodes.s_Press_ + TextCodes.KeySelect + "\n\tto select a metal to be a " + TextCodes.Pull_target + ".\n" +
                TextCodes.s_Press_ + TextCodes.KeySelectAlternate + "\n\tto select a metal to be a " + TextCodes.Push_target + ".",
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
    }
}