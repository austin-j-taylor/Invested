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

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private Image titleScreenBG;
    private Button tutorialButton;
    private Button sandboxButton;
    private Button luthadelButton;
    private Button backButton;

    //private MainMenu mainMenu;
    private static HUD hud;
    private static GameObject player;

    void Start() {
        titleScreenBG = transform.parent.GetComponent<Image>();
        //mainMenu = transform.parent.GetComponentInChildren<MainMenu>();
        hud = transform.parent.parent.GetComponentInChildren<HUD>();
        player = GameObject.FindGameObjectWithTag("Player");

        Button[] buttons = GetComponentsInChildren<Button>();
        tutorialButton = buttons[0];
        sandboxButton = buttons[1];
        luthadelButton = buttons[2];
        backButton = buttons[3];

        tutorialButton.onClick.AddListener(OnClickedTutorial);
        sandboxButton.onClick.AddListener(OnClickedSandbox);
        luthadelButton.onClick.AddListener(OnClickedLuthadel);
        backButton.onClick.AddListener(OnClickedBack);

        // Only close the main menu after the scene loads to prevent jarring camera transitions
        SceneManager.sceneLoaded += ExitMainMenu;

        player.SetActive(false);
        gameObject.SetActive(false);
        hud.DisableHUD();
    }

    public void OpenSceneSelect() {
        gameObject.SetActive(true);
    }

    public void CloseSceneSelect() {
        gameObject.SetActive(false);
    }

    private void ExitMainMenu(Scene scene, LoadSceneMode mode) {
        if (scene.buildIndex != sceneTitleScreen) {
            titleScreenBG.gameObject.SetActive(false);
            CloseSceneSelect();
        }
    }

    public void LoadScene(int scene) {
        if (scene == sceneTitleScreen) {
            player.gameObject.SetActive(false);
            FPVCameraLock.UnlockCamera();
        } else {
            player.gameObject.SetActive(true);
            FPVCameraLock.LockCamera();
        }
        hud.ResetHUD();
        player.GetComponent<Player>().ReloadPlayerIntoNewScene(scene);

        SceneManager.LoadScene(scene);
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

    private void OnClickedBack() {
        //mainMenu.OpenMainMenu();
        CloseSceneSelect();
    }
}
