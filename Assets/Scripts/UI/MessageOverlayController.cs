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

    private const int numberOfMessages = 15;

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
            KeyJump + " to jump.\n\n\tCollect the vial of iron and the glowing bead."
        };
        TriggerBeadMessages[1] = new List<string> {
            "Pulling",
            KeyStartBurningIron + "\n\t\tto start burning " + Iron + ".",
            s_Hold_ + _KeyPull + " to " + Pull + ".\n"
        };
        TriggerBeadMessages[2] = new List<string> {
            "Pushing & Pulling Basics",
            s_Press_ + _KeySelect + " to select a metal to be a " + Pull_target + ".\n" + 
            s_Press_ + _KeySelectAlternate + " to select a metal to be a " + Push_target + ".",
            s_Hold_ + _KeyPull + " to " + Pull + ".\n" +
                s_Hold_ + _KeyPush + " to " + Push + ".",
            "While holding " + KeyNegate + ", " + s_Press_ + _KeySelect + "\n\t\t to deselect a " + Pull_target +
                ".\nLikewise for " + _KeySelectAlternate + " and " + Push_targets + ".",
            "Get familiar with these controls before proceeding.\n" +
            KeyHelp + " to toggle the " + HelpOverlay + ".\n"
        };
        TriggerBeadMessages[3] = new List<string> {
            "Pulling",
            "Cross the pit by " + Pulling + " yourself accross."
        };
        TriggerBeadMessages[4] = new List<string> {
            "Pushing",
            "Cross the pit by " + Pushing + " yourself accross."
        };
        TriggerBeadMessages[5] = new List<string> {
            "Advanced Pushing & Pulling",
            KeyNumberOfTargets + " to change your " + Gray("max number of " + Push_Pull_targets + ".") +
                "\nYou can target multiple metals by increasing this number.",
            KeyPushPullStrength + " to change " + Push_Pull + " " + BurnPercentage +
                ".\nUse this to vary the strength of your " + Pushes_and_Pulls + ".",
                "\n\n\tLook up. Balance in the air near the " + O_SeekerCube + "."
        };
        TriggerBeadMessages[6] = new List<string> {
            "Advanced Pushing & Pulling",
            KeyStopBurning + " to stop burning " + Gray("Iron and Steel") +
                (SettingsMenu.settingsData.controlScheme == SettingsData.MKQE || SettingsMenu.settingsData.controlScheme == SettingsData.MKEQ ? 
                    ".\n\t(Your keyboard may not support that last option.)" :
                    "."
                )
        };
        TriggerBeadMessages[7] = new List<string> {
            "Advanced Movement",
            KeyWalk + " to anchor yourself. This increases your moment of inertia and makes you move slower."
        };
        TriggerBeadMessages[8] = new List<string> {
            "Advanced Movement - Pewter",
            KeySprint + " to burn " + Pewter + ".",
            "While burning " + Pewter  + ":\n\t\t• Move to " + Sprint + ".\n\t\t• Jump to " + PewterJump + "."
        };
        TriggerBeadMessages[9] = new List<string> {
            "Advanced Movement - Pewter",
            PewterJump + " while not trying to " + Sprint + " to jump straight up and higher."
        };
        TriggerBeadMessages[10] = new List<string> {
            "Advanced Movement - Pewter",
            PewterJump + " while touching a wall\n\t\t• while trying to move away from the wall to kick off of the wall\n\t\t• while trying to move into the wall to wall jump up\n\n\tWall jump up the crevice.\n\n\t" + Pewter + " burns quickly, so refill at a vial if you run out."
        };
        TriggerBeadMessages[11] = new List<string> {
            "Coins",
            s_Hold_ + _KeyPull + " near " + O_Coins + " to pick them up.",
            KeyThrow + " to toss a coin in front of you. Try " + Pushing + " on it as you throw.", 
            KeyDrop + " to drop a coin at your feet. Try " + Pushing + " on it.",
            "\t\t• " + KeyDropDirection + " while dropping a coin to toss the coin away from that direction.\n\n\tScale the wall using " + O_Coins + " and " + Pewter + 
                ".\n\n\t(Hint: multi-targeting is your best friend when using coins.)\n\t(" + KeyHelp + " to toggle the " + HelpOverlay + ".)"
        };
        TriggerBeadMessages[12] = new List<string> {
            "Balancers",
            "Each trio of red, blue, and grey cubes are a Balancer.\n" +
            "The blue cube Pulls on the grey metal cube, while the red cube Pushes on it.\n" +
            "They vary their Push/Pull strength to keep the metal cube in equilibrium, countering gravity.\n" +
            "The red/blue cubes follow the metal cube's position and direction to try to keep it balanced.\n\n" +
            "Some initial conditions are more stable than others; reset the scene to see how they start."
        };
        TriggerBeadMessages[13] = new List<string> {
            "Zinc Peripheral",
            KeyZincTime + ".\n" +
            "The sphere's processing speed significantly accelerates, giving you more time to react to the world around you.\n" +
            "The zinc bank automatically recharges by drawing speed from a slave processor, but it does run out eventually.\n\n" +
            "See the Articles for more details."
        };
        TriggerBeadMessages[14] = new List<string> {
            "Coinshot Mode",
            KeyCoinshotMode + " to activate " + CoinshotMode
                + ".\n\t\t• While in " + CoinshotMode + ", " + KeyCoinshotThrow
                + " to throw coins.\n\t\t" + KeyCoinshotMode + " again to disable " + CoinshotMode + ".\n\n"
                //+ "You can't " + Push + " or " + Pull + " without targeting in " + CoinshotMode + "."
        };


        // after teaching to deselect
        //"If you have both " + Pull_targets + " and " + Push_targets + ", " + Pulling + " and " + Pushing + " will only operate on their respective targets.\n" +
        //    "If you have selected only " + Pull_targets + " or only " + Push_targets +", " + Pulling + " and " + Pushing + "will operate on any target."

        //KeyCoinshotMode + " to activate " + CoinshotMode
        //    + ".\n\t\t• While in " + CoinshotMode + ", " + KeyCoinshotThrow
        //    + " to throw coins.\n\t\t" + KeyCoinshotMode + " again to disable " + CoinshotMode + ".";

        UpdateText();
    }
}
