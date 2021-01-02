using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the screen that appears when a Harmony Target is entered and the level is compelted.
/// </summary>
public class LevelCompletedMenu : Menu {

    private Button continueButton;
    private Button returnButton;

    public static HarmonyTarget InTarget { get; set; }

    void Start() {
        Button[] buttons = GetComponentsInChildren<Button>();
        returnButton = buttons[0];
        continueButton = buttons[1];

        continueButton.onClick.AddListener(ClickContinue);
        returnButton.onClick.AddListener(ClickReturn);

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
        GameManager.SceneTransitionManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
        gameObject.SetActive(false);
        GameManager.MenusController.mainMenu.sceneSelectMenu.Open();
    }

    public void OpenScreen(HarmonyTarget target) {
        InTarget = target;
        gameObject.SetActive(true);
        continueButton.gameObject.SetActive(!SceneSelectMenu.IsCurrentSceneTutorial);
        CameraController.UnlockCamera();
        MainMenu.FocusOnButton(transform);
    }
}
