using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

/// <summary>
/// Handles the menu that appears when a Challenge is started, giving an overview for it.
/// </summary>
public class ChallengeMenu : MonoBehaviour {

    private Text challengeText, descriptionText, startText, recommendedText;
    private Button startButton, exitButton;

    private static ChallengeMenu instance;

    public static bool IsOpen => instance.gameObject.activeSelf;

    private void Start() {
        challengeText = transform.Find("ChallengeText").GetComponent<Text>();
        descriptionText = challengeText.transform.Find("Description").GetComponent<Text>();
        recommendedText = transform.Find("RecommendedHeader/RecommendedText").GetComponent<Text>();
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
        // Set recommendations for challenge
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Recommended powers:");
        if (challenge.recommendedIron) {
            if (Prima.PrimaInstance.ActorIronSteel.IronReserve.IsEnabled)
                builder.AppendLine(" • " + TextCodes.Iron_proper);
            else
                builder.AppendLine(" • " + TextCodes.MidBlue("<Not yet remembered>"));
        }
        if (challenge.recommendedSteel) {
            if (Prima.PrimaInstance.ActorIronSteel.SteelReserve.IsEnabled)
                builder.AppendLine(" • " + TextCodes.Steel_proper);
            else
                builder.AppendLine(" • " + TextCodes.Red("<Not yet remembered>"));
        }
        if (challenge.recommendedPewter) {
            if (Prima.PlayerPewter.PewterReserve.IsEnabled)
                builder.AppendLine(" • " + TextCodes.Pewter_proper);
            else
                builder.AppendLine(" • " + TextCodes.PewterWhite("<Not yet remembered>"));
        }
        if (challenge.recommendedZinc) {
            if (Player.CanControlZinc)
                builder.AppendLine(" • " + TextCodes.Zinc);
            else
                builder.AppendLine(" • " + TextCodes.ZincBlue("<Not yet remembered>"));
        }
        if (challenge.recommendedCoins) {
            if (Player.CanThrowCoins)
                builder.AppendLine(" • " + TextCodes.O_Coins);
            else
                builder.AppendLine(" • " + TextCodes.Gold("<Not yet remembered>"));
        }
        builder.Remove(builder.Length - 1, 1); // remove trailing newline
        instance.recommendedText.text = builder.ToString();
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
