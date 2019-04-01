using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneSelectMenu : MonoBehaviour {

    // Scene build indices
    public const int sceneMain = 0;
    public const int sceneTitleScreen = 1;
    public const int sceneLevel01 = 2;
    public const int sceneLuthadel = 3;
    public const int sceneShootingGrounds = 4;
    public const int sceneSandbox = 5;
    public const int sceneSouthernMountains = 6;
    public const int sceneSimulationDuel = 7;
    public const int sceneSimulationWall = 8;
    public const int sceneSimulationGround = 9;

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private Text tooltip;
    private Transform levelsHeader;
    private Transform sandboxesHeader;
    private Transform simulationsHeader;
    private Button level01Button;
    private Button luthadelButton;
    private Button shootingGroundsButton;
    private Button sandboxButton;
    private Button southernMountainsButton;
    private Button simulationDuelButton;
    private Button simulationWallButton;
    private Button simulationGroundButton;
    private Button backButton;

    void Start() {
        tooltip = transform.Find("Tooltip").GetComponent<Text>();
        levelsHeader = transform.Find("Levels").transform;
        sandboxesHeader = transform.Find("Sandboxes").transform;
        simulationsHeader = transform.Find("Simulations").transform;

        Button[] buttonsLevels = levelsHeader.GetComponentsInChildren<Button>();
        Button[] buttonsSandboxes = sandboxesHeader.GetComponentsInChildren<Button>();
        Button[] buttonsSimulations = simulationsHeader.GetComponentsInChildren<Button>();
        level01Button = buttonsLevels[0];
        luthadelButton = buttonsSandboxes[0];
        shootingGroundsButton = buttonsSandboxes[1];
        sandboxButton = buttonsSandboxes[2];
        southernMountainsButton = buttonsSandboxes[3];
        simulationDuelButton = buttonsSimulations[0];
        simulationWallButton = buttonsSimulations[1];
        simulationGroundButton = buttonsSimulations[2];
        backButton = transform.Find("Back").GetComponent<Button>();

        level01Button.onClick.AddListener(OnClickedLevel01Button);
        luthadelButton.onClick.AddListener(OnClickedLuthadel);
        shootingGroundsButton.onClick.AddListener(OnClickedShootingGrounds);
        sandboxButton.onClick.AddListener(OnClickedSandbox);
        southernMountainsButton.onClick.AddListener(OnClickedSouthernMountains);
        simulationDuelButton.onClick.AddListener(OnClickedSimulationDuel);
        simulationWallButton.onClick.AddListener(OnClickedSimulationWall);
        simulationGroundButton.onClick.AddListener(OnClickedSimulationGround);
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
                Time.fixedDeltaTime = SettingsMenu.settingsData.timeScale / 60;

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

    private void OnClickedLevel01Button() {
        LoadScene(sceneLevel01);
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

    private void OnClickedSimulationDuel() {
        LoadScene(sceneSimulationDuel);
    }

    private void OnClickedSimulationWall() {
        LoadScene(sceneSimulationWall);
    }

    private void OnClickedSimulationGround() {
        LoadScene(sceneSimulationGround);
    }

    private void OnClickedBack() {
        Close();
    }
}
