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
    public const string s_Mouse_Button_3 = "Mouse Button 3";
    public const string s_Mouse_Button_4 = "Mouse Button 4";
    public const string s_Mouse_Button_5 = "Mouse Button 5";
    public const string s_Mouse_Button_4Abridged = "MB4";
    public const string s_Mouse_Button_5Abridged = "MB5";
    public const string s_E = "E";
    public const string s_Q = "Q";
    public const string s_Left_Trigger = "Left Trigger";
    public const string s_Right_Trigger = "Right Trigger";
    public const string s_Left_Bumper = "Left Bumper";
    public const string s_Right_Bumper = "Right Bumper";
    public const string s_Left_BumperAbridged = "LB";
    public const string s_Right_BumperAbridged = "RB";
    public const string s_Left_Click = "Left-click";
    public const string s_Right_Click = "Right-click";
    public const string s_Up_Down_on_the_D_Pad = "Up/Down on the D-Pad";
    public const string s_Left_Right_on_the_D_Pad = "Left/Right on the D-Pad";

    public const string s_Press_ = "Press ";
    public const string s_Pressing_ = "Pressing ";
    public const string s_Hold_ = "Hold ";
    public const string s_Click_in_ = "Click in ";
    public const string s_Use_ = "Use ";
    public const string s_Tap_ = "Tap ";
    public const string s_Scroll_ = "Scroll ";
    
    // Opening tags for certain colors
    public static string Blue_Open() {
        return "<color=#0080ff>";
    }
    public static string MidBlue_Open() {
        return "<color=#00bfff>";
    }
    public static string LightBlue_Open() {
        return "<color=#7fdfff>";
    }
    public static string Gray_Open() {
        return "<color=#bfbfbf>";
    }
    public static string Red_Open() {
        return "<color=#ff8080>";
    }
    // Methods that accept strings as arguments and return strings colored in their respective color
    public static string Blue(string s) {
        return Blue_Open() + s + "</color>";
    }
    public static string MidBlue(string s) {
        return MidBlue_Open() + s + "</color>";
    }
    public static string LightBlue(string s) {
        return LightBlue_Open() + s + "</color>";
    }
    public static string ZincBlue(string s) {
        return "<color=#c1dbff>" + s + "</color>";
    }
    public static string LightRed(string s) {
        return "<color=#ffbfbf>" + s + "</color>";
    }
    public static string Red(string s) {
        return Red_Open() + s + "</color>";
    }
    public static string Gray(string s) {
        return Gray_Open() + s + "</color>";
    }
    public static string Gold(string s) {
        return "<color=#fff080>" + s + "</color>";
    }
    public static string Orange(string s) {
        return "<color=#ff9d60>" + s + "</color>";
    }
    public static string Bronze(string s) {
        return "<color=#ff9f00>" + s + "</color>";
    }
    // Same as above, but for specific characters
    public static string Color_Kog(string s) {
        return Gray(s);
    }
    public static string Color_Prima(string s) {
        return LightBlue(s);
    }
    public static string Color_Machines(string s) {
        return (s);
    }
    public static string Color_Kog_Open() {
        return Gray_Open();
    }
    public static string Color_Prima_Open() {
        return LightBlue_Open();
    }
    public static string Color_Machines_Open() {
        return ("");
    }
    // Same as above, but for other key words
    public static string Color_Location_Open() {
        return Blue_Open();
    }
    public static string Color_Pull_Open() {
        return MidBlue_Open();
    }
    public static string Color_Push_Open() {
        return Red_Open();
    }

    // Known words that should always appear in a specific color
    public static string Iron {
        get {
            return Gray("iron");
        }
    }
    public static string Steel {
        get {
            return Gray("steel");
        }
    }
    public static string Pewter {
        get {
            return Orange("pewter");
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
    public static string Pull_Push {
        get {
            return Pull + '/' + Push;
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
    public static string Mark_pulling {
        get {
            return LightBlue("Mark");
        }
    }
    public static string Marked_metal{
        get {
            return LightBlue("Marked metal");
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
    public static string CoinshotMode {
        get {
            return Gold("Coinshot mode");
        }
    }
    public static string ZincTime {
        get {
            return ZincBlue("Zinc Time");
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
    public static string Sprinting {
        get {
            return Orange("Sprinting");
        }
    }
    public static string PewterJump {
        get {
            return Orange("Pewter Jump");
        }
    }
    public static string PewterJumping {
        get {
            return Orange("Pewter Jumping");
        }
    }
    public static string HelpOverlay {
        get {
            return Gray("Help Overlay");
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
    // Gamepad
    public static string LeftClick {
        get {
            return MidBlue(s_Left_Click);
        }
    }
    public static string LeftClickAbridged {
        get {
            return MidBlue("LMB");
        }
    }
    public static string RightClick {
        get {
            return Red(s_Right_Click);
        }
    }
    public static string RightClickAbridged {
        get {
            return Red("RMB");
        }
    }
    public static string RightTrigger {
        get {
            return MidBlue(s_Right_Trigger);
        }
    }
    public static string RightTriggerAbridged {
        get {
            return MidBlue("RT");
        }
    }
    public static string LeftTrigger {
        get {
            return Red(s_Left_Trigger);
        }
    }
    public static string LeftTriggerAbridged {
        get {
            return Red("LT");
        }
    }
    public static string mouseButton3 {
        get {
            return Gray(s_Mouse_Button_3);
        }
    }
    public static string theScrollWheel {
        get {
            return "the " + Gray("scroll wheel");
        }
    }
    public static string scrollWheel {
        get {
            return Gray("scroll wheel");
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
    public static string Back {
        get {
            return Gold("Back");
        }
    }
    public static string Start {
        get {
            return Gray("Start");
        }
    }
    public static string A {
        get {
            return Gray("A");
        }
    }
    public static string B {
        get {
            return Gray("B");
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
    // Mouse/Keyboard
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
            return ZincBlue("Tab");
        }
    }
    public static string R {
        get {
            return Gray("R");
        }
    }
    public static string WASD {
        get {
            return Gray("W/A/S/D");
        }
    }
    public static string Escape {
        get {
            return Gray("Escape");
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
                return s_Click_in_ + theRightJoystick;
            else
                return s_Hold_ + Ctrl;
        }
    }
    public static string KeySprint {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + B;
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
                return "press " + A;
            else
                return "press " + Space;
        }
    }
    public static string _KeyPull {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return RightTrigger;
            else
                return LeftClick;
        }
    }
    public static string _KeyPullAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return RightTriggerAbridged;
            else
                return LeftClickAbridged;
        }
    }
    public static string _KeyPush {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return LeftTrigger;
            else
                return RightClick;
        }
    }
    public static string _KeyPushAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return LeftTriggerAbridged;
            else
                return RightClickAbridged;
        }
    }
    public static string KeyZincTime {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Click_in_ + "the " + ZincBlue("left joystick");
            else
                return s_Hold_ + Tab;
        }
    }
    public static string _KeySelect {
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
    public static string _KeySelectAbridged {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return LightBlue(s_Mouse_Button_5Abridged);
                    }
                case SettingsData.MK45: {
                        return LightBlue(s_Mouse_Button_4Abridged);
                    }
                case SettingsData.MKEQ: {
                        return LightBlue(s_E);
                    }
                case SettingsData.MKQE: {
                        return LightBlue(s_Q);
                    }
                default: {
                        return LightBlue(s_Right_BumperAbridged);
                    }
            }
        }
    }
    public static string _KeySelectAlternate {
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
    public static string _KeySelectAlternateAbridged {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return LightRed(s_Mouse_Button_4Abridged);
                    }
                case SettingsData.MK45: {
                        return LightRed(s_Mouse_Button_5Abridged);
                    }
                case SettingsData.MKEQ: {
                        return LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return LightRed(s_E);
                    }
                default: {
                        return LightRed(s_Left_BumperAbridged);
                    }
            }
        }
    }
    public static string KeyStartBurning {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return LeftClick + ", " + RightClick + ", or " + s_Press_ + LightBlue(s_Mouse_Button_5) + " or " + LightRed(s_Mouse_Button_4);
                    }
                case SettingsData.MK45: {
                        return LeftClick + ", " + RightClick + ", or " + s_Press_ + LightBlue(s_Mouse_Button_4) + " or " + LightRed(s_Mouse_Button_5);
                    }
                case SettingsData.MKEQ: {
                        return LeftClick + ", " + RightClick + ", or " + s_Press_ + LightBlue(s_E) + " or " + LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return LeftClick + ", " + RightClick + ", or " + s_Press_ + LightBlue(s_Q) + " or " + LightRed(s_E);
                    }
                default: {
                        return s_Press_ + LeftTrigger + ", " + RightTrigger + ", " + LightBlue(s_Right_Bumper) + ", or " + LightRed(s_Left_Bumper);
                    }
            }
        }
    }
    public static string KeyStartBurningIron {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + LightBlue(s_Right_Bumper);
            else
                return LeftClick;
        }
    }

    public static string KeyStopBurning {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Press_ + Gray("X") + " or " + s_Hold_ + R + ", " + LightBlue(s_Mouse_Button_5) + ", and " + LightRed(s_Mouse_Button_4);
                    }
                case SettingsData.MK45: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Press_ + Gray("X") + " or " + s_Hold_ + R + ", " + LightBlue(s_Mouse_Button_4) + ", and " + LightRed(s_Mouse_Button_5);
                    }
                case SettingsData.MKEQ: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Press_ + Gray("X") + " or " + s_Hold_ + R + ", " + LightBlue(s_E) + ", and " + LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return "Decrease " + BurnPercentage + " to 0% or " + s_Press_ + Gray("X") + " or " + s_Hold_ + R + ", " + LightBlue(s_Q) + ", and " + LightRed(s_E);
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
                return "(" + R + " or " + LeftAlt + ")";
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
                return s_Press_ + mouseButton3;
        }
    }
    public static string KeyDrop {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + A + " and press " + X;
            else
                return s_Hold_ + Space + " and press " + mouseButton3;
        }
    }
    public static string KeyDropDirection {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Use_ + theLeftJoystick;
            else
                return s_Hold_ + WASD;
        }
    }
    public static string KeySwap {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Double-tap " + Y;
            else
                return "Double-tap " + R;
        }
    }
    public static string KeyCoinshotMode {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + Back;
            else
                return "Tap " + C;
        }
    }
    public static string KeyCoinshotThrow {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + LeftTrigger + " and press " + RightTrigger + " (or vice-versa)";
            else
                return s_Hold_ + RightClick + " and press " + LeftClick + " (or vice-versa)";
        }
    }
    public static string KeyHelp {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ +  Start + " > Settings > Interface > Help Overlay";
            else
                return s_Press_ + H + " or " + s_Press_ + Escape + " > Settings > Interface > Help Overlay";
        }
    }


}
