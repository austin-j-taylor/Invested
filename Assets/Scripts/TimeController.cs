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
            if(currentScale != value && value > 0) {
                Time.fixedDeltaTime = value / 60;
            }
            currentScale = value;
            if (!PauseMenu.IsPaused) {
                Time.timeScale = currentScale;
            }
        }
    }

    // Use this for initialization
    void Awake() {
        currentScale = -1;
    }

    // Update is called once per frame
    void Update() {

    }
}
