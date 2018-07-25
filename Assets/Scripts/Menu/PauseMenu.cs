using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    private SettingsMenu settingsMenu;

    private Image pauseMenu;
    private Button settingsButton;
    private Button unpauseButton;
    private Button resetButton;
    private Button quitButton;

    private bool paused;

    // Use this for initialization
    void Start () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();

        pauseMenu = GetComponent<Image>();

        Button[] buttons = GetComponentsInChildren<Button>();
        settingsButton = buttons[0];
        unpauseButton = buttons[1];
        resetButton = buttons[2];
        quitButton = buttons[3];

        settingsButton.onClick.AddListener(ClickSettings);
        unpauseButton.onClick.AddListener(ClickUnpause);
        resetButton.onClick.AddListener(ClickReset);
        quitButton.onClick.AddListener(ClickQuit);

        pauseMenu.gameObject.SetActive(false);
        paused = false;
    }

    public void TogglePaused() {
        if (paused)
            UnPause();
        else
            Pause();
    }

    private void Pause() {
        //Cursor.visible = true;
        FPVCameraLock.UnlockCamera();
        Time.timeScale = 0f;
        pauseMenu.gameObject.SetActive(true);
        paused = true;
    }

    private void UnPause() {
        //Cursor.visible = false;
        FPVCameraLock.LockCamera();
        Time.timeScale = 1f;
        pauseMenu.gameObject.SetActive(false);
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
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void ClickQuit() {
        Application.Quit();
    }
}
