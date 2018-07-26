using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    private static SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;
    private MainMenu mainMenu;
    
    private Button settingsButton;
    private Button unpauseButton;
    private Button resetButton;
    private Button quitButton;

    private static GameObject pauseMenu;
    private static bool paused;

    // Use this for initialization
    void Start () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();
        sceneSelectMenu = transform.parent.GetComponentInChildren<SceneSelectMenu>();
        mainMenu = transform.parent.GetComponentInChildren<MainMenu>();

        Button[] buttons = GetComponentsInChildren<Button>();
        settingsButton = buttons[0];
        unpauseButton = buttons[1];
        resetButton = buttons[2];
        quitButton = buttons[3];

        settingsButton.onClick.AddListener(ClickSettings);
        unpauseButton.onClick.AddListener(ClickUnpause);
        resetButton.onClick.AddListener(ClickReset);
        quitButton.onClick.AddListener(ClickQuit);

        pauseMenu = gameObject;
        gameObject.SetActive(false);
        paused = false;
    }

    public static void TogglePaused() {
        if (paused)
            UnPause();
        else
            Pause();
    }

    private static void Pause() {
        //Cursor.visible = true;
        FPVCameraLock.UnlockCamera();
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        paused = true;
    }

    private static void UnPause() {
        settingsMenu.CloseSettings();

        //Cursor.visible = false;
        FPVCameraLock.LockCamera();
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        paused = false;
    }

    private void ClickSettings() {
        settingsMenu.OpenSettings();
    }

    private void ClickUnpause() {
        UnPause();
    }

    private void ClickReset() {
        UnPause();
        sceneSelectMenu.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClickQuit() {
        UnPause();
        FPVCameraLock.UnlockCamera();
        mainMenu.OpenMenu();
        sceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
