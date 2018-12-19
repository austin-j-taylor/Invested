using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSelectMenu : MonoBehaviour {

    // Scene build indices
    public const int sceneMain = 0;
    public const int sceneTitleScreen = 1;
    public const int sceneLuthadel = 2;
    public const int sceneShootingGrounds = 3;
    public const int sceneSandbox = 4;
    public const int sceneSouthernMountains = 5;

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private Image titleScreenBG;
    private Button luthadelButton;
    private Button shootingGroundsButton;
    private Button sandboxButton;
    private Button southernMountainsButton;
    private Button backButton;

    void Start() {
        titleScreenBG = transform.parent.GetComponent<Image>();

        Button[] buttons = GetComponentsInChildren<Button>();
        luthadelButton = buttons[0];
        shootingGroundsButton = buttons[1];
        sandboxButton = buttons[2];
        southernMountainsButton = buttons[3];
        backButton = buttons[4];

        luthadelButton.onClick.AddListener(OnClickedLuthadel);
        shootingGroundsButton.onClick.AddListener(OnClickedShootingGrounds);
        sandboxButton.onClick.AddListener(OnClickedSandbox);
        southernMountainsButton.onClick.AddListener(OnClickedSouthernMountains);
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
        //CameraController.SetExternalSource(null, null);
        Player.PlayerInstance.transform.parent = GameObject.FindGameObjectWithTag("GameController").transform;

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

    private void OnClickedLuthadel() {
        LoadScene(sceneLuthadel);
    }

    private void OnClickedShootingGrounds() {
        LoadScene(sceneShootingGrounds);
    }

    private void OnClickedSandbox() {
        LoadScene(sceneSandbox);
    }

    private void OnClickedSouthernMountains() {
        LoadScene(sceneSouthernMountains);
    }

    private void OnClickedBack() {
        //mainMenu.OpenMainMenu();
        CloseSceneSelect();
    }
}
