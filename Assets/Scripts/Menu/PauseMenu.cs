using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Pause Menu
/// </summary>
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

    #region clearing
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
        SceneManager.sceneLoaded += ClearAfterSceneChange;

        gameObject.SetActive(false);
        IsPaused = false;
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            if (scene.buildIndex != SceneSelectMenu.sceneTitleScreen)
                MainMenu.Close();
            UnPause();
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
    #endregion

    private void LateUpdate() {
        if (Keybinds.ExitMenu()) {
            if (settingsMenu.IsOpen) {
                settingsMenu.BackAndSaveSettings();
            } else {
                UnPause();
            }
        }
    }

    public static void Pause() {
        if (!IsPaused) {
            IsPaused = true;
            CameraController.UnlockCamera();
            Time.timeScale = 0f;
            GameManager.AudioManager.SetMasterPitch(0);
            HUD.DisableHUD();

            // Update blue lines for this frame
            Player.PlayerIronSteel.UpdateBlueLines();

            instance.gameObject.SetActive(true);
            Open();

            switch (GameManager.State) {
                case GameManager.GameState.Standard:
                    instance.resetText.text = "Return to Checkpoint";
                    instance.quitText.text = "Quit Level";
                    instance.settingsButton.gameObject.SetActive(true);
                    instance.resetButton.gameObject.SetActive(true);
                    instance.quitButton.gameObject.SetActive(true);
                    break;
                case GameManager.GameState.Challenge:
                    instance.resetText.text = "Restart Challenge";
                    instance.quitText.text = "Exit Challenge";
                    instance.settingsButton.gameObject.SetActive(false);
                    instance.resetButton.gameObject.SetActive(true);
                    instance.quitButton.gameObject.SetActive(true);
                    break;
                case GameManager.GameState.Cutscene:
                    instance.settingsButton.gameObject.SetActive(false);
                    instance.resetButton.gameObject.SetActive(false);
                    instance.quitButton.gameObject.SetActive(false);
                    break;
            }
        }
    }

    public static void UnPause() {
        if (IsPaused) {
            IsPaused = false;
            settingsMenu.Close();

            if (!LevelCompletedScreen.IsOpen && !MainMenu.IsOpen) { // If there is another menu open. Change this if we rework menus.
                CameraController.LockCamera();
            }
            if (SettingsMenu.settingsInterface.hudEnabled == 1)
                HUD.EnableHUD();
            GameManager.AudioManager.SetMasterPitch(1);
            Close();
            instance.gameObject.SetActive(false);
        }
        TimeController.CurrentTimeScale = TimeController.CurrentTimeScale;
    }

    #region OnClick
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
                UnPause();
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
    #endregion
}
