using UnityEngine;

/* 
 * Contains all keybinds. Allows for easy switching between keyboard and gamepad playing using the GamepadController.
 */

public static class Keybinds {

    private static float timeToHoldDPadY = 0f;
    private static float timeToHoldDPadX = 0f;
    private static float triggerDeadband = 0.01f;

    public static bool IronPulling() {
        return RightBurnRate() > triggerDeadband;
    }

    public static bool SteelPushing() {
        return LeftBurnRate() > triggerDeadband;
    }

    // Both Keyboard and Gamepad (Specifics for M45/KQE)
    public static bool SelectDown() {
                    return Input.GetButtonDown("Mouse4");
    }
    public static bool Select() {
                    return Input.GetButton("Mouse4");
    }

    public static bool SelectAlternate() {
                    return Input.GetButton("Mouse3");
    }
    public static bool SelectAlternateDown() {
                    return Input.GetButtonDown("Mouse3");
    }

    // Both Keyboard and Gamepad
    public static bool EscapeDown() {
        return Input.GetButtonDown("Cancel");
    }

    public static float Horizontal() {
            return Input.GetAxisRaw("Horizontal");
    }

    public static float Vertical() {
            return Input.GetAxisRaw("Vertical");
    }

    public static bool Jump() {
            return Input.GetButton("Jump");
    }

    public static bool JumpDown() {
        return Input.GetButtonDown("Jump");
    }

    public static bool WithdrawCoinDown() {
            return Input.GetKeyDown(KeyCode.LeftControl);
    }

    public static bool NegateDown() {
            return Input.GetButtonDown("Left Shift");
    }
    public static bool Negate() {
            return Input.GetButton("Left Shift");
    }
    public static bool StopBurnUp() {
            return Input.GetButtonUp("Left Shift");
    }

    public static float LeftBurnRate() {
            return Input.GetButton("Mouse1") ? 1 : 0;
    }

    public static float RightBurnRate() {
            return Input.GetButton("Mouse0") ? 1 : 0;
    }

    public static bool ToggleCoinshotMode() {
            return Input.GetKeyDown(KeyCode.C);
    }

    public static bool ToggleHelpOverlay() {
            return Input.GetKeyDown(KeyCode.H);
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
            timeToHoldDPadY = Time.time + .1f;
            return Input.GetAxis("GamepadDPadY");
        } else {
            return 0;
        }
    }

    public static float DPadXAxis() {
        if (timeToHoldDPadX < Time.time) {
            timeToHoldDPadX = Time.time + .1f;
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
