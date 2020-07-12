using UnityEngine;

/// <summary>
/// Contains all keybinds. Allows for easy switching between keyboard and gamepad playing.
/// </summary>
public class Keybinds : MonoBehaviour {

    #region constants
    private const float triggerDeadband = 0.01f;
    private const float dpadDeadband = 0.01f;
    private const float doubleTapThreshold = 0.25f;
    #endregion

    #region fields
    private static float timeToHoldDPadY = 0f;
    private static float timeToHoldDPadX = 0f;
    //private static float doubleTapTimeWheel = float.NegativeInfinity;

    // Only used for convert Gamepad axes to binary buttons
    private static bool lastWasPulling = false;
    private static bool lastWasPushing = false;
    private static bool lastWasDPadRight = false;
    #endregion

    // Mouse/Stick Axis names
    public static string MouseX => "Mouse X";
    public static string MouseY => "Mouse Y";
    public static string JoystickRightHorizontal => "HorizontalRight";
    public static string JoystickRightVertical => "VerticalRight";

    private void LateUpdate() {
        lastWasPulling = IronPulling();
        lastWasPushing = SteelPushing();
        lastWasDPadRight = Input.GetAxis("GamepadDPadX") > dpadDeadband;
    }

    public static bool PullDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            if (Input.GetAxis("RightTrigger") > triggerDeadband) {
                return !lastWasPulling;
            }
        } else {
            return Input.GetButtonDown("Mouse0");
        }
        return false;
    }
    public static bool PushDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            if (Input.GetAxis("LeftTrigger") > triggerDeadband) {
                return !lastWasPushing;
            }
        } else {
            return Input.GetButtonDown("Mouse1");
        }
        return false;
    }

    public static bool IronPulling() => RightBurnPercentage() > triggerDeadband;
    public static bool SteelPushing() => LeftBurnPercentage() > triggerDeadband;

    #region selecting
    // Both Keyboard and Gamepad (Specifics for M45/KQE)
    public static bool SelectDown() {
        switch (SettingsMenu.settingsGameplay.controlScheme) {
            case JSONSettings_Gameplay.MK54: {
                    return Input.GetKeyDown(KeyCode.Mouse4);
                }
            case JSONSettings_Gameplay.MK45: {
                    return Input.GetKeyDown(KeyCode.Mouse3);
                }
            case JSONSettings_Gameplay.MKEQ: {
                    return Input.GetKeyDown(KeyCode.E);
                }
            case JSONSettings_Gameplay.MKQE: {
                    return Input.GetKeyDown(KeyCode.Q);
                }
            default: {
                    return Input.GetButtonDown("RightBumper");
                }
        }
    }
    public static bool Select() {
        switch (SettingsMenu.settingsGameplay.controlScheme) {
            case JSONSettings_Gameplay.MK54: {
                    return Input.GetKey(KeyCode.Mouse4);
                }
            case JSONSettings_Gameplay.MK45: {
                    return Input.GetKey(KeyCode.Mouse3);
                }
            case JSONSettings_Gameplay.MKEQ: {
                    return Input.GetKey(KeyCode.E);
                }
            case JSONSettings_Gameplay.MKQE: {
                    return Input.GetKey(KeyCode.Q);
                }
            default: {
                    return Input.GetButton("RightBumper");
                }
        }
    }
    public static bool SelectUp() {
        switch (SettingsMenu.settingsGameplay.controlScheme) {
            case JSONSettings_Gameplay.MK54: {
                    return Input.GetKeyUp(KeyCode.Mouse4);
                }
            case JSONSettings_Gameplay.MK45: {
                    return Input.GetKeyUp(KeyCode.Mouse3);
                }
            case JSONSettings_Gameplay.MKEQ: {
                    return Input.GetKeyUp(KeyCode.E);
                }
            case JSONSettings_Gameplay.MKQE: {
                    return Input.GetKeyUp(KeyCode.Q);
                }
            default: {
                    return Input.GetButtonUp("RightBumper");
                }
        }
    }

    public static bool SelectAlternateDown() {
        switch (SettingsMenu.settingsGameplay.controlScheme) {
            case JSONSettings_Gameplay.MK54: {
                    return Input.GetKeyDown(KeyCode.Mouse3);
                }
            case JSONSettings_Gameplay.MK45: {
                    return Input.GetKeyDown(KeyCode.Mouse4);
                }
            case JSONSettings_Gameplay.MKEQ: {
                    return Input.GetKeyDown(KeyCode.Q);
                }
            case JSONSettings_Gameplay.MKQE: {
                    return Input.GetKeyDown(KeyCode.E);
                }
            default: {
                    return Input.GetButtonDown("LeftBumper");
                }
        }
    }
    public static bool SelectAlternate() {
        switch (SettingsMenu.settingsGameplay.controlScheme) {
            case JSONSettings_Gameplay.MK54: {
                    return Input.GetKey(KeyCode.Mouse3);
                }
            case JSONSettings_Gameplay.MK45: {
                    return Input.GetKey(KeyCode.Mouse4);
                }
            case JSONSettings_Gameplay.MKEQ: {
                    return Input.GetKey(KeyCode.Q);
                }
            case JSONSettings_Gameplay.MKQE: {
                    return Input.GetKey(KeyCode.E);
                }
            default: {
                    return Input.GetButton("LeftBumper");
                }
        }
    }
    public static bool SelectAlternateUp() {
        switch (SettingsMenu.settingsGameplay.controlScheme) {
            case JSONSettings_Gameplay.MK54: {
                    return Input.GetKeyUp(KeyCode.Mouse3);
                }
            case JSONSettings_Gameplay.MK45: {
                    return Input.GetKeyUp(KeyCode.Mouse4);
                }
            case JSONSettings_Gameplay.MKEQ: {
                    return Input.GetKeyUp(KeyCode.Q);
                }
            case JSONSettings_Gameplay.MKQE: {
                    return Input.GetKeyUp(KeyCode.E);
                }
            default: {
                    return Input.GetButtonUp("LeftBumper");
                }
        }
    }
    #endregion

    // Both Keyboard and Gamepad
    public static bool EscapeDown() {
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("GamepadStart");
    }
    public static bool ExitMenu() {
        return Input.GetButtonDown("Cancel") || Input.GetButtonDown("GamepadStart");
    }

    public static float Horizontal() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetAxis("Horizontal");
        else
            return Input.GetAxisRaw("Horizontal");
    }

    public static float Vertical() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetAxis("Vertical");
        else
            return Input.GetAxisRaw("Vertical");
    }

    public static bool Jump() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetButton("GamepadA");
        else
            return Input.GetKey(KeyCode.Space);
    }
    public static bool JumpDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetButtonDown("GamepadA");
        else
            return Input.GetKeyDown(KeyCode.Space);
    }
    public static bool AdvanceConversation() {
        return JumpDown();
    }
    public static bool AccelerateConversation() {
        return Jump();
    }

    public static bool Sprint() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetButton("GamepadB");
        else
            return Input.GetKey(KeyCode.LeftShift);
    }
    public static bool SprintDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetButtonDown("GamepadB");
        else
            return Input.GetKeyDown(KeyCode.LeftShift);
    }

    public static bool Walk() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButton("GamepadRightJoystickClick");
        } else {
            return Input.GetKey(KeyCode.LeftControl);
        }
    }

    public static bool WithdrawCoinDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetButtonDown("GamepadX");
        else
            return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Mouse2);
    }
    public static bool WithdrawCoin() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetButton("GamepadX");
        else
            return Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.Mouse2);
    }
    public static bool TossCoinDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return !lastWasDPadRight && Input.GetAxis("GamepadDPadX") > dpadDeadband;
        else
            return false;
    }
    public static bool TossCoinCondition() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetAxis("GamepadDPadX") > dpadDeadband;
        else
            return Walk();
    }

    public static bool MultipleMarks() => Walk();

    public static bool StopBurning() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return false;
        else
            return Input.GetKeyDown(KeyCode.X);
    }

    public static float LeftBurnPercentage() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetAxis("LeftTrigger");
        else
            return Input.GetKey(KeyCode.Mouse1) ? 1 : 0;
    }

    public static float RightBurnPercentage() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
            return Input.GetAxis("RightTrigger");
        else
            return Input.GetKey(KeyCode.Mouse0) ? 1 : 0;
    }

    public static bool ToggleHelpOverlay() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false; //return Input.GetButtonDown("GamepadBack"); ;
        } else {
            return Input.GetKeyDown(KeyCode.H) || Input.GetKeyDown(KeyCode.F1);
        }
    }

    public static bool ToggleTextLog() {
        if (SettingsMenu.settingsGameplay.UsingGamepad)
            //return Input.GetButtonDown("GamepadBack");
            return false;
        else
            return Input.GetKeyDown(KeyCode.L);
    }

    public static bool ZincTimeDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButtonDown("GamepadLeftJoystickClick");
        } else {
            return Input.GetKeyDown(KeyCode.Tab);
        }
    }
    public static bool ZincTime() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButton("GamepadLeftJoystickClick");
        } else {
            return Input.GetKey(KeyCode.Tab);
        }
    }

    #region controlWheel
    public static bool ControlWheel() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButton("GamepadY");
        } else {
            return Input.GetKey(KeyCode.R);
        }
    }
    public static bool ControlWheelDown() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButtonDown("GamepadY");
        } else {
            return Input.GetKeyDown(KeyCode.R);
        }
    }
    public static bool ControlWheelConfirm() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButtonDown("GamepadA");
        } else {
            return Input.GetKeyDown(KeyCode.Mouse0);
        }
    }
    public static bool ControlWheelManual() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false;
        } else {
            return Input.GetKeyDown(KeyCode.Alpha1);
        }
    }
    public static bool ControlWheelArea() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false;
        } else {
            return Input.GetKeyDown(KeyCode.Alpha2);
        }
    }
    public static bool ControlWheelBubble() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false;
        } else {
            return Input.GetKeyDown(KeyCode.Alpha3);
        }
    }
    public static bool ControlWheelCoinshot() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false;
        } else {
            return Input.GetKeyDown(KeyCode.Alpha4);
        }
    }
    public static bool ControlWheelThrowingMode() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false;
        } else {
            return Input.GetKeyDown(KeyCode.C);
        }
    }
    public static bool ControlWheelDeselectAll() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return false;
        } else {
            return Input.GetKeyDown(KeyCode.Z);
        }
    }
    #endregion

    // Only Gamepad
    public static float LeftTrigger() {
        return Input.GetAxis("LeftTrigger");
    }

    public static float RightTrigger() {
        return Input.GetAxis("RightTrigger");
    }

    public static float DPadYAxis() {
        if (timeToHoldDPadY < Time.time) {
            timeToHoldDPadY = Time.time + .1f * Time.timeScale;
            return Input.GetAxis("GamepadDPadY");
        } else {
            return 0;
        }
    }
    public static float DPadXAxis() {
        if (timeToHoldDPadX < Time.time) {
            timeToHoldDPadX = Time.time + .1f * Time.timeScale;
            return Input.GetAxis("GamepadDPadX");
        } else {
            return 0;
        }
    }

    // Only Mouse/Keyboard
    public static float ScrollWheelAxis() {
        return Input.GetAxis("Mouse ScrollWheel");
    }

    public static bool ScrollWheelButton() {
        return Input.GetButton("Mouse2");
    }
    public static bool TogglePerspective() {
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
            return Input.GetButtonDown("GamepadBack");
        } else {
            return Input.GetKeyDown(KeyCode.F5);
        }
    }
}
