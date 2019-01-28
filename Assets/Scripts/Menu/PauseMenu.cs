using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool IsPaused { get; private set; }

    private static SettingsMenu settingsMenu;

    private Button unpauseButton;
    private Button settingsButton;
    private Button resetButton;
    private Button quitButton;

    private static GameObject pauseMenu;

    // Use this for initialization
    void Awake() {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();

        Button[] buttons = GetComponentsInChildren<Button>();
        unpauseButton = buttons[0];
        settingsButton = buttons[1];
        resetButton = buttons[2];
        quitButton = buttons[3];

        unpauseButton.onClick.AddListener(ClickUnpause);
        settingsButton.onClick.AddListener(ClickSettings);
        resetButton.onClick.AddListener(ClickReset);
        quitButton.onClick.AddListener(ClickQuit);

        pauseMenu = gameObject;
        gameObject.SetActive(false);
        IsPaused = false;
    }
    private void LateUpdate() {
        if (Keybinds.EscapeDown()) {
            if (settingsMenu.IsOpen) {
                settingsMenu.BackAndSaveSettings();
            } else {
                UnPause();
            }
        }
    }

    public static void Pause() {
        CameraController.UnlockCamera();
        GamepadController.SetRumble(0, 0);
        Time.timeScale = 0f;

        pauseMenu.SetActive(true);
        IsPaused = true;
    }

    public static void UnPause() {
        settingsMenu.Close();

        CameraController.LockCamera();
        Time.timeScale = SettingsMenu.settingsData.timeScale;
        pauseMenu.SetActive(false);
        IsPaused = false;
    }

    private void ClickUnpause() {
        UnPause();
    }

    private void ClickSettings() {
        settingsMenu.Open();
    }

    private void ClickReset() {
        SceneSelectMenu.ReloadScene();
    }

    private void ClickQuit() {
        CameraController.UnlockCamera();
        SceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
