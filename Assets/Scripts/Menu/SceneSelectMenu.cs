using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SceneSelectMenu : MonoBehaviour {

    // Scene build indices
    public const int sceneMain = 0,
        sceneTitleScreen = 1,
        sceneMARL1 = 2,
        sceneLuthadel = 3,
        sceneShootingGrounds = 4,
        sceneSandbox = 5,
        sceneSouthernMountains = 6,
        sceneSeaOfMetal = 7,
        sceneStorms = 8,
        sceneSimulationDuel = 9,
        sceneSimulationWall = 10,
        sceneSimulationGround = 11,
        sceneTutorial1 = 12,
        sceneTutorial2 = 13,
        sceneTutorial3 = 14,
        sceneTutorial4 = 15;

    public static bool IsTutorial(int sceneIndex) {
        return sceneIndex == sceneTutorial1 ||
                sceneIndex == sceneTutorial2 ||
                sceneIndex == sceneTutorial3 ||
                sceneIndex == sceneTutorial4;
    }

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    public static bool NowLoadingScene { get; private set; } = true;

    private static Button highlitButton;

    private Text tooltip;
    private Transform tutorialsHeader;
    private Transform levelsHeader;
    private Transform sandboxesHeader;
    private Transform simulationsHeader;
    private Button levelMARL1Button;
    private Button levelMARL2Button;
    private Button levelMARL3Button;
    private Button levelMARL4Button;
    private Button tutorial1Button;
    private Button tutorial2Button;
    private Button tutorial3Button;
    private Button tutorial4Button;
    private Button luthadelButtonDay;
    private Button luthadelButtonNight;
    private Button shootingGroundsButton;
    private Button sandboxButton;
    private Button southernMountainsButton;
    private Button seaOfMetalButton;
    private Button stormsButton;
    private Button simulationDuelButton;
    private Button simulationWallButton;
    private Button simulationGroundButton;
    private Button backButton;


    void Start() {
        tooltip = transform.Find("Tooltip").GetComponent<Text>();
        tutorialsHeader = transform.Find("Tutorials").transform;
        levelsHeader = transform.Find("Levels").transform;
        sandboxesHeader = transform.Find("Sandboxes").transform;
        simulationsHeader = transform.Find("Simulations").transform;
        
        levelMARL1Button = levelsHeader.Find("MARL1").GetComponent<Button>();
        levelMARL2Button = levelsHeader.Find("MARL2").GetComponent<Button>();
        levelMARL3Button = levelsHeader.Find("MARL3").GetComponent<Button>();
        levelMARL4Button = levelsHeader.Find("MARL4").GetComponent<Button>();
        tutorial1Button = tutorialsHeader.Find("Tutorial1").GetComponent<Button>();
        tutorial2Button = tutorialsHeader.Find("Tutorial2").GetComponent<Button>();
        tutorial3Button = tutorialsHeader.Find("Tutorial3").GetComponent<Button>();
        tutorial4Button = tutorialsHeader.Find("Tutorial4").GetComponent<Button>();
        luthadelButtonDay = sandboxesHeader.Find("LuthadelDay").GetComponent<Button>();
        luthadelButtonNight = sandboxesHeader.Find("LuthadelNight").GetComponent<Button>();
        shootingGroundsButton = sandboxesHeader.Find("ShootingGrounds").GetComponent<Button>();
        sandboxButton = sandboxesHeader.Find("Sandbox").GetComponent<Button>();
        seaOfMetalButton = sandboxesHeader.Find("SeaOfMetal").GetComponent<Button>();
        stormsButton = sandboxesHeader.Find("IlluminatingStorms").GetComponent<Button>();
        southernMountainsButton = sandboxesHeader.Find("SouthernMountains").GetComponent<Button>();
        simulationDuelButton = simulationsHeader.Find("Duel").GetComponent<Button>();
        simulationWallButton = simulationsHeader.Find("CoinWall").GetComponent<Button>();
        simulationGroundButton = simulationsHeader.Find("CoinGround").GetComponent<Button>();
        backButton = transform.Find("Back").GetComponent<Button>();

        levelMARL1Button.onClick.AddListener(OnClickedlevelMARL1Button);
        levelMARL2Button.onClick.AddListener(OnClickedlevelMARL2Button);
        levelMARL3Button.onClick.AddListener(OnClickedlevelMARL3Button);
        levelMARL4Button.onClick.AddListener(OnClickedlevelMARL4Button);
        tutorial1Button.onClick.AddListener(OnClickedTutorial1Button);
        tutorial2Button.onClick.AddListener(OnClickedTutorial2Button);
        tutorial3Button.onClick.AddListener(OnClickedTutorial3Button);
        tutorial4Button.onClick.AddListener(OnClickedTutorial4Button);
        luthadelButtonDay.onClick.AddListener(OnClickedLuthadelDay);
        luthadelButtonNight.onClick.AddListener(OnClickedLuthadelNight);
        shootingGroundsButton.onClick.AddListener(OnClickedShootingGrounds);
        sandboxButton.onClick.AddListener(OnClickedSandbox);
        southernMountainsButton.onClick.AddListener(OnClickedSouthernMountains);
        seaOfMetalButton.onClick.AddListener(OnClickedSeaOfMetal);
        stormsButton.onClick.AddListener(OnClickedStorms);
        simulationDuelButton.onClick.AddListener(OnClickedSimulationDuel);
        simulationWallButton.onClick.AddListener(OnClickedSimulationWall);
        simulationGroundButton.onClick.AddListener(OnClickedSimulationGround);
        backButton.onClick.AddListener(OnClickedBack);

        highlitButton = tutorial1Button;

        // Only close the main menu after the scene loads to prevent jarring camera transitions
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }

    public void Open() {
        gameObject.SetActive(true);
        tooltip.text = "";
        MainMenu.FocusOnButton(highlitButton);
        // Lock levels when previous levels are not completed
        levelMARL1Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial1);
        levelMARL2Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial2);
        levelMARL3Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial3);
        levelMARL4Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial4);
        tutorial1Button.interactable = true;
        tutorial2Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeMARL1);
        tutorial3Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeMARL2);
        tutorial4Button.interactable = FlagsController.GetLevel(FlagsController.Level.completeMARL3);
        sandboxButton.interactable = true;
        shootingGroundsButton.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial1);
        southernMountainsButton.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial2);
        seaOfMetalButton.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial3);
        stormsButton.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial3);
        luthadelButtonDay.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial4);
        luthadelButtonNight.interactable = FlagsController.GetLevel(FlagsController.Level.completeTutorial4);
    }

    public void Close() {
        gameObject.SetActive(false);
        MainMenu.OpenTitleScreen();
    }

    public static void LoadScene(int scene) {
        //CameraController.SetExternalSource(null, null);
        Player.PlayerInstance.transform.parent = EventSystem.current.transform;
        
        SceneManager.LoadScene(scene);
    }

    public static void ReloadScene() {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) {
            PauseMenu.UnPause();
            LevelCompletedScreen.Close();

            if (scene.buildIndex == sceneTitleScreen) {
                MainMenu.Open();

                Player.CanControl = false;
                Player.CanPause = false;
                Player.PlayerInstance.gameObject.SetActive(true);
                CameraController.UnlockCamera();
                CameraController.ActiveCamera.clearFlags = CameraClearFlags.Skybox;
                CameraController.Cinemachine.m_IgnoreTimeScale = true;
                //CameraController.ActiveCamera.clearFlags = CameraClearFlags.SolidColor;
                if (isActiveAndEnabled)
                    MainMenu.FocusOnButton(highlitButton);
            //} else if (IsTutorial(scene.buildIndex)) {
            //    // Tutorial levels have a special level transition from the title screen.
            //    MainMenu.Close();
            //    TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
            //    CameraController.LockCamera();
            //    CameraController.ActiveCamera.clearFlags = CameraClearFlags.Skybox;
            //    CameraController.Cinemachine.m_IgnoreTimeScale = false;
            //    Player.CanPause = true;
            //    HUD.ResetHUD();

            } else {
                MainMenu.Close();
                TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
                Player.PlayerInstance.gameObject.SetActive(true);
                CameraController.UsingCinemachine = false;
                CameraController.Cinemachine.m_IgnoreTimeScale = false;
                CameraController.LockCamera();
                CameraController.ActiveCamera.clearFlags = CameraClearFlags.Skybox;
                HUD.ResetHUD();

                // Set parameters for starting on certain scenes
                Player.PlayerInstance.CoinHand.Pouch.Fill();
                Player.PlayerIronSteel.IronReserve.SetMass(150);
                Player.PlayerIronSteel.SteelReserve.SetMass(150);
                Player.PlayerPewter.PewterReserve.SetMass(150);
                Player.PlayerIronSteel.IronReserve.IsEnabled = true;
                Player.PlayerIronSteel.SteelReserve.IsEnabled = true;
                Player.PlayerPewter.PewterReserve.IsEnabled = true;
            }
        }
        NowLoadingScene = false;
    }


    public void SetTooltip(string tip) {
        tooltip.text = tip;
    }

    private void LoadSceneFromClick(int scene) {
        highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        LoadScene(scene);
    }
    private void OnClickedlevelMARL1Button() {
        LoadSceneFromClick(sceneMARL1);
    }
    private void OnClickedlevelMARL2Button() {
        //LoadSceneFromClick(sceneMARL2);
    }
    private void OnClickedlevelMARL3Button() {
        //LoadSceneFromClick(sceneMARL3);
    }
    private void OnClickedlevelMARL4Button() {
        //LoadSceneFromClick(sceneMARL4);
    }
    private void OnClickedTutorial1Button() {
        LoadSceneFromClick(sceneTutorial1);
    }
    private void OnClickedTutorial2Button() {
        LoadSceneFromClick(sceneTutorial2);
    }
    private void OnClickedTutorial3Button() {
        LoadSceneFromClick(sceneTutorial3);
    }
    private void OnClickedTutorial4Button() {
        LoadSceneFromClick(sceneTutorial4);
    }

    private void OnClickedLuthadelDay() {
        Environment_Luthadel.DayMode = true;
        LoadSceneFromClick(sceneLuthadel);
    }
    private void OnClickedLuthadelNight() {
        Environment_Luthadel.DayMode = false;
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
    private void OnClickedStorms() {
        LoadSceneFromClick(sceneStorms);
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

    public bool SceneIsMainOrTitleScreen(Scene scene) {
        return scene.buildIndex == sceneMain || scene.buildIndex == sceneTitleScreen;
    }
}
