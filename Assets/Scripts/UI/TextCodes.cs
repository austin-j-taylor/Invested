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
    public const string s_Scroll_Wheel = "Scroll Wheel";
    public const string s_Mouse_Button_4Abridged = "MB4";
    public const string s_Mouse_Button_5Abridged = "MB5";
    public const string s_E = "E";
    public const string s_Q = "Q";
    public const string s_Left_Trigger = "Left Trigger";
    public const string s_Right_Trigger = "Right Trigger";
    public const string s_Left_Bumper = "Left Bumper";
    public const string s_Right_Bumper = "Right Bumper";
    public const string s_Left_Joystick = "Left Joystick";
    public const string s_Right_Joystick = "Right Joystick";
    public const string s_Left_BumperAbridged = "LB";
    public const string s_Right_BumperAbridged = "RB";
    public const string s_Left_Click = "Left-click";
    public const string s_Right_Click = "Right-click";

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
    public static string OffWhite_Open() {
        return "<color=#99B2FF>";
    }
    public static string Red_Open() {
        return "<color=#ff8080>";
    }
    public static string ZincBlue_Open() {
        return "<color=#c1dbff>";
    }
    public static string Gold_Open() {
        return "<color=#fff080>";
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
        return ZincBlue_Open() + s + "</color>";
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
        return Gold_Open() + s + "</color>";
    }
    public static string Orange(string s) {
        return "<color=#ff9d60>" + s + "</color>";
    }
    public static string OffWhite(string s) {
        return "<color=#99B2FF>" + s + "</color>";
    }
    public static string Bronze(string s) {
        return "<color=#ff9f00>" + s + "</color>";
    }
    // Same as above, but for specific characters
    public static string Color_Kog(string s) {
        return LightBlue(s);
    }
    public static string Color_Prima(string s) {
        return (s);
    }
    public static string Color_Machines(string s) {
        return Gray(s);
    }
    public static string Color_Kog_Open() {
        return LightBlue_Open();
    }
    public static string Color_Prima_Open() {
        return ("");
    }
    public static string Color_Machines_Open() {
        return Gray_Open();
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
    public static string Color_Pewter_Open() {
        return OffWhite_Open();
    }
    public static string Color_Zinc_Open() {
        return ZincBlue_Open();
    }
    public static string Color_Coin_Open() {
        return Gold_Open();
    }

    // Known words that should always appear in a specific color
    public static string Iron {
        get {
            return MidBlue("iron");
        }
    }
    public static string Steel {
        get {
            return Red("steel");
        }
    }
    public static string Pewter {
        get {
            return OffWhite("pewter");
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
    public static string PushesAndPulls {
        get {
            return Pushes + " and " + Pulls;
        }
    }
    public static string Mark {
        get {
            return Gray("Mark");
        }
    }
    public static string Mark_pulling {
        get {
            return LightBlue("Mark");
        }
    }
    public static string Mark_pushing {
        get {
            return Red("Mark");
        }
    }
    public static string Marking_pulling {
        get {
            return LightBlue("Marking");
        }
    }
    public static string Marked_pulling {
        get {
            return LightBlue("Marked");
        }
    }
    public static string MarkedMetal {
        get {
            return LightBlue("Marked metal");
        }
    }
    public static string BubbleMode {
        get {
            return Red("Bubble Mode");
        }
    }
    public static string AreaMode {
        get {
            return MidBlue("Area Mode");
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
    public static string Zinc {
        get {
            return ZincBlue("Zinc");
        }
    }
    public static string ControlWheel {
        get {
            return ZincBlue("Control Wheel");
        }
    }
    public static string BurnPercentage {
        get {
            return Gray("Burn Percentage");
        }
    }
    public static string Sprint {
        get {
            return OffWhite("Sprint");
        }
    }
    public static string Sprinting {
        get {
            return OffWhite("sprinting");
        }
    }
    public static string PewterJump {
        get {
            return OffWhite("Jump");
        }
    }
    public static string PewterJumping {
        get {
            return OffWhite("Jumping");
        }
    }
    public static string Anchor {
        get {
            return OffWhite("Anchor");
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
    // "<Input>" -> that input, in the correct color
    // "<Input>Abridged" -> a shortened version of that input for Help and Control Wheel
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
    public static string MouseButton3 {
        get {
            return Gray(s_Mouse_Button_3);
        }
    }
    public static string ScrollWheel {
        get {
            return Gray(s_Scroll_Wheel);
        }
    }
    public static string LeftJoystick {
        get {
            return Gray(s_Left_Joystick);
        }
    }
    public static string LeftJoystick_Zinc {
        get {
            return ZincBlue(s_Left_Joystick);
        }
    }
    public static string RightJoystick {
        get {
            return Gray(s_Right_Joystick);
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
    public static string Mouse {
        get {
            return Gray("Mouse");
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
            return ZincBlue("R");
        }
    }
    public static string F {
        get {
            return Gold("F");
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
    public static string F1 {
        get {
            return Gray("F1");
        }
    }
    public static string F5 {
        get {
            return Gray("F5");
        }
    }

    // "Key<Action>" -> the key that performs that action, different for Mouse/Keyboard and Gamepad, like "Space" for jump
    // "Key<Action>Abridged" -> Abridged key, like "LMB" for left mouse button
    // "HowTo<Action>" -> the operation that does that action, like "Press Space" for jump
    public static string KeyMove {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "the " + LeftJoystick;
            else
                return WASD;
        }
    }
    public static string HowToMove => s_Use_ + KeyMove;
    public static string KeyAnchor {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return RightJoystick;
            else
                return Ctrl;
        }
    }
    public static string HowToAnchor {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Click_in_ + "the " + RightJoystick;
            else
                return s_Hold_ + Ctrl;
        }
    }
    public static string KeySprint {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return B;
            else
                return Shift;
        }
    }
    public static string HowToSprint => s_Hold_ + KeySprint;
    public static string KeyLook {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "the " + RightJoystick;
            else
                return "the " + Mouse;
        }
    }
    public static string HowToLook => s_Use_ + KeyLook;
    public static string KeyJump {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return A;
            else
                return Space;
        }
    }
    public static string HowToJump => s_Press_ + KeyJump;
    public static string KeyPull {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return RightTrigger;
            else
                return LeftClick;
        }
    }
    public static string KeyPullAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return RightTriggerAbridged;
            else
                return LeftClickAbridged;
        }
    }
    public static string HowToPull => s_Press_ + KeyPull;
    public static string KeyPush {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return LeftTrigger;
            else
                return RightClick;
        }
    }
    public static string KeyPushAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return LeftTriggerAbridged;
            else
                return RightClickAbridged;
        }
    }
    public static string HowToPush => s_Press_ + KeyPush;
    public static string KeyPushPullAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return RightTriggerAbridged + '/' + LeftTriggerAbridged;
            else
                return LeftClickAbridged + '/' + RightClickAbridged;
        }
    }
    public static string KeyPullPushAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return LeftTriggerAbridged + '/' + RightTriggerAbridged;
            else
                return RightClickAbridged + '/' + LeftClickAbridged;
        }
    }
    public static string KeyZincTime {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Click_in_ + LeftJoystick_Zinc;
            else
                return Tab;
        }
    }
    public static string HowToZincTime {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Click_in_ + "the " + LeftJoystick_Zinc;
            else
                return s_Hold_ + Tab;
        }
    }
    public static string KeyControlWheel {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Click_in_ + LeftJoystick_Zinc;
            else
                return R;
        }
    }
    public static string HowToControlWheel {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Click_in_ + "the " + LeftJoystick_Zinc;
            else
                return s_Hold_ + R;
        }
    }
    public static string KeyMark_Pull {
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
    public static string HowToMark_Pull => s_Press_ + KeyMark_Pull;
    public static string KeyMark_PullAbridged {
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
    public static string KeyMark_PullPushAbridged => KeyMark_PullAbridged + "/" + KeyMark_PushAbridged;
    public static string KeyMark_Push {
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
    public static string KeyMark_PushAbridged {
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
    public static string KeyMarkAbridged {
        get {
            switch (SettingsMenu.settingsData.controlScheme) {
                case SettingsData.MK54: {
                        return LightBlue(s_Mouse_Button_5Abridged) + '/' + LightRed(s_Mouse_Button_4Abridged);
                    }
                case SettingsData.MK45: {
                        return LightBlue(s_Mouse_Button_4Abridged) + '/' + LightRed(s_Mouse_Button_5Abridged);
                    }
                case SettingsData.MKEQ: {
                        return LightBlue(s_E) + '/' + LightRed(s_Q);
                    }
                case SettingsData.MKQE: {
                        return LightBlue(s_Q) + '/' + LightRed(s_E);
                    }
                default: {
                        return LightBlue(s_Right_Bumper) + '/' + LightRed(s_Left_BumperAbridged);
                    }
            }
        }
    }
    public static string HowToStartBurningIron {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + LightBlue(s_Right_Bumper);
            else
                return LeftClick;
        }
    }

    public static string HowToStopBurning {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Hold_ + Y + ", " + LightBlue(s_Right_Bumper) + ", and " + LightRed(s_Left_Bumper);
            else
                return s_Press_ + Gray("X");
        }
    }

    public static string HowToMultiMark => HowToAnchor;
    public static string KeyMultiMark => KeyAnchor;
    public static string KeyPushPullStrength {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return LeftTrigger + "/" + RightTrigger + " pressure";
            else
                return ScrollWheel;
        }
    }
    public static string HowToPushPullStrength {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return "Change the pressure on " + LeftTrigger + " and " + RightTrigger;
            else
                return s_Scroll_ + "the " + ScrollWheel;
        }
    }
    public static string KeyRadiusAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return Gray("Up/Down") + " on the D-Pad";
            else
                return R + " + " + ScrollWheel;
        }
    }
    public static string HowToRadius {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ +  Gray("Up/Down") + " on the D-Pad";
            else
                return s_Hold_ + R + " + " + s_Scroll_ + "the " + ScrollWheel;
        }
    }
    public static string KeyThrow {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return X;
            else
                return F;
        }
    }
    public static string HowToThrow => s_Press_ + KeyThrow;
    public static string HowToHelp {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return s_Press_ + Start + " > Settings > Interface > Help Overlay";
            else
                return s_Press_ + H + " or " + s_Press_ + F1;
        }
    }
    public static string HowToHelpAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return Start + " > Settings > Interface";
            else
                return H + " or " + F1;
        }
    }
    public static string HowToPerspectiveAbridged {
        get {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                return Start + " > Settings > Gameplay";
            else
                return F5 + " or " + Escape + " > Settings > Gameplay";
        }
    }
}
