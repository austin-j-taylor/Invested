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
    public const int sceneLevel02 = 10;
    public const int sceneLevel03 = 11;
    public const int sceneLevel04 = 12;

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
    private Button level02Button;
    private Button level03Button;
    private Button level04Button;
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
        
        level01Button = levelsHeader.Find("Tutorial01").GetComponent<Button>();
        level02Button = levelsHeader.Find("Tutorial02").GetComponent<Button>();
        level03Button = levelsHeader.Find("Tutorial03").GetComponent<Button>();
        level04Button = levelsHeader.Find("Tutorial04").GetComponent<Button>();
        luthadelButton = sandboxesHeader.Find("Luthadel").GetComponent<Button>();
        shootingGroundsButton = sandboxesHeader.Find("ShootingGrounds").GetComponent<Button>();
        sandboxButton = sandboxesHeader.Find("Sandbox").GetComponent<Button>();
        southernMountainsButton = sandboxesHeader.Find("SouthernMountains").GetComponent<Button>();
        simulationDuelButton = simulationsHeader.Find("Duel").GetComponent<Button>();
        simulationWallButton = simulationsHeader.Find("CoinWall").GetComponent<Button>();
        simulationGroundButton = simulationsHeader.Find("CoinGround").GetComponent<Button>();
        backButton = transform.Find("Back").GetComponent<Button>();

        level01Button.onClick.AddListener(OnClickedLevel01Button);
        level02Button.onClick.AddListener(OnClickedLevel02Button);
        level03Button.onClick.AddListener(OnClickedLevel03Button);
        level04Button.onClick.AddListener(OnClickedLevel04Button);
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
        // Lock levels when previous levels are not completed
        if(!FlagsController.GetLevel(FlagsController.Level.Complete03)) {
            level04Button.interactable = false;
        }
        if (!FlagsController.GetLevel(FlagsController.Level.Complete02)) {
            level03Button.interactable = false;
        }
        if (!FlagsController.GetLevel(FlagsController.Level.Complete01)) {
            level02Button.interactable = false;
        }
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

            if (scene.buildIndex == sceneTitleScreen) {

                Player.PlayerInstance.gameObject.SetActive(false);
                CameraController.UnlockCamera();
                if (isActiveAndEnabled)
                    MainMenu.FocusOnCurrentMenu(transform);
            } else {
                MainMenu.Close();
                TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;

                Player.PlayerInstance.gameObject.SetActive(true);
                CameraController.LockCamera();
                HUD.ResetHUD();

                // Set parameters for starting on certain scenes
                if (scene.buildIndex == sceneLevel01) {
                    Player.PlayerInstance.CoinHand.Pouch.Clear();
                    Player.PlayerIronSteel.IronReserve.SetMass(50);
                    Player.PlayerIronSteel.SteelReserve.SetMass(0);
                    Player.PlayerPewter.PewterReserve.SetMass(0);
                } else {
                    // For every scene except the tutorial, give metals and coins at the start.
                    Player.PlayerInstance.CoinHand.Pouch.Fill();
                    Player.PlayerIronSteel.IronReserve.SetMass(150);
                    Player.PlayerIronSteel.SteelReserve.SetMass(150);
                    Player.PlayerPewter.PewterReserve.SetMass(150);
                }

            }
        }
    }

    public void SetTooltip(string tip) {
        tooltip.text = tip;
    }

    private void OnClickedLevel01Button() {
        LoadScene(sceneLevel01);
    }
    private void OnClickedLevel02Button() {
        LoadScene(sceneLevel02);
    }
    private void OnClickedLevel03Button() {
        LoadScene(sceneLevel03);
    }
    private void OnClickedLevel04Button() {
        LoadScene(sceneLevel04);
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
