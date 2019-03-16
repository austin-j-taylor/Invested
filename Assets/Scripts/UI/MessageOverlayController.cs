﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static TextCodes;

/*
 * Displays a message over the HUD. Stores strings that represent these messages.
 *      TriggerBeadPopups send these messages.
 *      The strings must be dynamically stored such that they can change with the Control Scheme.
 */
public class MessageOverlayController : MonoBehaviour {

    private const int numberOfMessages = 11;

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
    // Must be assigned at runtime because changing the control scheme changes the contents of each string
    // Not the most efficient, but hey, at least it's not Java
    public void UpdateMessages() {
        // TriggerBeadPopup strings
        TriggerBeadMessages[0] = new List<string> {
            "Movement",
            KeyLook + " to look around.\n\n" + KeyMove + " to move.",
            KeyJump + " to jump.\n\n\tCollect the vial of metals and the glowing bead.",

        };
        TriggerBeadMessages[1] = new List<string> {
            "Pushing & Pulling Basics",
            KeyStartBurning + "\n\t\tto start burning " + Iron + " or " + Steel + ".",
            s_Press_ + KeySelect + "\n\t\tto select a metal to be a " + Pull_target + ".\n" +
                s_Press_ + KeySelectAlternate + "\n\t\tto select a metal to be a " + Push_target + ".",
            KeyPull + " to " + Pull + ".\n" +
                KeyPush + " to " + Push + ".",
            "While holding " + KeyNegate + ", " + s_Press_ + KeySelect + "\n\t\t to deselect a " + Pull_target +
                ".\nLikewise for " + KeySelectAlternate + " and " + Push_targets + ".",
            KeyHelp + " to toggle the Help Overlay.\nPlay around a bit before proceeding."
        };
        TriggerBeadMessages[2] = new List<string> {
            "Pulling",
            "Cross the pit by " + Pulling + " yourself accross."
        };
        TriggerBeadMessages[3] = new List<string> {
            "Pushing",
            "Cross the pit by " + Pushing + " yourself accross."
        };
        TriggerBeadMessages[4] = new List<string> {
            "Advanced Pushing & Pulling",
            KeyNumberOfTargets + " to change your " + Gray("max number of " + Push_Pull_targets + ".") +
                "\nYou can target multiple metals by increasing this number.",
            KeyPushPullStrength + " to change " + Push_Pull + " " + BurnPercentage +
                ".\nUse this to vary the strength of your " + Pushes_and_Pulls + ".",
                "\n\n\tLook up. Balance in the air near the " + O_SeekerCube + "."
        };
        TriggerBeadMessages[5] = new List<string> {
            "Advanced Pushing & Pulling",
            KeyStopBurning + " to stop burning " + Gray("Iron and Steel.")
        };
        TriggerBeadMessages[6] = new List<string> {
            "Advanced Movement",
            KeyWalk + " to walk slowly."
        };
        TriggerBeadMessages[7] = new List<string> {
            "Advanced Movement - Pewter",
            KeySprint + " to burn " + Pewter + ". While burning " + Pewter  + ":\n\t\t• Move to " + Sprint + ".\n\t\t• Jump to " + PewterJump + "."
        };
        TriggerBeadMessages[8] = new List<string> {
            "Advanced Movement - Pewter",
            PewterJump + " while not trying to " + Sprint + " to jump straight up and higher."
        };
        TriggerBeadMessages[9] = new List<string> {
            "Advanced Movement - Pewter",
            PewterJump + " while touching a wall and:\n\t\t• trying to move away from the wall to kick off of the wall.\n\t\t• trying to move into the wall to wall jump up.\n\n\tWall jump up the crevice."
        };
        TriggerBeadMessages[10] = new List<string> {
            "Coins",
            "Collect the " + O_Coins + ".",
            "throw it"
        };
        // after teaching to deselect
        //"If you have both " + Pull_targets + " and " + Push_targets + ", " + Pulling + " and " + Pushing + " will only operate on their respective targets.\n" +
        //    "If you have selected only " + Pull_targets + " or only " + Push_targets +", " + Pulling + " and " + Pushing + "will operate on any target."

        UpdateText();
    }
}
