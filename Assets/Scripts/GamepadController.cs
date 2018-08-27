using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
/*
 * GamepadController is a mostly static class used to set controller vibrations.
 * GamepadRumble actually sends the vibration values to the GamePad/deals with IEnumerators.
 */
public class GamepadController : MonoBehaviour {
    
    private static bool updateRumble = false;
    private static bool usingGamepad = false;
    private static bool usingRumble = true;
    private static bool shaking = false;
    private static float leftRumble = 0;
    private static float rightRumble = 0;
    public const float rumbleFactor = .3f;

    private static GamepadRumble rumble;

    private void Start() {
        rumble = gameObject.AddComponent<GamepadRumble>();
    }

    public static bool UsingGamepad {
        get {
            return usingGamepad;
        }
        set {
            usingGamepad = value;
            updateRumble = true;
        }
    }
    public static bool UsingRumble {
        get {
            return usingRumble;
        }
        set {
            usingRumble = value;
            updateRumble = true;
        }
    }
    public static float LeftRumble {
        get {
            return leftRumble;
        }
        set {
            leftRumble = value;
            updateRumble = true;
        }
    }
    public static float RightRumble {
        get {
            return rightRumble;
        }
        set {
            rightRumble = value;
            updateRumble = true;
        }
    }
    
    public static void SetRumble(float left, float right) {
        leftRumble = left;
        rightRumble = right;
        updateRumble = true;
    }

    public static void SetRumbleLeft(float left) {
        leftRumble = left;
        updateRumble = true;
    }

    public static void SetRumbleRight(float right) {
        rightRumble = right;
        updateRumble = true;
    }

    public static void Shake(float left, float right, float time = .1f) {
        if (usingGamepad && usingRumble)
            rumble.Shake(left, right, time);
    }

    private class GamepadRumble : MonoBehaviour {
        
        // Update is called once per frame
        void Update() {
            if (usingGamepad) {
                if (usingRumble && updateRumble && !shaking) {
                    GamePad.SetVibration(0, leftRumble, rightRumble);
                    updateRumble = false;
                } else if (updateRumble && !usingRumble) {
                    GamePad.SetVibration(0, 0, 0);
                }
            }
        }

        // Overrides the current left and right rumbles to shake the controller for a short time.
        public void Shake(float left, float right, float time) {
            StartCoroutine(ShakeController(left, right, time));
        }

        public IEnumerator ShakeController(float left, float right, float time) {
            if (UsingGamepad && UsingRumble) {
                shaking = true;
                GamePad.SetVibration(0, left, right);
                yield return new WaitForSeconds(time);
                GamePad.SetVibration(0, leftRumble, RightRumble);
                shaking = false;
            }
        }
    }
}
