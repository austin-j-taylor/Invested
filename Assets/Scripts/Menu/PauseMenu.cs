using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Pause Menu
/// </summary>
public class PauseMenu : Menu {

    public bool IsPaused => IsOpen;

    private static Transform buttonsHeader;
    private static Transform titleText;
    private Button unpauseButton;
    private Button settingsButton;
    private Button textLogButton;
    private Button resetButton;
    private Button quitButton;
    private Text resetText, quitText;

    #region clearing
    void Awake() {
        buttonsHeader = transform.Find("Header");
        titleText = transform.Find("TitleText");
        Button[] buttons = buttonsHeader.GetComponentsInChildren<Button>();
        unpauseButton = buttons[0];
        settingsButton = buttons[1];
        textLogButton = buttons[2];
        resetButton = buttons[3];
        quitButton = buttons[4];
        resetText = resetButton.GetComponentInChildren<Text>();
        quitText = quitButton.GetComponentInChildren<Text>();

        unpauseButton.onClick.AddListener(ClickUnpause);
        settingsButton.onClick.AddListener(ClickSettings);
        textLogButton.onClick.AddListener(ClickTextLog);
        resetButton.onClick.AddListener(ClickReset);
        quitButton.onClick.AddListener(ClickQuit);
        SceneManager.sceneLoaded += ClearAfterSceneChange;

        gameObject.SetActive(false);
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            if(IsOpen)
                Close();
        }
    }

    public override void Open() {
        base.Open();
        CameraController.UnlockCamera();
        Time.timeScale = 0f;
        GameManager.AudioManager.SetMasterPitch(0);
        HUD.TextLogController.Close();
        HUD.DisableHUD();

        // Update blue lines for this frame
        Prima.PrimaInstance.ActorIronSteel.UpdateBlueLines();
        
        switch (GameManager.CameraState) {
            case GameManager.GameCameraState.Standard:
                settingsButton.gameObject.SetActive(true);
                switch (GameManager.PlayState) {
                    case GameManager.GamePlayState.Standard:
                        resetText.text = "Return to Checkpoint";
                        quitText.text = "Quit Level";
                        resetButton.gameObject.SetActive(true);
                        quitButton.gameObject.SetActive(true);
                        break;
                    case GameManager.GamePlayState.Challenge:
                        resetText.text = "Restart Challenge";
                        quitText.text = "Exit Challenge";
                        resetButton.gameObject.SetActive(true);
                        quitButton.gameObject.SetActive(true);
                        break;
                }
                break;
            case GameManager.GameCameraState.Cutscene:
                settingsButton.gameObject.SetActive(false);
                resetButton.gameObject.SetActive(false);
                //quitButton.gameObject.SetActive(false);
                break;
        }

        buttonsHeader.gameObject.SetActive(true);
        titleText.gameObject.SetActive(true);
        MainMenu.FocusOnButton(unpauseButton);
    }

    public override void Close() {
        base.Close();

        if (!GameManager.MenusController.levelCompletedMenu.IsOpen && !GameManager.MenusController.mainMenu.IsOpen) { // If there is another menu open. Change this if we rework menus.
            CameraController.LockCamera();
        }
        HUD.EnableHUD();
        GameManager.AudioManager.SetMasterPitch(1);

        TimeController.CurrentTimeScale = TimeController.CurrentTimeScale;

    }

    public override void HideContents() {
        buttonsHeader.gameObject.SetActive(false);
        titleText.gameObject.SetActive(false);
    }
    #endregion
    
    #region OnClick
    private void ClickUnpause() {
        Close();
    }

    private void ClickSettings() {
        GameManager.MenusController.settingsMenu.Open();
    }

    private void ClickTextLog() {
        Close();
        HUD.TextLogController.Open();
    }

    private void ClickReset() {

        switch (GameManager.PlayState) {
            case GameManager.GamePlayState.Standard:
                //SceneSelectMenu.ReloadScene();
                Player.PlayerInstance.Respawn();
                Close();
                break;
            case GameManager.GamePlayState.Challenge:
                ChallengesManager.RestartCurrentChallenge();
                Close();
                break;
        }
    }

    private void ClickQuit() {
        switch (GameManager.PlayState) {
            case GameManager.GamePlayState.Standard:
                CameraController.UnlockCamera();
                Close();
                GameManager.SceneTransitionManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
                break;
            case GameManager.GamePlayState.Challenge:
                ChallengesManager.LeaveCurrentChallenge();
                Close();
                break;
        }
    }
    #endregion
}
