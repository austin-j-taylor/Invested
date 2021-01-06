using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the menu for selecting levels, sandboxes, etc.
/// </summary>
public class SceneSelectMenu : Menu {

    #region buildIndices
    // Scene build indices
    public const int sceneMain = 0,
        sceneTitleScreen = 1,
        sceneLuthadel = 2,
        sceneShootingGrounds = 3,
        sceneSandbox = 4,
        sceneSouthernMountains = 5,
        sceneSeaOfMetal = 6,
        sceneStorms = 7,
        sceneSimulationDuel = 8,
        sceneSimulationWall = 9,
        sceneSimulationGround = 10,
        sceneTutorial1 = 11,
        sceneTutorial2 = 12,
        sceneTutorial3 = 13,
        sceneTutorial4 = 14;
    #endregion

    public static bool IsTutorial(int sceneIndex) {
        return sceneIndex == sceneTutorial1 ||
                sceneIndex == sceneTutorial2 ||
                sceneIndex == sceneTutorial3 ||
                sceneIndex == sceneTutorial4;
    }
    public static bool IsCurrentSceneTutorial => IsTutorial(SceneManager.GetActiveScene().buildIndex);

    // Preserves the selected button to enter before entering that scene
    // so we have the same button selected when we leave the level
    public Button HighlitButton { get; private set; }

    #region fields
    private static SceneSelectMenu instance;

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

    private Button_SceneSelectMenu[] buttons;
    #endregion

    #region clearing
    void Awake() {
        instance = this;

        tooltip = transform.Find("Tooltip").GetComponent<Text>();
        tutorialsHeader = transform.Find("Tutorials").transform;
        levelsHeader = transform.Find("Levels").transform;
        sandboxesHeader = transform.Find("Sandboxes").transform;
        simulationsHeader = transform.Find("Simulations").transform;

        buttons = GetComponentsInChildren<Button_SceneSelectMenu>();

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

        HighlitButton = tutorial1Button;
    }
    private void Start() {
        Refresh();
        gameObject.SetActive(false);
    }

    public override void Open() {
        GameManager.MenusController.mainMenu.titleScreen.Close();
        base.Open();
        tooltip.text = "";
        MainMenu.FocusOnButton(HighlitButton);
        Refresh();
    }

    public override void Close() {
        base.Close();
        GameManager.MenusController.mainMenu.titleScreen.Open();
    }

    public void Refresh() {
        // Lock levels when previous levels are not completed
        //levelMARL1Button.interactable = FlagsController.GetData("completeTutorial1");
        //levelMARL2Button.interactable = FlagsController.GetData("completeTutorial2");
        //levelMARL3Button.interactable = FlagsController.GetData("completeTutorial3");
        //levelMARL4Button.interactable = FlagsController.GetData("completeTutorial4");
        tutorial1Button.interactable = true;
        tutorial2Button.interactable = FlagsController.instance.completeTutorial1;
        tutorial3Button.interactable = FlagsController.instance.completeTutorial2;
        tutorial4Button.interactable = FlagsController.instance.completeTutorial3;
        sandboxButton.interactable = true;
        shootingGroundsButton.interactable = FlagsController.instance.completeTutorial1;
        southernMountainsButton.interactable = FlagsController.instance.completeTutorial2;
        seaOfMetalButton.interactable = FlagsController.instance.completeTutorial3;
        stormsButton.interactable = FlagsController.instance.completeTutorial3;
        luthadelButtonDay.interactable = FlagsController.instance.completeTutorial4;
        luthadelButtonNight.interactable = FlagsController.instance.completeTutorial4;
        // Set the little "Completed" symbol next to each level to be enabled/disabled
        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].CheckCompleted();
        }
    }
    #endregion

    public void SetTooltip(string tip) {
        tooltip.text = tip;
    }

    public bool SceneIsMainOrTitleScreen(Scene scene) {
        return scene.buildIndex == sceneMain || scene.buildIndex == sceneTitleScreen;
    }

    #region OnClick
    private void LoadSceneFromClick(int scene) {
        HighlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        GameManager.SceneTransitionManager.LoadScene(scene);
    }
    private void OnClickedlevelMARL1Button() {
        //LoadSceneFromClick(sceneMARL1);
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
    private void OnClickedBack() => Close();
    #endregion
}
