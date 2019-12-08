using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneSelectMenu : MonoBehaviour {

    // Scene build indices
    public const int sceneMain = 0;
    public const int sceneTitleScreen = 1;
    public const int sceneLevel01 = 2;
    public const int sceneLuthadel = 3;
    public const int sceneShootingGrounds = 4;
    public const int sceneSandbox = 5;
    public const int sceneSouthernMountains = 6;
    public const int sceneSeaOfMetal = 7;
    public const int sceneSimulationDuel = 8;
    public const int sceneSimulationWall = 9;
    public const int sceneSimulationGround = 10;
    public const int sceneLevel02 = 11;
    public const int sceneLevel03 = 12;
    public const int sceneLevel04 = 13;

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private static Button highlitButton;

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
    private Button seaOfMetalButton;
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
        seaOfMetalButton = sandboxesHeader.Find("SeaOfMetal").GetComponent<Button>();
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
        seaOfMetalButton.onClick.AddListener(OnClickedSeaOfMetal);
        simulationDuelButton.onClick.AddListener(OnClickedSimulationDuel);
        simulationWallButton.onClick.AddListener(OnClickedSimulationWall);
        simulationGroundButton.onClick.AddListener(OnClickedSimulationGround);
        backButton.onClick.AddListener(OnClickedBack);

        highlitButton = level01Button;

        // Only close the main menu after the scene loads to prevent jarring camera transitions
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    public void Open() {
        gameObject.SetActive(true);
        tooltip.text = "";
        MainMenu.FocusOnButton(highlitButton);
        // Lock levels when previous levels are not completed
        if (!FlagsController.GetLevel(FlagsController.Level.Complete03)) {
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
        Player.PlayerInstance.transform.parent = EventSystem.current.transform;
        
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
                    MainMenu.FocusOnButton(highlitButton);
            } else {
                MainMenu.Close();
                TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;

                Player.PlayerInstance.gameObject.SetActive(true);
                CameraController.LockCamera();
                HUD.ResetHUD();

                // Set parameters for starting on certain scenes
                Player.PlayerInstance.CoinHand.Pouch.Fill();
                Player.PlayerIronSteel.IronReserve.SetMass(150);
                Player.PlayerIronSteel.SteelReserve.SetMass(150);
                Player.PlayerPewter.PewterReserve.SetMass(150);
            }
        }
    }

    public void SetTooltip(string tip) {
        tooltip.text = tip;
    }

    private void LoadSceneFromClick(int scene) {
        highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        LoadScene(scene);
    }
    private void OnClickedLevel01Button() {
        LoadSceneFromClick(sceneLevel01);
    }
    private void OnClickedLevel02Button() {
        LoadSceneFromClick(sceneLevel02);
    }
    private void OnClickedLevel03Button() {
        LoadSceneFromClick(sceneLevel03);
    }
    private void OnClickedLevel04Button() {
        LoadSceneFromClick(sceneLevel04);
    }
    private void OnClickedLuthadel() {
        LoadSceneFromClick(sceneLuthadel);
    }
    private void OnClickedShootingGrounds() {
        LoadSceneFromClick(sceneShootingGrounds);
    }
    private void OnClickedSandbox() {
        LoadSceneFromClick(sceneSandbox);
    }
    private void OnClickedSouthernMountains() {
        LoadSceneFromClick(sceneSouthernMountains);
    }
    private void OnClickedSimulationDuel() {
        LoadSceneFromClick(sceneSimulationDuel);
    }
    private void OnClickedSeaOfMetal() {
        LoadSceneFromClick(sceneSeaOfMetal);
    }
    private void OnClickedSimulationWall() {
        LoadSceneFromClick(sceneSimulationWall);
    }
    private void OnClickedSimulationGround() {
        LoadSceneFromClick(sceneSimulationGround);
    }

    private void OnClickedBack() {
        Close();
    }
}
