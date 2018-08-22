using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool IsPaused { get; private set; }

    private static SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;
    private MainMenu mainMenu;
    
    private Button unpauseButton;
    private Button settingsButton;
    private Button resetButton;
    private Button quitButton;

    private static GameObject pauseMenu;

    // Use this for initialization
    void Start () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();
        sceneSelectMenu = transform.parent.GetComponentInChildren<SceneSelectMenu>();
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
                settingsMenu.BackSettings();
            } else {
                UnPause();
            }
        }
    }

    public static void Pause() {
        //Cursor.visible = true;
        CameraController.UnlockCamera();
        GamepadController.SetRumble(0, 0);
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        IsPaused = true;
    }

    public static void UnPause() {
        settingsMenu.CloseSettings();

        //Cursor.visible = false;
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
        UnPause();
        sceneSelectMenu.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClickQuit() {
        UnPause();
        CameraController.UnlockCamera();
        mainMenu.OpenMainMenu();
        sceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
