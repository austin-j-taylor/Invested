using UnityEngine;

/* 
 * Contains all keybinds. Allows for easy switching between keyboard and gamepad playing using the GamepadController.
 */

public static class Keybinds {

    public static float Horizontal() {
        if (GamepadController.UsingGamepad)
            return Input.GetAxis("GamepadHorizontal");
        else
            return Input.GetAxisRaw("Horizontal");
    }

    public static float Vertical() {
        if (GamepadController.UsingGamepad)
            return Input.GetAxis("GamepadVertical");
        else
            return Input.GetAxisRaw("Vertical");
    }

    public static bool Jump() {
        if (GamepadController.UsingGamepad)
            return Input.GetButton("GamepadX");
        else
            return Input.GetKey(KeyCode.Space);
    }
    public static bool JumpDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("GamepadX");
        else
            return Input.GetKeyDown(KeyCode.Space);
    }

    public static bool IronPulling() {
        return RightBurnRate() > .1f;
    }

    public static bool SteelPushing() {
        return LeftBurnRate() > .1f;
    }

    public static bool NegateDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("GamepadY");
        else
            return Input.GetKeyDown(KeyCode.LeftShift);
    }
    public static bool Negate() {
        if (GamepadController.UsingGamepad)
            return Input.GetButton("GamepadY");
        else
            return Input.GetKey(KeyCode.LeftShift);
    }
    public static bool StopBurnUp() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonUp("GamepadY");
        else
            return Input.GetKeyUp(KeyCode.LeftShift);
    }

    public static bool SelectDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("RightBumper");
        else
            return Input.GetButtonDown("Mouse4");
    }
    public static bool Select() {
        if (GamepadController.UsingGamepad)
            return Input.GetButton("RightBumper");
        else
            return Input.GetButton("Mouse4");
    }

    public static bool SelectAlternate() {
        if (GamepadController.UsingGamepad)
            return Input.GetButton("LeftBumper");
        else
            return Input.GetButton("Mouse3");
    }
    public static bool SelectAlternateDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("LeftBumper");
        else
            return Input.GetButtonDown("Mouse3");
    }

    public static float LeftBurnRate() {
        if (GamepadController.UsingGamepad)
            return Input.GetAxis("LeftTrigger");
        else
            return Input.GetButton("Mouse1") ? 1 : 0;
    }

    public static float RightBurnRate() {
        if (GamepadController.UsingGamepad)
            return Input.GetAxis("RightTrigger");
        else
            return Input.GetButton("Mouse0") ? 1 : 0;
    }

    public static bool WithdrawCoinDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("GamepadA");
        else
            return Input.GetKeyDown(KeyCode.LeftControl);
    }

    public static bool ToggleControllerSchemeDown() {
        return Input.GetKeyDown(KeyCode.P);
    }

    public static bool ToggleControllerRumbleDown() {
        return Input.GetKeyDown(KeyCode.O);
    }

    public static float ScrollWheelAxis() {
        if(GamepadController.UsingGamepad) {
            return 0;
        } else {
            return Input.GetAxis("Mouse ScrollWheel");
        }
    }

    public static bool ScrollWheelButton() {
        if (GamepadController.UsingGamepad) {
            return false;
        } else {
            return Input.GetButton("Mouse2");
        }
    }

}
