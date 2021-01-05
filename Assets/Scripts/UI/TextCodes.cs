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
    // String constants (prefixed with s_) for commands
    public const string s_Press_ = "Press ";
    public const string s_Pressing_ = "Pressing ";
    public const string s_Hold_ = "Hold ";
    public const string s_Click_in_ = "Click in ";
    public const string s_Use_ = "Use ";
    public const string s_Tap_ = "Tap ";
    public const string s_Scroll_ = "Scroll ";
    public const string s_Double_tap = "Double-tap ";
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
    public static string RightTrigger => "<sprite name=gamepad_rt>";
    public static string RightTriggerAbridged => "<sprite name=gamepad_rt>";
    public static string LeftTrigger => "<sprite name=gamepad_lt>";
    public static string LeftTriggerAbridged => "<sprite name=gamepad_lt>";
    public static string RightBumper => "<sprite name=gamepad_rb>";
    public static string LeftBumper => "<sprite name=gamepad_lb>";
    public static string LeftJoystick => "<sprite name=gamepad_joystick_l>";
    public static string LeftJoystick_Zinc => "<sprite name=gamepad_joystick_l>";
    public static string LeftJoystickClick => "<sprite name=gamepad_click_l>";
    public static string RightJoystick => "<sprite name=gamepad_joystick_r>";
    public static string RightJoystickClick => "<sprite name=gamepad_click_r>";
    public static string DPadUpDown => "<sprite name=gamepad_dpad_ud>";
    public static string DPadLeftRight => "<sprite name=gamepad_dpad_lr>";
    public static string DPadRight => "<sprite name=gamepad_dpad_r>";
    public static string DPadLeft => "<sprite name=gamepad_dpad_l>";
    public static string Back => "<sprite name=gamepad_back>";
    public static string Start => "<sprite name=gamepad_start>";
    public static string A => "<sprite name=gamepad_a>";
    public static string B => "<sprite name=gamepad_b>";
    public static string X => "<sprite name=gamepad_x>";
    public static string Y => "<sprite name=gamepad_y>";
    // Mouse/Keyboard
    public static string Mouse => "<sprite name=mouse>";
    public static string LeftClick => "<sprite name=lmb>";
    public static string LeftClickAbridged => "<sprite name=lmb>";
    public static string RightClick => "<sprite name=rmb>";
    public static string RightClickAbridged => "<sprite name=rmb>";
    public static string MouseButton3 => "<sprite name=mb3>";
    public static string MiddleMouseButton => "<sprite name=mb3>";
    public static string ScrollWheel => "<sprite name=scroll>";
    public static string MouseButton4 => "<sprite name=mb4>";
    public static string MouseButton5 => "<sprite name=mb5>";
    public static string Shift => "<sprite name=shift>";
    public static string Space => "<sprite name=space>";
    public static string Ctrl => "<sprite name=ctrl>";
    public static string LeftAlt => "<sprite name=alt>";
    public static string Tab => "<sprite name=tab>";
    public static string E => "<sprite name=e>";
    public static string Q => "<sprite name=q>";
    public static string R => "<sprite name=r>";
    public static string F => "<sprite name=f>";
    public static string WASD => "<sprite name=w><sprite name=a><sprite name=s><sprite name=d>";
    public static string Escape => "<sprite name=esc>";
    public static string H => "<sprite name=h>";
    public static string C => "<sprite name=c>";
    public static string Z => "<sprite name=z>";
    public static string V => "<sprite name=v>";
    public static string L => "<sprite name=l>";
    public static string F1 => "<sprite name=f1>";
    public static string F5 => "<sprite name=f5>";
    public static string Numeric1 => "<sprite name=key1>";
    public static string Numeric2 => "<sprite name=key2>";
    public static string Numeric3 => "<sprite name=key3>";
    public static string Numeric4 => "<sprite name=key4>";
    public static string Numeric5 => "<sprite name=key5>";
    public static string Numeric6 => "<sprite name=key6>";
    public static string Numeric7 => "<sprite name=key7>";
    public static string Numeric8 => "<sprite name=key8>";
    public static string Numeric9 => "<sprite name=key9>";
    public static string Numeric0 => "<sprite name=key0>";
    #endregion

    #region keys
    // "Key<Action>" -> the key that performs that action, different for Mouse/Keyboard and Gamepad, like "Space" for jump
    // "Key<Action>Abridged" -> Abridged key, like "LMB" for left mouse button
    // "HowTo<Action>" -> the operation that does that action, like "Press Space" for jump
    public static string KeyMove {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftJoystick;
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
                //return s_Click_in_ + RightJoystick;
                return RightJoystickClick;
            else
                return Ctrl;
        }
    }
    public static string HowToAnchor {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return RightJoystickClick;
            //return s_Click_in_ + "the " + RightJoystick;
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
                return RightJoystick;
            else
                return Mouse;
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
                return LeftJoystickClick;
            //return s_Click_in_ + LeftJoystick_Zinc;
            else
                return Tab;
        }
    }
    public static string HowToZincTime {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return LeftJoystickClick;
            //return s_Click_in_ + "the " + LeftJoystick_Zinc;
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
    public static string KeyManual => SettingsMenu.settingsGameplay.UsingGamepad ? "" : Numeric1;
    public static string KeyArea => SettingsMenu.settingsGameplay.UsingGamepad ? "" : Numeric2;
    public static string KeyBubble => SettingsMenu.settingsGameplay.UsingGamepad ? "" : Numeric3;
    public static string KeyBubblePolarity => SettingsMenu.settingsGameplay.UsingGamepad ? "" : V;
    public static string KeyCoinshot => SettingsMenu.settingsGameplay.UsingGamepad ? "" : Numeric4;
    public static string KeyThrowingMode => SettingsMenu.settingsGameplay.UsingGamepad ? "" : C;
    public static string KeyDeselectAll => SettingsMenu.settingsGameplay.UsingGamepad ? "" : Z;
    public static string KeyStopBurning => SettingsMenu.settingsGameplay.UsingGamepad ? "" : X;
    public static string KeyMark_Pull {
        get {
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return MouseButton5;
                    }
                case JSONSettings_Gameplay.MK45: {
                        return MouseButton4;
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return E;
                    }
                case JSONSettings_Gameplay.MKQE: {
                        return Q;
                    }
                default: {
                        return RightBumper;
                    }
            }
        }
    }
    public static string KeyMark_Push {
        get {
            switch (SettingsMenu.settingsGameplay.controlScheme) {
                case JSONSettings_Gameplay.MK54: {
                        return MouseButton4;
                    }
                case JSONSettings_Gameplay.MK45: {
                        return MouseButton5;
                    }
                case JSONSettings_Gameplay.MKEQ: {
                        return Q;
                    }
                case JSONSettings_Gameplay.MKQE: {
                        return E;
                    }
                default: {
                        return LeftBumper;
                    }
            }
        }
    }
    public static string HowToMark_Pull => s_Press_ + KeyMark_Pull;
    public static string KeyMark => KeyMark_Pull + "/" + KeyMark_Push;
    public static string HowToStartBurningIron {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Press_ + RightTrigger;
            else
                return LeftClick;
        }
    }


    public static string HowToMultiMark_Pull => HowToAnchor + " + " + KeyMark_Pull;
    public static string HowToMultiMark => HowToAnchor + " + " + KeyMark;
    public static string KeyMultiMark_Pull => KeyAnchor + " + " + KeyMark_Pull;
    public static string KeyMultiMark => KeyAnchor + " + " + KeyMark;
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
                return DPadUpDown;
            else
                return R + " + " + ScrollWheel;
        }
    }
    public static string HowToRadius {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return s_Press_ + DPadUpDown;
            else
                return s_Hold_ + R + " + " + s_Scroll_ + ScrollWheel;
        }
    }
    public static string KeyThrow {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return X;
            else
                return F + " or " + MiddleMouseButton;
        }
    }
    public static string HowToThrow => s_Press_ + KeyThrow;
    public static string KeyToss {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return DPadRight;
            else
                return KeyAnchor + " + " + F + " or " + MiddleMouseButton;
        }
    }
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
    public static string HowToTextLog {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return Escape + " > Text Log";
            else
                return L;
        }
    }
    public static string HowToPerspectiveAbridged {
        get {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                return Back;
            else
                return F5;
        }
    }
    #endregion
}
