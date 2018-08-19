using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class GamepadController : MonoBehaviour {
    
    private static bool updateRumble;
    private static bool usingGamepad;
    private static bool usingRumble;
    private bool shaking;
    private float leftRumble;
    private float rightRumble;

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
    public float LeftRumble {
        get {
            return leftRumble;
        }
        set {
            leftRumble = value;
            updateRumble = true;
        }
    }
    public float RightRumble {
        get {
            return rightRumble;
        }
        set {
            rightRumble = value;
            updateRumble = true;
        }
    }

    // Use this for initialization
    void Start() {
        usingGamepad = false;
        usingRumble = true;
        updateRumble = false;
        shaking = false;
    }
    

    // Update is called once per frame
    void Update() {
        if (usingGamepad) {
            if ((usingRumble && updateRumble && !shaking)) {
                GamePad.SetVibration(0, leftRumble, rightRumble);
                updateRumble = false;
            } else if (updateRumble && !usingRumble) {
                GamePad.SetVibration(0, 0, 0);
            }
        }
    }
    
    public void SetRumble(float left, float right) {
        leftRumble = left;
        rightRumble = right;
        updateRumble = true;
    }

    public void SetRumbleLeft(float left) {
        leftRumble = left;
        updateRumble = true;
    }

    public void SetRumbleRight(float right) {
        rightRumble = right;
        updateRumble = true;
    }

    // Overrides the current left and right rumbles to shake the controller for a short time.
    public void Shake(float left, float right, float time = .1f) {
        StartCoroutine(ShakeController(left, right, time));
    }

    public IEnumerator ShakeController(float left, float right, float time) {
        if (usingGamepad && usingRumble) {
            shaking = true;
            GamePad.SetVibration(0, left, right);
            yield return new WaitForSeconds(time);
            GamePad.SetVibration(0, leftRumble, rightRumble);
            shaking = false;
        }
    }




}
