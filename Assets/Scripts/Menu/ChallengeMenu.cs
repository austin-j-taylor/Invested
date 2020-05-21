using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChallengeMenu : MonoBehaviour {

    private Text challengeText, descriptionText, startText;
    private Button startButton, exitButton;

    private static ChallengeMenu instance;

    public static bool IsOpen {
        get {
            return instance.gameObject.activeSelf;
        }
    }

    private void Start() {
        challengeText = transform.Find("ChallengeText").GetComponent<Text>();
        descriptionText = challengeText.transform.Find("Description").GetComponent<Text>();
        startButton = transform.Find("Header/StartButton").GetComponent<Button>();
        startText = startButton.GetComponentInChildren<Text>();
        exitButton = transform.Find("Header/ExitButton").GetComponent<Button>();
        startButton.onClick.AddListener(OnClickStart);
        exitButton.onClick.AddListener(OnClickExit);
        instance = this;

        gameObject.SetActive(false);
    }

    public static void OpenIntroduction(Challenge challenge) {
        HUD.DisableHUD();
        CameraController.UnlockCamera();
        Player.CanPause = false;
        Player.CanControl = false;
        instance.startText.text = "Start Challenge";

        instance.gameObject.SetActive(true);
        MainMenu.FocusOnButton(instance.startButton);
        instance.challengeText.text = challenge.GetFullName();
        instance.descriptionText.text = challenge.challengeDescription;

    }
    public static void OpenFailure() {
        HUD.DisableHUD();
        CameraController.UnlockCamera();
        Player.CanPause = false;
        Player.CanControl = false;
        instance.startText.text = "Retry Challenge";
        instance.gameObject.SetActive(true);
        MainMenu.FocusOnButton(instance.startButton);
        instance.challengeText.text = "Challenge failed";
    }
    public static void Close() {
        instance.gameObject.SetActive(false);
        HUD.EnableHUD();
        CameraController.LockCamera();
        Player.CanPause = true;
        Player.CanControl = true;
    }

    private void OnClickStart() {
        Close();
        ChallengesManager.StartCurrentChallenge();
    }
    private void OnClickExit() {
        Close();
        ChallengesManager.LeaveCurrentChallenge();
    }
}
