using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/*
 * Knows what time scale the game should be playing at when not paused.
 */
public class TimeController : MonoBehaviour {

    private const float fixedTimeRatio = 1 / 60.0f / 2.0f;
    private static float currentScale;

    public static float CurrentTimeScale {
        get {
            return currentScale;
        }
        set {
            if (currentScale != value && value > 0) {
                Time.fixedDeltaTime = value * fixedTimeRatio;
            }
            currentScale = value;
            if (MainMenu.IsOpen || PauseMenu.IsPaused) {
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
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        CurrentTimeScale = SettingsMenu.settingsData.timeScale;
    }
}
