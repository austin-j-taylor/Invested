using UnityEngine;
using System.Collections;

/*
 * Knows what time scale the game should be playing at when not paused.
 */
public class TimeController : MonoBehaviour {

    private static float currentScale;

    public static float CurrentTimeScale {
        get {
            return currentScale;
        }
        set {
            if (currentScale != value && value > 0) {
                Time.fixedDeltaTime = value / 60;
            }
            currentScale = value;
            if(MainMenu.IsOpen || PauseMenu.IsPaused) {
                // time scale stay at zero
                Time.timeScale = 0;
            } else {
                Time.timeScale = currentScale;
            }
        }
    }

    // Use this for initialization
    void Awake() {
        currentScale = -1;
    }
}
