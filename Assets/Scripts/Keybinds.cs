using UnityEngine;

/* 
 * Contains all keybinds. Allows for easy switching between keyboard and gamepad playing using the GamepadController.
 */

public static class Keybinds {
    
    private static float timeToHold = 0f;

    public static float Horizontal() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetAxis("GamepadHorizontal");
        else
            return Input.GetAxisRaw("Horizontal");
    }

    public static float Vertical() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetAxis("GamepadVertical");
        else
            return Input.GetAxisRaw("Vertical");
    }

    public static bool Jump() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetButton("GamepadA");
        else
            return Input.GetKey(KeyCode.Space);
    }
    public static bool JumpDown() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetButtonDown("GamepadA");
        else
            return Input.GetKeyDown(KeyCode.Space);
    }

    public static bool WithdrawCoinDown() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetButtonDown("GamepadX");
        else
            return Input.GetKeyDown(KeyCode.LeftControl);
    }

    public static bool IronPulling() {
        return RightBurnRate() > .1f;
    }

    public static bool SteelPushing() {
        return LeftBurnRate() > .1f;
    }

    public static bool NegateDown() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetButtonDown("GamepadY");
        else
            return Input.GetKeyDown(KeyCode.LeftShift);
    }
    public static bool Negate() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetButton("GamepadY");
        else
            return Input.GetKey(KeyCode.LeftShift);
    }
    public static bool StopBurnUp() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetButtonUp("GamepadY");
        else
            return Input.GetKeyUp(KeyCode.LeftShift);
    }

    public static bool SelectDown() {
        switch(SettingsMenu.settingsData.controlScheme) {
            case 0: {
                    return Input.GetButtonDown("Mouse4");
                }
            case 1: {
                    return Input.GetKeyDown(KeyCode.E);
                }
            default: {
                    return Input.GetButtonDown("RightBumper");
                }
        }
    }
    public static bool Select() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case 0: {
                    return Input.GetButton("Mouse4");
                }
            case 1: {
                    return Input.GetKey(KeyCode.E);
                }
            default: {
                    return Input.GetButton("RightBumper");
                }
        }
    }

    public static bool SelectAlternate() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case 0: {
                    return Input.GetButton("Mouse3");
                }
            case 1: {
                    return Input.GetKey(KeyCode.Q);
                }
            default: {
                    return Input.GetButton("LeftBumper");
                }
        }
    }
    public static bool SelectAlternateDown() {
        switch (SettingsMenu.settingsData.controlScheme) {
            case 0: {
                    return Input.GetButtonDown("Mouse3");
                }
            case 1: {
                    return Input.GetKeyDown(KeyCode.Q);
                }
            default: {
                    return Input.GetButtonDown("LeftBumper");
                }
        }
    }

    public static float LeftBurnRate() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetAxis("LeftTrigger");
        else
            return Input.GetButton("Mouse1") ? 1 : 0;
    }

    public static float RightBurnRate() {
        if (SettingsMenu.settingsData.controlScheme == 2)
            return Input.GetAxis("RightTrigger");
        else
            return Input.GetButton("Mouse0") ? 1 : 0;
    }

    public static float ScrollWheelAxis() {
        if(SettingsMenu.settingsData.controlScheme == 2) {
            if (timeToHold < Time.time) {
                timeToHold = Time.time + .1f;
                return Input.GetAxis("GamepadDPadY");
            } else {
                return 0f;
            }
        } else {
            return Input.GetAxis("Mouse ScrollWheel");
        }
    }

    public static bool ScrollWheelButton() {
        if (SettingsMenu.settingsData.controlScheme == 2) {
            return false;
        } else {
            return Input.GetButton("Mouse2");
        }
    }

    public static bool EscapeDown() {
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("GamepadStart");
    }

}
