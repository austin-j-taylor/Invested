using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the screen that appears when a Harmony Target is entered and the level is compelted.
/// </summary>
public class LevelCompletedScreen : MonoBehaviour {

    private Button continueButton;
    private Button returnButton;

    private static LevelCompletedScreen instance;

    public static HarmonyTarget InTarget { get; set; }
    public static bool IsOpen {
        get {
            return instance.gameObject.activeSelf;
        }
    }

    void Start() {
        Button[] buttons = GetComponentsInChildren<Button>();
        returnButton = buttons[0];
        continueButton = buttons[1];

        continueButton.onClick.AddListener(ClickContinue);
        returnButton.onClick.AddListener(ClickReturn);

        instance = this;
        gameObject.SetActive(false);
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            Close();
        }
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
        instance.gameObject.SetActive(true);
        instance.continueButton.gameObject.SetActive(!SceneSelectMenu.IsCurrentSceneTutorial);
        CameraController.UnlockCamera();
        MainMenu.FocusOnButton(instance.transform);
    }
    public static void Close() {
        instance.gameObject.SetActive(false);
    }
}
