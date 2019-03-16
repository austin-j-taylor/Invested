using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/*
 * Contains several static fields referenced in on-screen text.
 * For example, if a text field says "You can Pull metals", the word "Pull" should be in blue.
 *      -> "You can " + TextCodes.Pull + " metals."
 * 
 * Also contains strings that will return a certain controller button depending on the control scheme
 *      i.e. will return "Mouse Button 4" or "Q" or "Left Bumper" depending on the control scheme.
 */
public class TextCodes : MonoBehaviour {

    // String constants (prefixed with s_)
    public const string s_Mouse_Button_4 = "Mouse Button 4";
    public const string s_Mouse_Button_5 = "Mouse Button 5";
    public const string s_E = "E";
    public const string s_Q = "Q";
    public const string s_Left_Trigger = "Left Trigger";
    public const string s_Right_Trigger = "Right Trigger";
    public const string s_Left_Bumper = "Left Bumper";
    public const string s_Right_Bumper = "Right Bumper";
    public const string s_Left_Click = "Left Click";
    public const string s_Right_Click = "Right Click";
    public const string s_Up_Down_on_the_D_Pad = "Up/Down on the D-Pad";
    public const string s_Left_Right_on_the_D_Pad = "Left/Right on the D-Pad";

    public const string s_Press_ = "Press ";
    public const string s_Hold_ = "Hold ";
    public const string s_Use_ = "Use ";
    public const string s_Tap_ = "Tap ";
    public const string s_Scroll_ = "Scroll ";
    public const string s_Leftdashclick_ = "Left-click ";
    public const string s_Rightdashclick_ = "Right-click ";


    GameObject[] tester;
    private void Update() {
        tester = GameObject.FindGameObjectsWithTag("Testing");
        tester[13].GetComponent<Text>().text = KeyThrow + " to throw a coin in front of you. Try " + Pushing + " on it as you throw.";
        tester[14].GetComponent<Text>().text = KeyDrop + " to drop a coin at your feet. Try " + Pushing + " on it."
            + "\n\t\t• " + KeyDropDirection + " while dropping a coin will drop the coin in the opposite direction.";
        tester[15].GetComponent<Text>().text = Pull + " a coin into you to " + Gray("catch") + " it.";
        //tester[16].GetComponent<Text>().text = KeySwap + " to swap your " + Push_targets + " and " + Pull_targets + ".";
        tester[17].GetComponent<Text>().text = KeyCoinshotMode + " to activate " + CoinshotMode
            + ".\n\t\t• While in " + CoinshotMode + ", " + KeyCoinshotThrow
            + " to throw coins.\n\t\t" + KeyCoinshotMode + " again to disable " + CoinshotMode + ".";
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
    public static string Gold(string s) {
        return "<color=#fff080>" + s + "</color>";
    }
    public static string Orange(string s) {
        return "<color=#bf3f00>" + s + "</color>";
    }
    public static string Bronze(string s) {
        return "<color=#ff9f00>" + s + "</color>";
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
    public static string Pewter {
        get {
            return Gray("Pewter");
        }
    }
    public static string Pull {
        get {
            return MidBlue("Pull");
        }
    }
    public static string Pulls {
        get {
            return MidBlue("Pulls");
        }
    }
    public static string Pulling {
        get {
            return MidBlue("Pulling");
        }
    }
    public static string Push {
        get {
            return Red("Push");
        }
    }
    public static string Pushes {
        get {
            return Red("Pushes");
        }
    }
    public static string Pushing {
        get {
            return Red("Pushing");
        }
    }
    public static string Push_Pull {
        get {
            return Push + '/' + Pull;
        }
    }
    public static string Pull_or_Push {
        get {
            return Pull + " or " + Push;
        }
    }
    public static string Pushes_and_Pulls {
        get {
            return Pushes + " and " + Pulls;
        }
    }
    public static string Pull_target {
        get {
            return LightBlue("Pull-target");
        }
    }
    public static string Pull_targets {
        get {
            return LightBlue("Pull-targets");
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
    public static string Push_Pull_targets {
        get {
            return Push + '/' + LightBlue("Pull-targets");
        }
    }
    public static string Pull_or_Push_target {
        get {
            return Pull + LightBlue("-") +" or " + Red("Push-target");
        }
    }
    public static string CoinshotMode {
        get {
            return Gold("Coinshot mode");
        }
    }
    public static string BurnPercentage {
        get {
            return Gray("Burn Percentage");
        }
    }
    public static string Sprint {
        get {
            return Orange("Sprint");
        }
    }
    public static string PewterJump {
        get {
            return Orange("Pewter Jump");
        }
    }
    // Objects, prefixed with "O_"
    public static string O_SeekerCube {
        get {
            return Bronze("Seeker Cube");
        }
    }
    public static string O_Coin {
        get {
            return Gold("Coin");
        }
    }
    public static string O_Coins {
        get {
            return Gold("Coins");
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
    public static string theLeftJoystick {
        get {
            return "the " + Gray("left joystick");
        }
    }
    public static string theRightJoystick {
        get {
            return "the " + Gray("right joystick");
        }
    }
    public static string theMouse {
        get {
            return "the " + Gray("mouse");
        }
    }
    public static string Shift {
        get {
            return Gray("Shift");
        }
    }
    public static string Space {
        get {
            return Gray("Space");
        }
    }
    public static string Ctrl {
        get {
            return Gray("Ctrl");
        }
    }
    public static string LeftAlt {
        get {
            return Gray("Left Alt");
        }
    }
    public static string Tab {
        get {
            return Gray("Tab");
        }
    }
    public static string WASD {
        get {
            return Gray("W/A/S/D");
        }
    }
    public static string H {
        get {
            return Gray("H");
        }
    }
    public static string C {
        get {
            return Gold("C");
        }
    }
    public static string A {
        get {
            return Gray("A");
        }
    }
    public static string X {
        get {
            return Gray("X");
        }
    }
    public static string Y {
        get {
            return Gray("Y");
        }
    }



    // Returns the (colored) button input that corresponds to the command, depending on the control scheme
    public static string KeyMove {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Use_ + theLeftJoystick;
            else
                return s_Use_ + WASD;
        }
    }
    public static string KeyWalk {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Use_ + theLeftJoystick;
            else
                return s_Hold_ + Ctrl;
        }
    }
    public static string KeySprint {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + "XXX";
            else
                return s_Hold_ + Shift;
        }
    }
    public static string KeyLook {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Use_ + theRightJoystick;
            else
                return s_Use_ + theMouse;
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
    public static string KeyPullOrPush {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + RightTrigger + " or " + LeftTrigger;
            else
                return s_Hold_ + LeftClick + " or " + RightClick;
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
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Hold_ + Tab + ", " + LightBlue(s_Mouse_Button_5) + ", and " + LightRed(s_Mouse_Button_4);
                    }
                case SettingsData.MK45: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Hold_ + Tab + ", " + LightBlue(s_Mouse_Button_4) + ", and " + LightRed(s_Mouse_Button_5);
                    }
                case SettingsData.MKEQ: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Hold_ + Tab + ", " + LightBlue(s_E) + ", and " + LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Hold_ + Tab + ", " + LightBlue(s_Q) + ", and " + LightRed(s_E);
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
                return Tab;
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
                return s_Press_ + Gray("Up/Down") + " on the D-Pad";
            else
                return s_Press_ + KeyNegate + " and scroll " + theScrollWheel;
        }
    }
    public static string KeyThrow {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + X;
            else
                return s_Press_ + Ctrl;
        }
    }
    public static string KeyDrop {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + A + " and press " + X;
            else
                return s_Hold_ + Space + " and press " + Ctrl;
        }
    }
    public static string KeyDropDirection {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Tilting " + theLeftJoystick;
            else
                return "Holding " + WASD;
        }
    }
    public static string KeySwap {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Double-tap " + Y;
            else
                return "Double-tap " + Tab;
        }
    }
    public static string KeyCoinshotMode {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Click in " + Gold("the right joystick");
            else
                return "Tap " + C;
        }
    }
    public static string KeyCoinshotThrow {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "hold " + LeftTrigger + " and press " + RightTrigger + " (or vice-versa)";
            else
                return "hold " + RightClick + " and press " + LeftClick + " (or vice-versa)";
        }
    }
    public static string KeyHelp {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + "UNIMPLEMENTED";
            else
                return s_Press_ + H;
        }
    }


}
