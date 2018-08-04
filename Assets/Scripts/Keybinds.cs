using UnityEngine;

/* 
 * Contains all keybinds. Allows for easy switching between keyboard and gamepad playing using the GamepadController.
 */

public static class Keybinds {
    
    private static float timeToHold = 0f;

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
            return Input.GetButton("GamepadA");
        else
            return Input.GetKey(KeyCode.Space);
    }
    public static bool JumpDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("GamepadA");
        else
            return Input.GetKeyDown(KeyCode.Space);
    }

    public static bool WithdrawCoinDown() {
        if (GamepadController.UsingGamepad)
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
        else if (GamepadController.UsingMB45)
            return Input.GetButtonDown("Mouse4");
        else
            return Input.GetKeyDown(KeyCode.E);
    }
    public static bool Select() {
        if (GamepadController.UsingGamepad)
            return Input.GetButton("RightBumper");
        else if (GamepadController.UsingMB45)
            return Input.GetButton("Mouse4");
        else
            return Input.GetKey(KeyCode.E);
    }

    public static bool SelectAlternate() {
        if (GamepadController.UsingGamepad)
            return Input.GetButton("LeftBumper");
        else if (GamepadController.UsingMB45)
            return Input.GetButton("Mouse3");
        else
            return Input.GetKey(KeyCode.Q);
    }
    public static bool SelectAlternateDown() {
        if (GamepadController.UsingGamepad)
            return Input.GetButtonDown("LeftBumper");
        else if (GamepadController.UsingMB45)
            return Input.GetButtonDown("Mouse3");
        else
            return Input.GetKeyDown(KeyCode.Q);
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

    public static float ScrollWheelAxis() {
        if(GamepadController.UsingGamepad) {
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
        if (GamepadController.UsingGamepad) {
            return false;
        } else {
            return Input.GetButton("Mouse2");
        }
    }

    public static bool EscapeDown() {
        //if (GamepadController.UsingGamepad) {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("GamepadStart");
        //} else {
        //    return Input.GetKeyDown(KeyCode.EscapeDown);
        //}
    }

}
