using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChallengeMenu : MonoBehaviour {

    private Text challengeText;
    private Button startButton, exitButton;
    private Challenge currentChallenge;

    private static ChallengeMenu instance;

    public static bool IsOpen {
        get {
            return instance.gameObject.activeSelf;
        }
    }

    private void Start() {
        challengeText = transform.Find("ChallengeText").GetComponent<Text>();
        startButton = transform.Find("Header/StartButton").GetComponent<Button>();
        exitButton = transform.Find("Header/ExitButton").GetComponent<Button>();
        startButton.onClick.AddListener(OnClickStart);
        exitButton.onClick.AddListener(OnClickExit);
        instance = this;

        gameObject.SetActive(false);
    }

    public static void Open(Challenge challenge) {
        HUD.DisableHUD();
        CameraController.UnlockCamera();
        Player.CanPause = false;
        Player.CanControl = false;

        instance.gameObject.SetActive(true);
        instance.startButton.gameObject.SetActive(true);
        MainMenu.FocusOnButton(instance.startButton);
        instance.currentChallenge = challenge;
        instance.challengeText.text = "Challenge - " + challenge.challengeName;

    }
    public static void Close() {
        instance.gameObject.SetActive(false);
        HUD.EnableHUD();
        CameraController.LockCamera();
        Player.CanPause = true;
        Player.CanControl = true;
    }

    public static void LeaveCurrentChallenge() {
        instance.currentChallenge.LeaveChallenge();
    }
    public static void RestartCurrentChallenge() {
        instance.currentChallenge.StartChallenge();
    }

    private void OnClickStart() {
        Close();
        currentChallenge.StartChallenge();
    }
    private void OnClickExit() {
        Close();
        currentChallenge.LeaveChallenge();
    }
}
