using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/*
 * Contains several static fields referenced in on-screen text.
 * For example, if a text field says "You can Pull metals", the word "Pull" should be in blue.
 * Typing "You can " + TextCodes.Pull + " metals.
 * 
 * Also contains strings that will return a certain controller button depending on the control scheme
 *      i.e. will return "s_Mouse_Button_4" or "Q" or "Left Bumper" depending on the control scheme
 */
public class TextCodes : MonoBehaviour {

    // String constants (prefixed with s_)
    private const string s_the_mouse = "the mouse";
    private const string s_Mouse_Button_4 = "Mouse Button 4";
    private const string s_Mouse_Button_5 = "Mouse Button 5";
    private const string s_Space = "Space";
    private const string s_Ctrl = "Ctrl";
    private const string s_Shift = "Shift";
    private const string s_WASD = "W/A/S/D";
    private const string s_Q = "Q";
    private const string s_E = "E";
    private const string s_A = "A";
    private const string s_X = "X";
    private const string s_Y = "Y";
    private const string s_Left_Trigger = "Left Trigger";
    private const string s_Right_Trigger = "Right Trigger";
    private const string s_Left_Bumper = "Left Bumper";
    private const string s_Right_Bumper = "Right Bumper";
    private const string s_Left_Click = "Left Click";
    private const string s_Right_Click = "Right Click";
    private const string s_Left_Joystick = "the left joystick";
    private const string s_Right_Joystick = "the right joystick";
    private const string s_Up_Down_on_the_D_Pad = "Up/Down on the D-Pad";
    private const string s_Left_Right_on_the_D_Pad = "Left/Right on the D-Pad";

    private const string s_Press_ = "Press ";
    private const string s_Hold_ = "Hold ";
    private const string s_Use_ = "Use ";
    private const string s_Tap_ = "Tap ";
    private const string s_Scroll_ = "Scroll ";
    private const string s_Leftdashclick_ = "Left-click ";
    private const string s_Rightdashclick_ = "Right-click ";


    GameObject[] tester;
    private void Update() {
        tester = GameObject.FindGameObjectsWithTag("Testing");
        tester[0].GetComponent<Text>().text = KeyMove + " to move.";
        tester[1].GetComponent<Text>().text = KeyLook + " to look around.";
        tester[2].GetComponent<Text>().text = KeyJump + " to jump.";
        tester[3].GetComponent<Text>().text = KeyStartBurning + " to start burning " + Iron + " or " + Steel + ".";
        tester[4].GetComponent<Text>().text = s_Press_ + KeySelect + " to select a metal to be a " + Pull_target + ".";
        tester[5].GetComponent<Text>().text = s_Press_ + KeySelectAlternate + " to select a metal to be a " + Push_target + ".";
        tester[6].GetComponent<Text>().text = KeyPull + " to " + Pull + ".";
        tester[7].GetComponent<Text>().text = KeyPush + " to " + Push + ".";
        tester[8].GetComponent<Text>().text = "While holding " + KeyNegate
            + ":\n\t\t• " + s_Hold_ + KeySelect + " while looking at a " + Pull_target
            + " to deselect it.\n\t\t• " + s_Tap_ + KeySelect + " while not looking at a " + Pull_target + " to deselect your oldest " + Pull_target
            + ".\n\t\t Likewise for " + KeySelectAlternate + " and " + Push_targets + ".";
        tester[9].GetComponent<Text>().text = KeyPassiveBurn + " to change which metal you passively burn.";
        tester[10].GetComponent<Text>().text = KeyStopBurning + " to stop burning " + Gray("Iron and Steel.");
        tester[11].GetComponent<Text>().text = KeyPushPullStrength + " to change " + Gray("Push/Pull strength");
        tester[12].GetComponent<Text>().text = KeyNumberOfTargets + " to change the " + Gray("number of Push/Pull-targets.");
    }

    // Methods that accept strings as arguments and return strings colored in their respective color
    public static string Blue(string s) {
        return "<color=#0080ff>" + s + "</color>";
    }
    public static string MidBlue(string s) {
        return "<color=#00bfff>" + s + "</color>";
    }
    public static string LightBlue(string s) {
        return "<color=#7fdfff>" + s + "</color>";
    }
    public static string LightRed(string s) {
        return "<color=#ffbfbf>" + s + "</color>";
    }
    public static string Red(string s) {
        return "<color=#ff8080>" + s + "</color>";
    }
    public static string Gray(string s) {
        return "<color=#bfbfbf>" + s + "</color>";
    }

    // Known words that should always appear in a specific color
    public static string Iron {
        get {
            return Gray("Iron");
        }
    }
    public static string Steel {
        get {
            return Gray("Steel");
        }
    }
    public static string Pull {
        get {
            return MidBlue("Pull");
        }
    }
    public static string Pulling {
        get {
            return LightBlue("Pulling");
        }
    }
    public static string Push {
        get {
            return Red("Push");
        }
    }
    public static string Pushing {
        get {
            return LightRed("Pushing");
        }
    }
    public static string Pull_target {
        get {
            return LightBlue("Pull-target");
        }
    }
    public static string Push_target {
        get {
            return LightRed("Push-target");
        }
    }
    public static string Push_targets {
        get {
            return LightRed("Push-targets");
        }
    }

    // Known inputs that should always appear in a specific color
    public static string LeftClick {
        get {
            return MidBlue(s_Left_Click);
        }
    }
    public static string RightClick {
        get {
            return Red(s_Right_Click);
        }
    }
    public static string RightTrigger {
        get {
            return MidBlue(s_Right_Trigger);
        }
    }
    public static string LeftTrigger {
        get {
            return Red(s_Left_Trigger);
        }
    }
    public static string theScrollWheel {
        get {
            return "the " + Gray("scroll wheel");
        }
    }
    public static string Shift {
        get {
            return Gray(s_Shift);
        }
    }
    public static string Space {
        get {
            return Gray(s_Space);
        }
    }
    public static string A {
        get {
            return Gray(s_A);
        }
    }
    public static string X {
        get {
            return Gray(s_X);
        }
    }
    public static string Y {
        get {
            return Gray(s_Y);
        }
    }



    // Returns the (colored) button input that corresponds to the command, depending on the control scheme
    public static string KeyMove {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Use_ + Gray(s_Left_Joystick);
            else
                return s_Use_ + Gray(s_WASD);
        }
    }
    public static string KeyLook {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Use_ + Gray(s_Right_Joystick);
            else
                return s_Use_ + Gray(s_the_mouse);
        }
    }
    public static string KeyJump {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + A;
            else
                return s_Press_ + Space;
        }
    }
    public static string KeyPull {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + RightTrigger;
            else
                return s_Hold_ + LeftClick;
        }
    }
    public static string KeyPush {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + LeftTrigger;
            else
                return s_Hold_ + RightClick;
        }
    }
    public static string KeySelect {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return LightBlue(s_Mouse_Button_5);
                    }
                case SettingsData.MK45: {
                        return LightBlue(s_Mouse_Button_4);
                    }
                case SettingsData.MKEQ: {
                        return LightBlue(s_E);
                    }
                case SettingsData.MKQE: {
                        return LightBlue(s_Q);
                    }
                default: {
                        return LightBlue(s_Right_Bumper);
                    }
            }
        }
    }
    public static string KeySelectAlternate {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return LightRed(s_Mouse_Button_4);
                    }
                case SettingsData.MK45: {
                        return LightRed(s_Mouse_Button_5);
                    }
                case SettingsData.MKEQ: {
                        return LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return LightRed(s_E);
                    }
                default: {
                        return LightRed(s_Left_Bumper);
                    }
            }
        }
    }
    public static string KeyStartBurning {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return s_Press_ + LightBlue(s_Mouse_Button_5) + " or " + LightRed(s_Mouse_Button_4);
                    }
                case SettingsData.MK45: {
                        return s_Press_ + LightBlue(s_Mouse_Button_4) + " or " + LightRed(s_Mouse_Button_5);
                    }
                case SettingsData.MKEQ: {
                        return s_Press_ + LightBlue(s_E) + " or " + LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return s_Press_ + LightBlue(s_Q) + " or " + LightRed(s_E);
                    }
                default: {
                        return s_Press_ + LightBlue(s_Right_Bumper) + " or " + LightRed(s_Left_Bumper);
                    }
            }
        }
    }
    public static string KeyStopBurning {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return s_Hold_ + Shift + ", " + LightBlue(s_Mouse_Button_5) + ", and " + LightRed(s_Mouse_Button_4);
                    }
                case SettingsData.MK45: {
                        return s_Hold_ + Shift + ", " + LightBlue(s_Mouse_Button_4) + ", and " + LightRed(s_Mouse_Button_5);
                    }
                case SettingsData.MKEQ: {
                        return s_Hold_ + Shift + ", " + LightBlue(s_E) + ", and " + LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return s_Hold_ + Shift + ", " + LightBlue(s_Q) + ", and " + LightRed(s_E);
                    }
                default: {
                        return s_Hold_ + Y + ", " + LightBlue(s_Right_Bumper) + ", and " + LightRed(s_Left_Bumper);
                    }
            }
        }
    }
    public static string KeyPassiveBurn {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Tap_ + RightTrigger + " or " + LeftTrigger;
            else
                return s_Tap_ + LeftClick + " or " + RightClick;
        }
    }
    public static string KeyNegate {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return Y;
            else
                return Shift;
        }
    }
    public static string KeyPushPullStrength {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Change the pressure on " + LeftTrigger + " and " + RightTrigger;
            else
                return s_Scroll_ + theScrollWheel;
        }
    }
    public static string KeyNumberOfTargets {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Press " + Gray("Up/Down") + " on the D-Pad";
            else
                return s_Press_ + "and " + s_Scroll_ + theScrollWheel;
        }
    }


}
