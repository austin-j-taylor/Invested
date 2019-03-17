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
    public const int sceneLevel01 = 6;

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private Text tooltip;
    private Transform levelsHeader;
    private Transform sandboxesHeader;
    private Button luthadelButton;
    private Button shootingGroundsButton;
    private Button sandboxButton;
    private Button southernMountainsButton;
    private Button level01Button;
    private Button backButton;

    void Start() {
        tooltip = transform.Find("Tooltip").GetComponent<Text>();
        levelsHeader = transform.Find("Levels").transform;
        sandboxesHeader = transform.Find("Sandboxes").transform;

        Button[] buttonsLevels = levelsHeader.GetComponentsInChildren<Button>();
        Button[] buttonsSandboxes = sandboxesHeader.GetComponentsInChildren<Button>();
        luthadelButton = buttonsSandboxes[0];
        shootingGroundsButton = buttonsSandboxes[1];
        sandboxButton = buttonsSandboxes[2];
        southernMountainsButton = buttonsSandboxes[3];
        level01Button = buttonsLevels[0];
        backButton = transform.Find("Back").GetComponent<Button>();
        
        luthadelButton.onClick.AddListener(OnClickedLuthadel);
        shootingGroundsButton.onClick.AddListener(OnClickedShootingGrounds);
        sandboxButton.onClick.AddListener(OnClickedSandbox);
        southernMountainsButton.onClick.AddListener(OnClickedSouthernMountains);
        level01Button.onClick.AddListener(OnClickedLevel01Button);
        backButton.onClick.AddListener(OnClickedBack);

        // Only close the main menu after the scene loads to prevent jarring camera transitions
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    public void Open() {
        gameObject.SetActive(true);
        tooltip.text = "";
        MainMenu.FocusOnCurrentMenu(transform);
    }

    public void Close() {
        gameObject.SetActive(false);
        MainMenu.OpenTitleScreen();
    }

    public static void LoadScene(int scene) {
        //CameraController.SetExternalSource(null, null);
        Player.PlayerInstance.transform.parent = GameObject.FindGameObjectWithTag("GameController").transform;
        if (scene == sceneTitleScreen) {
            MainMenu.Open();
            //MainMenu.OpenSceneSelectMenu();
        }
        
        SceneManager.LoadScene(scene);
    }

    public static void ReloadScene() {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            PauseMenu.UnPause();

            if (scene.buildIndex != sceneTitleScreen) {
                //Close();
                MainMenu.Close();

                Player.PlayerInstance.gameObject.SetActive(true);
                CameraController.LockCamera();
                HUD.ResetHUD();
            } else {
                Player.PlayerInstance.gameObject.SetActive(false);
                CameraController.UnlockCamera();
                if(isActiveAndEnabled)
                    MainMenu.FocusOnCurrentMenu(transform);
            }
        }
    }

    public void SetTooltip(string tip) {
        tooltip.text = tip;
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

    private void OnClickedLevel01Button() {
        LoadScene(sceneLevel01);
    }

    private void OnClickedBack() {
        Close();
    }
}
