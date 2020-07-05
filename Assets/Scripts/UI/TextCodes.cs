using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Contains several static properties that referenced by in-game text.
/// For example, if a text field says "You can Pull metals with LMB", the words "Pull" and "LMB" would be in blue.
/// Also handles control scheme-dependent messages, such as saying "Press Space to Jump" with Keyboard/Mouse and "Press A to Jump" with Gamepad.
/// </summary>
public class TextCodes : MonoBehaviour {

    #region constants
    // String constants (prefixed with s_) for inputs
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
    // String constants (prefixed with s_) for commands
    public const string s_Press_ = "Press ";
    public const string s_Pressing_ = "Pressing ";
    public const string s_Hold_ = "Hold ";
    public const string s_Click_in_ = "Click in ";
    public const string s_Use_ = "Use ";
    public const string s_Tap_ = "Tap ";
    public const string s_Scroll_ = "Scroll ";
    #endregion

    #region colors
    // Opening tags for certain colors
    public static string Blue_Open() => "<color=#0080ff>";
    public static string MidBlue_Open() => "<color=#00bfff>";
    public static string LightBlue_Open() => "<color=#7fdfff>";
    public static string Gray_Open() => "<color=#bfbfbf>";
    public static string PewterWhite_Open() => "<color=#99B2FF>";
    public static string Red_Open() => "<color=#ff8080>";
    public static string ZincBlue_Open() => "<color=#c1dbff>";
    public static string Gold_Open() => "<color=#fff080>";
    // Methods that accept strings as arguments and return strings colored in their respective color
    public static string Blue(string s) => Blue_Open() + s + "</color>";
    public static string MidBlue(string s) => MidBlue_Open() + s + "</color>";
    public static string LightBlue(string s) => LightBlue_Open() + s + "</color>";
    public static string ZincBlue(string s) => ZincBlue_Open() + s + "</color>";
    public static string LightRed(string s) => "<color=#ffbfbf>" + s + "</color>";
    public static string Red(string s) => Red_Open() + s + "</color>";
    public static string Gray(string s) => Gray_Open() + s + "</color>";
    public static string Gold(string s) => Gold_Open() + s + "</color>";
    public static string Orange(string s) => "<color=#ff9d60>" + s + "</color>";
    public static string PewterWhite(string s) => "<color=#99B2FF>" + s + "</color>";
    public static string Bronze(string s) => "<color=#ff9f00>" + s + "</color>";
    // Same as above, but for specific characters
    public static string Color_Kog(string s) => LightBlue(s);
    public static string Color_Prima(string s) => (s);
    public static string Color_Machines(string s) => Gray(s);
    public static string Color_Kog_Open() => LightBlue_Open();
    public static string Color_Prima_Open() => ("");
    public static string Color_Machines_Open() => Gray_Open();
    // Open tags for specific actions or ideas
    public static string Color_Location_Open() => Blue_Open();
    public static string Color_Pull_Open() => MidBlue_Open();
    public static string Color_Push_Open() => Red_Open();
    public static string Color_Pewter_Open() => PewterWhite_Open();
    public static string Color_Zinc_Open() => ZincBlue_Open();
    public static string Color_Coin_Open() => Gold_Open();

    // Known words that should always appear in a specific color
    public static string Iron => MidBlue("iron");
    public static string Iron_proper => MidBlue("Iron");
    public static string Steel => Red("steel");
    public static string Steel_proper => Red("Steel");
    public static string Pewter => PewterWhite("pewter");
    public static string Pewter_proper => PewterWhite("Pewter");
    public static string Pull => MidBlue("Pull");
    public static string Pulls => MidBlue("Pulls");
    public static string Pulling => MidBlue("Pulling");
    public static string Push => Red("Push");
    public static string Pushes => Red("Pushes");
    public static string Pushing => Red("Pushing");
    public static string Push_Pull => Push + '/' + Pull;
    public static string Pull_Push => Pull + '/' + Push;
    public static string PushesAndPulls => Pushes + " and " + Pulls;
    public static string Mark => Gray("Mark");
    public static string Mark_pulling => LightBlue("Mark");
    public static string Mark_pushing => Red("Mark");
    public static string Marking_pulling => LightBlue("Marking");
    public static string Marked_pulling => LightBlue("Marked");
    public static string MarkedMetal => LightBlue("Marked metal");
    public static string BubbleMode => Red("Bubble Mode");
    public static string AreaMode => MidBlue("Area Mode");
    public static string CoinshotMode => Gold("Coinshot mode");
    public static string ZincTime => ZincBlue("Zinc Time");
    public static string Zinc => ZincBlue("Zinc");
    public static string ControlWheel => ZincBlue("Control Wheel");
    public static string BurnPercentage => Gray("Burn Percentage");
    public static string Sprint => PewterWhite("Sprint");
    public static string Sprinting => PewterWhite("sprinting");
    public static string PewterJump => PewterWhite("Jump");
    public static string PewterJumping => PewterWhite("Jumping");
    public static string Anchor => PewterWhite("Anchor");
    public static string HelpOverlay => Gray("Help Overlay");
    // Objects, prefixed with "O_"
    public static string O_SeekerCube => Bronze("Seeker Cube");
    public static string O_Coin => Gold("Coin");
    public static string O_Coins => Gold("Coins");

    // Known inputs that should always appear in a specific color
    // "<Input>" -> that input, in the correct color
    // "<Input>Abridged" -> a shortened version of that input for Help and Control Wheel
    public static string LeftClick => MidBlue(s_Left_Click);
    public static string LeftClickAbridged => MidBlue("LMB");
    public static string RightClick => Red(s_Right_Click);
    public static string RightClickAbridged => Red("RMB");
    public static string RightTrigger => MidBlue(s_Right_Trigger);
    public static string RightTriggerAbridged => MidBlue("RT");
    public static string LeftTrigger => Red(s_Left_Trigger);
    public static string LeftTriggerAbridged => Red("LT");
    public static string MouseButton3 => Gray(s_Mouse_Button_3);
    public static string ScrollWheel => Gray(s_Scroll_Wheel);
    public static string LeftJoystick => Gray(s_Left_Joystick);
    public static string LeftJoystick_Zinc => ZincBlue(s_Left_Joystick);
    public static string RightJoystick => Gray(s_Right_Joystick);
    public static string Back => Gray("Back");
    public static string Start => Gray("Start");
    public static string A => Gray("A");
    public static string B => PewterWhite("B");
    public static string X => Gold("X");
    public static string Y => ZincBlue("Y");
    // Mouse/Keyboard
    public static string Mouse => Gray("Mouse");
    public static string Shift => Gray("Shift");
    public static string Space => Gray("Space");
    public static string Ctrl => Gray("Ctrl");
    public static string LeftAlt => Gray("Left Alt");
    public static string Tab => ZincBlue("Tab");
    public static string R => ZincBlue("R");
    public static string F => Gold("F");
    public static string WASD => Gray("W/A/S/D");
    public static string Escape => Gray("Escape");
    public static string H => Gray("H");
    public static string C => Gold("C");
    public static string F1 => Gray("F1");
    public static string F5 => Gray("F5");
    #endregion

    #region keys
    // "Key<Action>" -> the key that performs that action, different for Mouse/Keyboard and Gamepad, like "Space" for jump
    // "Key<Action>Abridged" -> Abridged key, like "LMB" for left mouse button
    // "HowTo<Action>" -> the operation that does that action, like "Press Space" for jump
    public static string KeyMove {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return "the " + LeftJoystick;
            else
                return WASD;
        }
    }
    public static string KeyMoveAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftJoystick;
            else
                return WASD;
        }
    }
    public static string HowToMove => s_Use_ + KeyMove;
    public static string KeyAnchor {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Click_in_ + RightJoystick;
            else
                return Ctrl;
        }
    }
    public static string HowToAnchor {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Click_in_ + "the " + RightJoystick;
            else
                return s_Hold_ + Ctrl;
        }
    }
    public static string KeySprint {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return B;
            else
                return Shift;
        }
    }
    public static string HowToSprint => s_Hold_ + KeySprint;
    public static string KeyLook {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return "the " + RightJoystick;
            else
                return "the " + Mouse;
        }
    }
    public static string KeyLookAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return RightJoystick;
            else
                return Mouse;
        }
    }
    public static string HowToLook => s_Use_ + KeyLook;
    public static string KeyJump {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return A;
            else
                return Space;
        }
    }
    public static string HowToJump => s_Press_ + KeyJump;
    public static string KeyPull {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return RightTrigger;
            else
                return LeftClick;
        }
    }
    public static string KeyPullAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return RightTriggerAbridged;
            else
                return LeftClickAbridged;
        }
    }
    public static string HowToPull => s_Press_ + KeyPull;
    public static string KeyPush {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftTrigger;
            else
                return RightClick;
        }
    }
    public static string KeyPushAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftTriggerAbridged;
            else
                return RightClickAbridged;
        }
    }
    public static string HowToPush => s_Press_ + KeyPush;
    public static string KeyPullPushAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftTriggerAbridged + '/' + RightTriggerAbridged;
            else
                return RightClickAbridged + '/' + LeftClickAbridged;
        }
    }
    public static string KeyZincTime {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Click_in_ + LeftJoystick_Zinc;
            else
                return Tab;
        }
    }
    public static string HowToZincTime {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Click_in_ + "the " + LeftJoystick_Zinc;
            else
                return s_Hold_ + Tab;
        }
    }
    public static string KeyControlWheel {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return Y;
                //return s_Click_in_ + LeftJoystick_Zinc;
            else
                return R;
        }
    }
    public static string HowToControlWheel {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Hold_ + Y;
            else
                return s_Hold_ + R;
        }
    }
    public static string KeyMark_Pull {
        get {
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return LightBlue(s_Mouse_Button_5);
                    }
                case JSONSettings_Gameplay.MK45: {
                        return LightBlue(s_Mouse_Button_4);
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return LightBlue(s_E);
                    }
                case JSONSettings_Gameplay.MKQE: {
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
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return LightBlue(s_Mouse_Button_5Abridged);
                    }
                case JSONSettings_Gameplay.MK45: {
                        return LightBlue(s_Mouse_Button_4Abridged);
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return LightBlue(s_E);
                    }
                case JSONSettings_Gameplay.MKQE: {
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
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return LightRed(s_Mouse_Button_4);
                    }
                case JSONSettings_Gameplay.MK45: {
                        return LightRed(s_Mouse_Button_5);
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return LightRed(s_Q);
                    }
                case JSONSettings_Gameplay.MKQE: {
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
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return LightRed(s_Mouse_Button_4Abridged);
                    }
                case JSONSettings_Gameplay.MK45: {
                        return LightRed(s_Mouse_Button_5Abridged);
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return LightRed(s_Q);
                    }
                case JSONSettings_Gameplay.MKQE: {
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
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return LightBlue(s_Mouse_Button_5Abridged) + '/' + LightRed(s_Mouse_Button_4Abridged);
                    }
                case JSONSettings_Gameplay.MK45: {
                        return LightBlue(s_Mouse_Button_4Abridged) + '/' + LightRed(s_Mouse_Button_5Abridged);
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return LightBlue(s_E) + '/' + LightRed(s_Q);
                    }
                case JSONSettings_Gameplay.MKQE: {
                        return LightBlue(s_Q) + '/' + LightRed(s_E);
                    }
                default: {
                        return LightBlue(s_Right_BumperAbridged) + '/' + LightRed(s_Left_BumperAbridged);
                    }
            }
        }
    }
    public static string HowToStartBurningIron {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Press_ + RightTrigger;
            else
                return LeftClick;
        }
    }

    public static string HowToStopBurning {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Hold_ + Y + ", " + LightBlue(s_Right_Bumper) + ", and " + LightRed(s_Left_Bumper);
            else
                return s_Press_ + Gray("X");
        }
    }

    public static string HowToMultiMark => HowToAnchor;
    public static string KeyMultiMark => KeyAnchor;
    public static string KeyPushPullStrength {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftTrigger + "/" + RightTrigger + " pressure";
            else
                return ScrollWheel;
        }
    }
    public static string KeyPullStrength {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return RightTrigger + " pressure";
            else
                return ScrollWheel;
        }
    }
    //public static string HowToPullStrength {
    //    get {
    //        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
    //            return "Change the pressure on " + RightTrigger;
    //        else
    //            return s_Scroll_ + "the " + ScrollWheel;
    //    }
    //}
    public static string KeyRadiusAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return Gray("Up/Down") + " on the D-Pad";
            else
                return R + " + " + ScrollWheel;
        }
    }
    public static string HowToRadius {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Press_ + Gray("Up/Down") + " on the D-Pad";
            else
                return s_Hold_ + R + " + " + s_Scroll_ + "the " + ScrollWheel;
        }
    }
    public static string KeyThrow {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return X;
            else
                return F;
        }
    }
    public static string HowToThrow => s_Press_ + KeyThrow;
    public static string HowToHelp {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Press_ + Start + " > Settings > Interface > Help Overlay";
            else
                return s_Press_ + H + " or " + s_Press_ + F1;
        }
    }
    public static string HowToHelpAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return Start + " > Settings > Interface";
            else
                return H + " or " + F1;
        }
    }
    public static string HowToPerspectiveAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return Back;
            else
                return F5 + " or " + Escape + " > Settings > Gameplay";
        }
    }
    #endregion
}
