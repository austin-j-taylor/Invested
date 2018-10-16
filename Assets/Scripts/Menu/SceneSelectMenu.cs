using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSelectMenu : MonoBehaviour {

    // Scene build indices
    public const int sceneMain = 0;
    public const int sceneTitleScreen = 1;
    public const int sceneTutorial = 2;
    public const int sceneSandbox = 3;
    public const int sceneLuthadel = 4;
    public const int sceneExperimental = 5;

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private Image titleScreenBG;
    private Button tutorialButton;
    private Button sandboxButton;
    private Button luthadelButton;
    private Button experimentalButton;
    private Button backButton;

    void Start() {
        titleScreenBG = transform.parent.GetComponent<Image>();

        Button[] buttons = GetComponentsInChildren<Button>();
        tutorialButton = buttons[0];
        sandboxButton = buttons[1];
        luthadelButton = buttons[2];
        experimentalButton = buttons[3];
        backButton = buttons[4];

        tutorialButton.onClick.AddListener(OnClickedTutorial);
        sandboxButton.onClick.AddListener(OnClickedSandbox);
        luthadelButton.onClick.AddListener(OnClickedLuthadel);
        experimentalButton.onClick.AddListener(OnClickedExperimental);
        backButton.onClick.AddListener(OnClickedBack);

        // Only close the main menu after the scene loads to prevent jarring camera transitions
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    public void OpenSceneSelect() {
        gameObject.SetActive(true);
    }

    public void CloseSceneSelect() {
        gameObject.SetActive(false);
    }

    public static void LoadScene(int scene) {
        CameraController.ExternalPositionTarget = null;

        SceneManager.LoadScene(scene);
    }

    public static void ReloadScene() {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            PauseMenu.UnPause();

            if (scene.buildIndex != sceneTitleScreen) {
                titleScreenBG.gameObject.SetActive(false);
                CloseSceneSelect();

                Player.PlayerInstance.gameObject.SetActive(true);
                CameraController.LockCamera();
                HUD.ResetHUD();

            } else {
                Player.PlayerInstance.gameObject.SetActive(false);
                CameraController.UnlockCamera();
            }
        }
    }

    private void OnClickedTutorial() {
        LoadScene(sceneTutorial);
    }

    private void OnClickedSandbox() {
        LoadScene(sceneSandbox);
    }

    private void OnClickedLuthadel() {
        LoadScene(sceneLuthadel);
    }

    private void OnClickedExperimental() {
        LoadScene(sceneExperimental);
    }

    private void OnClickedBack() {
        //mainMenu.OpenMainMenu();
        CloseSceneSelect();
    }
}
