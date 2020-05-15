using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static TextCodes;

/*
 * Stores the strings written to the message overlays.
 * I'm considering somehow restructuring this so it reads from file,
 * but I'd have to think about the formatting for the Text Codes.
 */

public class Messages : MonoBehaviour {

    // Each List of strings represents a series of messages that logicially appear after each other.
    // If it displays on the MessageOverlayDescriptive, the first element of each list is the header for that series of messages.
    public static List<string> marl_movement;
    public static List<string> marl_targeting;
    public static List<string> pulling0;
    public static List<string> pushingAndPullingBasics;
    public static List<string> pulling1;
    public static List<string> pulling2;
    public static List<string> advancedPushingAndPulling0;
    public static List<string> advancedPushingAndPulling1;
    public static List<string> advancedMovement;
    public static List<string> pewter0;
    public static List<string> pewter1;
    public static List<string> pewter2;
    public static List<string> coins;
    public static List<string> balancers;
    public static List<string> zinc;
    public static List<string> coinshot;

    public static List<string> tutorial_look,
                                tutorial_move,
                                tutorial_pull,
                                tutorial_mark;
    

    /* Called whenever certain settings change (i.e. the control scheme).
     * The strings on the message overlays change to reflect the new settings.
     */
    public static void Refresh() {
        tutorial_look = new List<string> {
            KeyLook + " to look around.",
        };
        tutorial_move = new List<string> {
            KeyMove + " to move.",
        };
        tutorial_pull = new List<string> {
            KeyStartBurningIron + " to start burning " + Iron + ".",
            "Look at a " + LightBlue("blue line") + " and " + s_Hold_ + _KeyPull + " to " + Pull + "."
        };
        tutorial_mark = new List<string> {
            s_Press_ + _KeySelect + " while looking at a metal to " + Mark_pulling + " it for " + Pulling + ".\n"
             + "You can " + Pull + " on a " + Marked_metal + " without looking at it.",
            "You can " + Mark_pulling + " multiple metals at once.",
            "To remove a " + Marked_metal + ", " + s_Press_ + _KeySelect + " while looking at it.\nPressing Z also unmarks all metals."
        };
        marl_movement = new List<string> {
            KeyLook + " to look around.",
            KeyStartBurningIron + " to start burning " + Iron + ".",
            "Look at the " + LightBlue("blue line") + ".\n" + s_Hold_ + _KeyPull + " to " + Pull + ".",
            KeyMove + " to move."
        };
        marl_targeting = new List<string> {
            KeyWalk + " to anchor yourself.",
            s_Press_ + _KeySelect + " to select a " + Pull_target + ".\n"
             + "You can " + Pull + " on a " + Pull_target + " without looking at it.",
            KeyNumberOfTargets + " to change your " + Gray("max number of " + Pull_targets) + ".",
            //s_Press_ + _KeySelect + " while holding " + KeyNegate + " to deselect a " + Pull_target + ".",

        };
        pulling0 = new List<string> {
            "Pulling",
            KeyStartBurningIron + "\n\t\tto start burning " + Iron + ".",
            s_Hold_ + _KeyPull + " to " + Pull + ".\n"
        };
        pushingAndPullingBasics = new List<string> {
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
        pulling1 = new List<string> {
            "Pulling",
            "Cross the pit by " + Pulling + " yourself accross."
        };
        pulling2 = new List<string> {
            "Pushing",
            "Cross the pit by " + Pushing + " yourself accross."
        };
        advancedPushingAndPulling0 = new List<string> {
            "Advanced Pushing & Pulling",
            KeyNumberOfTargets + " to change your " + Gray("max number of " + Push_Pull_targets + ".") +
                "\nYou can target multiple metals by increasing this number.",
            KeyPushPullStrength + " to change " + Push_Pull + " " + BurnPercentage +
                ".\nUse this to vary the strength of your " + Pushes_and_Pulls + ".",
                "\n\n\tLook up. Balance in the air near the " + O_SeekerCube + "."
        };
        advancedPushingAndPulling1 = new List<string> {
            "Advanced Pushing & Pulling",
            KeyStopBurning + " to stop burning " + Gray("Iron and Steel") +
                (SettingsMenu.settingsData.controlScheme == SettingsData.MKQE || SettingsMenu.settingsData.controlScheme == SettingsData.MKEQ?
                    ".\n\t(Your keyboard may not support that last option.)" :
                    "."
                )
        };
        advancedMovement = new List<string> {
            "Advanced Movement",
            KeyWalk + " to anchor yourself. This increases your moment of inertia and makes you move slower."
        };
        pewter0 = new List<string> {
            "Advanced Movement - Pewter",
            KeySprint + " to burn " + Pewter + ".",
            "While burning " + Pewter  + ":\n\t\t• Move to " + Sprint + ".\n\t\t• Jump to " + PewterJump + "."
        };
        pewter1 = new List<string> {
            "Advanced Movement - Pewter",
            PewterJump + " while not trying to " + Sprint + " to jump straight up and higher."
        };
        pewter2 = new List<string> {
            "Advanced Movement - Pewter",
            PewterJump + " while touching a wall\n\t\t• while trying to move away from the wall to kick off of the wall\n\t\t• while trying to move into the wall to wall jump up\n\n\tWall jump up the crevice.\n\n\t" + Pewter + " burns quickly, so refill at a vial if you run out."
        };
        coins = new List<string> {
            "Coins",
            s_Hold_ + _KeyPull + " near " + O_Coins + " to pick them up.",
            KeyThrow + " to toss a coin in front of you. Try " + Pushing + " on it as you throw.",
            KeyDrop + " to drop a coin at your feet. Try " + Pushing + " on it.",
            "\t\t• " + KeyDropDirection + " while dropping a coin to toss the coin away from that direction.\n\n\tScale the wall using " + O_Coins + " and " + Pewter +
                ".\n\n\t(Hint: multi-targeting is your best friend when using coins.)\n\t(" + KeyHelp + " to toggle the " + HelpOverlay + ".)"
        };
        balancers = new List<string> {
            "Balancers",
            "Each trio of red, blue, and grey cubes are a Balancer.\n" +
            "The blue cube Pulls on the grey metal cube, while the red cube Pushes on it.\n" +
            "They vary their Push/Pull strength to keep the metal cube in equilibrium, countering gravity.\n" +
            "The red/blue cubes follow the metal cube's position and direction to try to keep it balanced.\n\n" +
            "Some initial conditions are more stable than others; reset the scene to see how they start."
        };
        zinc = new List<string> {
            "Zinc Peripheral",
            KeyZincTime  + " to activate " + ZincTime + ".\n" +
            "The sphere's processing speed significantly accelerates, giving you more time to react to the world around you.\n" +
            "The zinc bank automatically recharges by drawing speed from a slave processor, but it does run out eventually.\n\n" +
            "See the Articles for more details."
        };
        coinshot = new List<string> {
        "Coinshot Mode",
            KeyCoinshotMode + " to activate " + CoinshotMode +
                ".\n\t\t• While in " + CoinshotMode + ", " + KeyCoinshotThrow +
                " to throw coins.\n\t\t" + KeyCoinshotMode + " again to disable " + CoinshotMode + ".\n\n"
            //+ "You can't " + Push + " or " + Pull + " without targeting in " + CoinshotMode + "."
        };
    }

    public void RefreshNonStatic() { // needed for Editor calls
        Refresh();
    }
}
