using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedScreen : MonoBehaviour {

    private Button continueButton;
    private Button returnButton;

    private static GameObject levelCompletedScreen;

    public static HarmonyTarget InTarget { get; set; }

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
        MainMenu.FocusOnCurrentMenu(levelCompletedScreen.transform);
    }
}
