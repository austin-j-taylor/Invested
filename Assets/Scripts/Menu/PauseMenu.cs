using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool IsPaused { get; private set; }

    private static SettingsMenu settingsMenu;
    private MainMenu mainMenu;
    
    private Button unpauseButton;
    private Button settingsButton;
    private Button resetButton;
    private Button quitButton;

    private static GameObject pauseMenu;

    // Use this for initialization
    void Awake () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();
        mainMenu = transform.parent.GetComponentInChildren<MainMenu>();

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
    private void Update() {
        if (Keybinds.EscapeDown()) {
            if(settingsMenu.IsOpen) {
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

        // Update blue lines on frame of pausing
        if(Player.PlayerIronSteel && Player.PlayerIronSteel.IsBurningIronSteel) {
            Player.PushPullController.SearchForMetals();
        }
        CameraController.UpdateCamera();
    }

    public static void UnPause() {
        settingsMenu.CloseSettings();

        CameraController.LockCamera();
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        IsPaused = false;
    }

    private void ClickUnpause() {
        UnPause();
    }

    private void ClickSettings() {
        settingsMenu.OpenSettings();
    }

    private void ClickReset() {
        SceneSelectMenu.ReloadScene();
    }

    private void ClickQuit() {
        CameraController.UnlockCamera();
        mainMenu.OpenMainMenu();
        SceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
