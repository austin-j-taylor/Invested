using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    private SettingsMenu settingsMenu;
    
    private Button settingsButton;
    private Button unpauseButton;
    private Button resetButton;
    private Button quitButton;

    private bool paused;

    // Use this for initialization
    void Start () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();

        Button[] buttons = GetComponentsInChildren<Button>();
        settingsButton = buttons[0];
        unpauseButton = buttons[1];
        resetButton = buttons[2];
        quitButton = buttons[3];

        settingsButton.onClick.AddListener(ClickSettings);
        unpauseButton.onClick.AddListener(ClickUnpause);
        resetButton.onClick.AddListener(ClickReset);
        quitButton.onClick.AddListener(ClickQuit);

        gameObject.SetActive(false);
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
        gameObject.SetActive(true);
        paused = true;
    }

    private void UnPause() {
        settingsMenu.CloseSettings();

        //Cursor.visible = false;
        FPVCameraLock.LockCamera();
        Time.timeScale = 1f;
        gameObject.SetActive(false);
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
        MainMenu.LoadScene(SceneManager.GetActiveScene());
    }

    private void ClickQuit() {
        Application.Quit();
    }
}
