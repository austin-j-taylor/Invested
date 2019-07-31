using UnityEngine;

/* 
 * Contains all keybinds. Allows for easy switching between keyboard and gamepad playing using the GamepadController.
 */

public class Keybinds : MonoBehaviour {

    private const float triggerDeadband = 0.01f;

    private static float timeToHoldDPadY = 0f;
    private static float timeToHoldDPadX = 0f;

    // Only used for convert Gamepad axes to binary buttons
    private static bool lastWasPulling = false;
    private static bool lastWasPushing = false;

    private void LateUpdate() {
        lastWasPulling = IronPulling();
        lastWasPushing = SteelPushing();
    }

    public static bool PullDown() {
        if(SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            if (Input.GetAxis("RightTrigger") > triggerDeadband) {
                return !lastWasPulling;
            }
        } else {
            return Input.GetButtonDown("Mouse0");
        }
        return false;
    }
    public static bool PushDown() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            if (Input.GetAxis("LeftTrigger") > triggerDeadband) {
                return !lastWasPushing;
            }
        } else {
            return Input.GetButtonDown("Mouse1");
        }
        return false;
    }

    public static bool IronPulling() {
        return RightBurnPercentage() > triggerDeadband;
    }
    public static bool SteelPushing() {
        return LeftBurnPercentage() > triggerDeadband;
    }

    // Both Keyboard and Gamepad (Specifics for M45/KQE)
    public static bool SelectDown() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case SettingsData.MK54: {
                    return Input.GetButtonDown("Mouse4");
                }
            case SettingsData.MK45: {
                    return Input.GetButtonDown("Mouse3");
                }
            case SettingsData.MKEQ: {
                    return Input.GetKeyDown(KeyCode.E);
                }
            case SettingsData.MKQE: {
                    return Input.GetKeyDown(KeyCode.Q);
                }
            default: {
                    return Input.GetButtonDown("RightBumper");
                }
        }
    }
    public static bool Select() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case SettingsData.MK54: {
                    return Input.GetButton("Mouse4");
                }
            case SettingsData.MK45: {
                    return Input.GetButton("Mouse3");
                }
            case SettingsData.MKEQ: {
                    return Input.GetKey(KeyCode.E);
                }
            case SettingsData.MKQE: {
                    return Input.GetKey(KeyCode.Q);
                }
            default: {
                    return Input.GetButton("RightBumper");
                }
        }
    }

    public static bool SelectAlternate() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case SettingsData.MK54: {
                    return Input.GetButton("Mouse3");
                }
            case SettingsData.MK45: {
                    return Input.GetButton("Mouse4");
                }
            case SettingsData.MKEQ: {
                    return Input.GetKey(KeyCode.Q);
                }
            case SettingsData.MKQE: {
                    return Input.GetKey(KeyCode.E);
                }
            default: {
                    return Input.GetButton("LeftBumper");
                }
        }
    }
    public static bool SelectAlternateDown() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case SettingsData.MK54: {
                    return Input.GetButtonDown("Mouse3");
                }
            case SettingsData.MK45: {
                    return Input.GetButtonDown("Mouse4");
                }
            case SettingsData.MKEQ: {
                    return Input.GetKeyDown(KeyCode.Q);
                }
            case SettingsData.MKQE: {
                    return Input.GetKeyDown(KeyCode.E);
                }
            default: {
                    return Input.GetButtonDown("LeftBumper");
                }
        }
    }

    // Both Keyboard and Gamepad
    public static bool EscapeDown() {
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("GamepadStart");
    }
    public static bool ExitMenu() {
        return Input.GetButtonDown("Cancel") || Input.GetButtonDown("GamepadStart");
    }

    public static float Horizontal() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetAxis("Horizontal");
        else
            return Input.GetAxisRaw("Horizontal");
    }

    public static float Vertical() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetAxis("Vertical");
        else
            return Input.GetAxisRaw("Vertical");
    }

    public static bool Jump() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButton("GamepadA");
        else
            return Input.GetKey(KeyCode.Space);
    }
    public static bool JumpDown() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButtonDown("GamepadA");
        else
            return Input.GetKeyDown(KeyCode.Space);
    }

    public static bool Sprint() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButton("GamepadB");
        else
            return Input.GetKey(KeyCode.LeftShift);
    }
    public static bool SprintDown() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButtonDown("GamepadB");
        else
            return Input.GetKeyDown(KeyCode.LeftShift);
    }
    
    public static bool Walk() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            return Input.GetButton("GamepadRightJoystickClick");
        } else {
            return Input.GetKey(KeyCode.LeftControl);
        }
    }

    public static bool WithdrawCoinDown() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButtonDown("GamepadX");
        else
            return Input.GetButtonDown("Mouse2");
    }

    public static bool NegateDown() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButtonDown("GamepadY");
        else
            return Input.GetKeyDown(KeyCode.R);
    }
    public static bool Negate() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetButton("GamepadY");
        else
            return Input.GetKey(KeyCode.R);
    }

    public static bool StopBurning() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return false;
        else
            return Input.GetKey(KeyCode.X);
    }

    public static float LeftBurnPercentage() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetAxis("LeftTrigger");
        else
            return Input.GetButton("Mouse1") ? 1 : 0;
    }

    public static float RightBurnPercentage() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
            return Input.GetAxis("RightTrigger");
        else
            return Input.GetButton("Mouse0") ? 1 : 0;
    }

    public static bool ToggleCoinshotMode() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            return Input.GetButtonDown("GamepadBack");
        } else {
            return Input.GetKeyDown(KeyCode.C);
        }
    }

    public static bool ToggleHelpOverlay() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            return false; //return Input.GetButtonDown("GamepadBack"); ;
        } else {
            return Input.GetKeyDown(KeyCode.H);
        }
    }


    public static bool ZincTimeDown() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            return Input.GetButtonDown("GamepadLeftJoystickClick");
        } else {
            return Input.GetKeyDown(KeyCode.Tab);
        }
    }
    public static bool ZincTime() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            return true; // return Input.GetButton("GamepadLeftJoystickClick"); // managed in FeruchemicalZinc
        } else {
            return Input.GetKey(KeyCode.Tab);
        }
    }
    public static bool ControlWheel() {
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            return true;
        } else {
            return Input.GetKey(KeyCode.T);
        }
    }


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

}
