using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

    public static bool IsPaused { get; private set; }

    private static SettingsMenu settingsMenu;

    private static Transform buttonsHeader;
    private static Transform titleText;
    private Button unpauseButton;
    private Button settingsButton;
    private Button resetButton;
    private Button quitButton;
    private Text resetText, quitText;

    private static PauseMenu instance;

    // Use this for initialization
    void Awake() {
        instance = this;

        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();

        buttonsHeader = transform.Find("Header");
        titleText = transform.Find("TitleText");
        Button[] buttons = buttonsHeader.GetComponentsInChildren<Button>();
        unpauseButton = buttons[0];
        settingsButton = buttons[1];
        resetButton = buttons[2];
        quitButton = buttons[3];
        resetText = resetButton.GetComponentInChildren<Text>();
        quitText = quitButton.GetComponentInChildren<Text>();

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
        titleText.gameObject.SetActive(true);
        MainMenu.FocusOnButton(instance.unpauseButton);
    }

    public static void Close() {
        buttonsHeader.gameObject.SetActive(false);
        titleText.gameObject.SetActive(false);
    }

    public static void Pause() {
        CameraController.UnlockCamera();
        Time.timeScale = 0f;
        GameManager.AudioManager.SetMasterPitch(0);
        HUD.DisableHUD();

        // Update blue lines for this frame
        Player.PlayerIronSteel.UpdateBlueLines();

        instance.gameObject.SetActive(true);
        Open();
        IsPaused = true;

        switch (GameManager.State) {
            case GameManager.GameState.Standard:
                instance.resetText.text = "Return to Checkpoint";
                instance.quitText.text = "Quit Level";
                instance.settingsButton.gameObject.SetActive(true);
                break;
            case GameManager.GameState.Challenge:
                instance.resetText.text = "Restart Challenge";
                instance.quitText.text = "Exit Challenge";
                instance.settingsButton.gameObject.SetActive(false);
                break;
        }
    }

    public static void UnPause() {
        if (IsPaused) {
            settingsMenu.Close();

            if(!LevelCompletedScreen.IsOpen) {
                CameraController.LockCamera();
            }
            Time.timeScale = TimeController.CurrentTimeScale;
            if(SettingsMenu.settingsData.hudEnabled == 1)
                HUD.EnableHUD();
            GameManager.AudioManager.SetMasterPitch(1);
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

        switch (GameManager.State) {
            case GameManager.GameState.Standard:
                //SceneSelectMenu.ReloadScene();
                Player.PlayerInstance.Respawn();
                break;
            case GameManager.GameState.Challenge:
                ChallengesManager.RestartCurrentChallenge();
                UnPause();
                break;
        }
    }

    private void ClickQuit() {
        switch (GameManager.State) {
            case GameManager.GameState.Standard:
                CameraController.UnlockCamera();
                SceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
                break;
            case GameManager.GameState.Challenge:
                ChallengesManager.LeaveCurrentChallenge();
                UnPause();
                break;
        }
    }
}
