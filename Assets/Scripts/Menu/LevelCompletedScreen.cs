using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedScreen : MonoBehaviour {

    private Button continueButton;
    private Button returnButton;

    private static GameObject levelCompletedScreen;

    // Use this for initialization
    void Start() {
        Button[] buttons = GetComponentsInChildren<Button>();
        continueButton = buttons[0];
        returnButton = buttons[1];

        continueButton.onClick.AddListener(ClickContinue);
        returnButton.onClick.AddListener(ClickReturn);

        levelCompletedScreen = gameObject;
        gameObject.SetActive(false);
    }

    private void ClickContinue() {
        FindObjectOfType<HarmonyTarget>().ReleasePlayer();
        gameObject.SetActive(false);
    }

    private void ClickReturn() {
        SceneSelectMenu.LoadScene(SceneSelectMenu.sceneTitleScreen);
        gameObject.SetActive(false);
        MainMenu.OpenSceneSelectMenu();
    }

    public static void OpenScreen() {
        levelCompletedScreen.SetActive(true);
        CameraController.UnlockCamera();
        MainMenu.FocusOnCurrentMenu(levelCompletedScreen.transform);
    }
}
