using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Knows what time scale the game should be playing at when not paused.
/// </summary>
public class TimeController : MonoBehaviour {

    private const float fixedTimeRatio = 1 / 60.0f / 2.0f;

    private static float desiredScale = 1, actualScale = 1;
    public static float CurrentTimeScale {
        get {
            return desiredScale;
        }
        set {
            desiredScale = value;

            if (MainMenu.IsOpen || PauseMenu.IsPaused) {
                // time scale should stay at zero
                actualScale = 0;
            } else {
                actualScale = value;
            }

            Time.timeScale = actualScale;
            if (value > 0) {
                Time.fixedDeltaTime = value * fixedTimeRatio;
            }
        }
    }

    void Awake() {
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        CurrentTimeScale = SettingsMenu.settingsData.timeScale;
    }
}
