using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool IsPaused { get; private set; }

    private static SettingsMenu settingsMenu;

    private static Transform buttonsHeader;
    private Button unpauseButton;
    private Button settingsButton;
    private Button resetButton;
    private Button quitButton;

    private static PauseMenu instance;

    // Use this for initialization
    void Awake() {
        instance = this;

        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();

        buttonsHeader = transform.Find("Header");
        Button[] buttons = buttonsHeader.GetComponentsInChildren<Button>();
        unpauseButton = buttons[0];
        settingsButton = buttons[1];
        resetButton = buttons[2];
        quitButton = buttons[3];

        unpauseButton.onClick.AddListener(ClickUnpause);
        settingsButton.onClick.AddListener(ClickSettings);
        resetButton.onClick.AddListener(ClickReset);
        quitButton.onClick.AddListener(ClickQuit);

        gameObject.SetActive(false);
        IsPaused = false;
    }

    private void LateUpdate() {
        if (Keybinds.ExitMenu()) {
            if (settingsMenu.IsOpen) {
                settingsMenu.BackAndSaveSettings();
            } else {
                UnPause();
            }
        }
    }

    public static void Open() {
        buttonsHeader.gameObject.SetActive(true);
        MainMenu.FocusOnCurrentMenu(instance.transform);
    }

    public static void Close() {
        buttonsHeader.gameObject.SetActive(false);
    }

    public static void Pause() {
        CameraController.UnlockCamera();
        GamepadController.SetRumble(0, 0);
        Time.timeScale = 0f;

        // Update blue lines for this frame (workaround)
        Player.PlayerIronSteel.UpdateBlueLines();

        instance.gameObject.SetActive(true);
        Open();
        IsPaused = true;
    }

    public static void UnPause() {
        if (IsPaused) {
            settingsMenu.Close();

            if(!LevelCompletedScreen.IsOpen) {
                CameraController.LockCamera();
            }
            Time.timeScale = TimeController.CurrentTimeScale;
            Close();
            instance.gameObject.SetActive(false);
            IsPaused = false;
        }
    }


    private void ClickUnpause() {
        UnPause();
    }

    private void ClickSettings() {
        Close();
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
