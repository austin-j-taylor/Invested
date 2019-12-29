using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedScreen : MonoBehaviour {

    private Button continueButton;
    private Button returnButton;

    private static GameObject levelCompletedScreen;

    public static HarmonyTarget InTarget { get; set; }
    public static bool IsOpen {
        get {
            return levelCompletedScreen.activeSelf;
        }
    }

    // Use this for initialization
    void Start() {
        Button[] buttons = GetComponentsInChildren<Button>();
        returnButton = buttons[0];
        continueButton = buttons[1];

        continueButton.onClick.AddListener(ClickContinue);
        returnButton.onClick.AddListener(ClickReturn);

        levelCompletedScreen = gameObject;
        gameObject.SetActive(false);
    }

    private void ClickContinue() {
        InTarget.ReleasePlayer();
        gameObject.SetActive(false);
    }

    private void ClickReturn() {
        SceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
        gameObject.SetActive(false);
        MainMenu.OpenSceneSelectMenu();
    }

    public static void OpenScreen(HarmonyTarget target) {
        InTarget = target;
        levelCompletedScreen.SetActive(true);
        CameraController.UnlockCamera();
        MainMenu.FocusOnButton(levelCompletedScreen.transform);
    }
    public static void Close() {
        levelCompletedScreen.SetActive(false);
    }
}
