using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
    
    // String constants
    private const string s_settings = "Settings";
    private const string s_glossary = "Glossary";
    private const string s_gameplay = "Gameplay";
    private const string s_interface = "Interface";
    private const string s_graphics = "Graphics";
    private const string s_allomancy = "Allomancy Physics";
    private const string s_world = "World Physics";
    private const string s_back = "Back";
    private const string s_save = "Save & Back";
    private const string s_reset = "Reset to Defaults";
    private const string s_reset_confirmed = "Settings reset.";

    // Previously-used string constants for buttons, labels, details, etc.
    //// Gameplay
    //private const string s_mk45 = "Mouse and Keyboard\n(MB 4 & 5)";
    //private const string s_mkQE = "Mouse and Keyboard\n(Keys Q & E)";
    //private const string s_game = "Gamepad";
    //private const string s_gameDetails = "Disconnect and reconnect gamepad if not working.";
    //private const string s_disabled = "Disabled";
    //private const string s_enabled = "Enabled";
    //private const string s_forc = "Control Force Magntitude";
    //private const string s_forcDetails = "Player sets a target force magnitude. Pushes will always try to have that magnitude.";
    //private const string s_perc = "Control Force Percentage";
    //private const string s_percDetails = "Player sets a percentage of their maximum possible force to Push with.";
    //private const string s_perspectiveFirstPerson = "First Person";
    //private const string s_perspectiveThirdPerson = "Third Person";
    //private const string s_clampingClamped = "Clamped";
    //private const string s_clampingClampedDetails = "Camera cannot be rotated beyond 90° from the horizontal. Acts like any other game.";
    //private const string s_clampingUnclamped = "Unclamped";
    //private const string s_clampingUnclampedDetails = "Camera can be rotated without limits and wraps vertically around the player.";

    //// Interface
    //private const string s_blueLinesDetails = "Disable if you are having performance/framerate issues.";
    //private const string s_newt = "Newtons";
    //private const string s_newtDetails = "Pushes will be expressed as forces in units of Newtons.";
    //private const string s_grav = "G's";
    //private const string s_gravDetails = "Pushes will be expressed as accelerations in units of G's.";
    //private const string s_interfaceSimple = "Only Net";
    //private const string s_interfaceSimpleDetails = "HUD will only display net forces on the player and targets.";
    //private const string s_interfaceSums = "Sums";
    //private const string s_interfaceSumsDetails = "HUD will display net forces as well as each individual Allomantic Force and Anchor Push Boost.";
    //private const string s_targetForcesEnabled = "Enabled";
    //private const string s_targetForcesEnabledDetails = "Targets will display force(s) acting on them.";
    //private const string s_targetForcesDisabled = "Disabled";
    //private const string s_targetForcesDisabledDetails = "Targets will not display force(s) acting on them.";
    //private const string s_targetMassesEnabled = "Enabled";
    //private const string s_targetMassesEnabledDetails = "Hovered-over metals will display their mass.";
    //private const string s_targetMassesDisabled = "Disabled";
    //private const string s_targetMassesDisabledDetails = "Hovered-over metals will not display their mass.";

    //// Allomancy
    //private const string s_norm = "Allomantic Normal Force\n(ANF)";
    //private const string s_normDetails = "Push on an anchored coin. The coin Pushes on the ground. The ground Pushes back on the coin. That force with which the ground resists your Push is returned to you.";
    //private const string s_expV = "Exponential with Velocity\n(EWV)";
    //private const string s_expVDetails = "AF ∝ e ^ -v/V where v = velocity of Allomancor or target; V = Exponential Constant";
    //private const string s_anchorDisabled = "Disabled";
    //private const string s_anchorDisabledDetails = "Anchors will provide no stronger Pushes than unanchored targets.";
    //private const string s_aNFMinZero = "Zero";
    //private const string s_aNFMinZeroDetails = "ANF will never be negative relative to the AF. Realistic behavior.";
    //private const string s_aNFMinZeroNegate = "Zero & Negate";
    //private const string s_aNFMinZeroNegateDetails = "ANF cannot be negative, but values that would be negative have their sign swapped and improve your Push instead. Unrealistic behavior.";
    //private const string s_aNFMinDisabled = "Disabled";
    //private const string s_aNFMinDisabledDetails = "ANF can be negative. You can Push but actually move towards your target, if your target resists you really well. Unrealistic behavior.";
    //private const string s_aNFMaxAF = "Allomantic Force";
    //private const string s_aNFMaxAFDetails = "ANF will never be higher than the AF. Realistic behavior. You cannot be resisted harder than you can Push.";
    //private const string s_aNFMaxDisabled = "Disabled";
    //private const string s_aNFMaxDisabledDetails = "ANF is uncapped. You can be resisted more than you can Push. Sometimes unrealistic behavior.";
    //private const string s_aNFEqualityEqual = "Equal";
    //private const string s_aNFEqualityEqualDetails = "The ANF on the target and Allomancer will be equal for both, calculated from whichever is more anchored.";
    //private const string s_aNFEqualityUnequal = "Unequal";
    //private const string s_aNFEqualityUnequalDetails = "The ANFs for the target and Allomancer will be calculated independently, depending on how each is individually anchored.";
    //private const string s_eWVAlwaysDecreasing = "No Changes";
    //private const string s_eWVAlwaysDecreasingDetails = "The higher the speed of the target, the weaker the Push. Period. Realistic behavior.";
    //private const string s_eWVOnlyBackwardsDecreases = "Only When Moving Away";
    //private const string s_eWVOnlyBackwardsDecreasesDetails = "If a target is moving away from you, the force is weaker. If a target is moving towards you, the force is unaffected.";
    //private const string s_eWVChangesWithSign = "Symmetrical";
    //private const string s_eWVChangesWithSignDetails = "The faster a target is moving away from you, the weaker the Push. The faster a targed is moving towards you, the stronger the Push.";
    //private const string s_eWVRelative = "Relative";
    //private const string s_eWVRelativeDetails = "v = Relative velocity of the target and Allomancer. The net forces on the target and Allomancer are equal.";
    //private const string s_eWVAbsolute = "Absolute";
    //private const string s_eWVAbsoluteDetails = "v = Absolute velocities of the target and Allomancer. The net forces on the target and Allomancer will be unequal and dependent on their individual velocities.";
    //private const string s_inverse = "Inverse Square";
    //private const string s_inverseDetails = "AF ∝ 1 / d² where d = distance between Allomancer and target";
    //private const string s_linear = "Linear";
    //private const string s_linearDetails = "AF ∝ 1 - d / R where d = distance between Allomancer and target; R = maximum range of Push";
    //private const string s_eWD = "Exponential with Distance";
    //private const string s_eWDDetails = "AF ∝ e ^ -d/D where d = distance between Allomancer and target; D = Exponential Constant";
    //private const string s_alloConstantDetails = "All Pushes are proportional to this constant.";
    //private const string s_maxRDetails = "Nearby metals can be detected within this range. Only impactful with \"Linear\" Force-Distance Relationship.";

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }
    public bool AreHeadersClosed {
        get {
            return settingsHeader.gameObject.activeSelf;
        }
    }
    public bool IsGlossaryOpen {
        get {
            return glossaryHeader.gameObject.activeSelf;
        }
    }
    public bool IsGameplayOpen {
        get {
            return gameplayHeader.gameObject.activeSelf;
        }
    }
    public bool IsInterfaceOpen {
        get {
            return interfaceHeader.gameObject.activeSelf;
        }
    }
    public bool IsGraphicsOpen {
        get {
            return graphicsHeader.gameObject.activeSelf;
        }
    }
    public bool IsAllomancyOpen {
        get {
            return allomancyHeader.gameObject.activeSelf;
        }
    }
    public bool IsWorldOpen {
        get {
            return worldHeader.gameObject.activeSelf;
        }
    }

    // Settings
    private Text titleText;
    private Transform settingsHeader;
    private Button glossaryButton;
    private Button gameplayButton;
    private Button interfaceButton;
    private Button graphicsButton;
    private Button allomancyButton;
    private Button worldButton;
    private Transform glossaryHeader;
    private Transform gameplayHeader;
    private Transform interfaceHeader;
    private Transform graphicsHeader;
    private Transform allomancyHeader;
    private Transform worldHeader;
    private Button closeButton;
    private Text closeText;
    private Button discardButton;
    private Button resetToDefaultsButton;
    private Text resetToDefaultsText;

    private Setting[] settings;

    public static SettingsData settingsData;

    void Awake() {
        settings = GetComponentsInChildren<Setting>();
        settingsData = gameObject.AddComponent<SettingsData>();

        // Settings Header
        titleText = transform.GetChild(1).GetComponent<Text>();
        settingsHeader = transform.GetChild(2);
        Button[] settingsHeaderButtons = settingsHeader.GetComponentsInChildren<Button>();
        glossaryButton = settingsHeaderButtons[0];
        gameplayButton = settingsHeaderButtons[1];
        interfaceButton = settingsHeaderButtons[2];
        graphicsButton = settingsHeaderButtons[3];
        allomancyButton = settingsHeaderButtons[4];
        worldButton = settingsHeaderButtons[5];
        // Glossary
        glossaryHeader = transform.GetChild(3);
        // Gameplay Header
        gameplayHeader = transform.GetChild(4);
        // Interface Header
        interfaceHeader = transform.GetChild(5);
        // Graphics Header
        graphicsHeader = transform.GetChild(6);
        // Allomancy Header
        allomancyHeader = transform.GetChild(7);
        // World Header
        worldHeader = transform.GetChild(8);
        // Close Button
        closeButton = transform.GetChild(9).GetComponent<Button>();

        closeText = closeButton.GetComponentInChildren<Text>();
        discardButton = transform.GetChild(10).GetComponent<Button>();
        resetToDefaultsButton = transform.GetChild(11).GetComponent<Button>();
        resetToDefaultsText = resetToDefaultsButton.GetComponentInChildren<Text>();

        // Command listeners assignment
        glossaryButton.onClick.AddListener(OpenGlossary);
        gameplayButton.onClick.AddListener(OpenGameplay);
        interfaceButton.onClick.AddListener(OpenInterface);
        graphicsButton.onClick.AddListener(OpenGraphics);
        allomancyButton.onClick.AddListener(OpenAllomancy);
        worldButton.onClick.AddListener(OpenWorld);
        closeButton.onClick.AddListener(OnClickClose);
        discardButton.onClick.AddListener(OnClickDiscard);
        resetToDefaultsButton.onClick.AddListener(OnClickResetToDefaults);
        // Initial field assignments
        titleText.text = s_settings;
        closeText.text = s_back;
        resetToDefaultsText.text = s_reset;
    }

    private void Start() {
        // Refresh all settings after they've been loaded
        RefreshSettings();

        discardButton.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(false);
    }

    private void RefreshSettings() {
        foreach (Setting setting in settings) {
            setting.RefreshData();
            setting.RefreshText();
        }
    }

    public void OpenSettings() {
        gameObject.SetActive(true);
        resetToDefaultsButton.gameObject.SetActive(true);
    }

    public void CloseSettings() {
        resetToDefaultsText.text = s_reset;
        resetToDefaultsButton.gameObject.SetActive(false);
        CloseGlossary();
        CloseInterface();
        CloseGraphics();
        CloseGameplay();
        CloseAllomancy();
        CloseWorld();
        gameObject.SetActive(false);
    }

    private void OpenHeader() {
        resetToDefaultsText.text = s_reset;
        settingsHeader.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(true);
        resetToDefaultsButton.gameObject.SetActive(false);
        closeText.text = s_save;
    }

    private void CloseHeader() {
        settingsHeader.gameObject.SetActive(true);
        discardButton.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(true);
        closeText.text = s_back;
    }

    private void OpenGlossary() {
        titleText.text = s_glossary;
        glossaryHeader.gameObject.SetActive(true);
        settingsHeader.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(false);
    }

    private void CloseGlossary() {
        titleText.text = s_settings;
        glossaryHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenGameplay() {
        titleText.text = s_gameplay;
        gameplayHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseGameplay() {
        titleText.text = s_settings;
        gameplayHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenInterface() {
        titleText.text = s_interface;
        interfaceHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseInterface() {
        titleText.text = s_settings;
        interfaceHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenGraphics() {
        titleText.text = s_graphics;
        graphicsHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseGraphics() {
        titleText.text = s_settings;
        graphicsHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenAllomancy() {
        titleText.text = s_allomancy;
        allomancyHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseAllomancy() {
        titleText.text = s_settings;
        allomancyHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenWorld() {
        titleText.text = s_world;
        worldHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseWorld() {
        titleText.text = s_settings;
        worldHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    public void BackAndSaveSettings() {
        if (!AreHeadersClosed)
            settingsData.SaveSettings();
        BackSettings();
    }

    private void BackSettings() {
        if (IsGlossaryOpen)
            CloseGlossary();
        else if (IsGameplayOpen)
            CloseGameplay();
        else if (IsInterfaceOpen)
            CloseInterface();
        else if (IsGraphicsOpen)
            CloseGraphics();
        else if (IsAllomancyOpen)
            CloseAllomancy();
        else if (IsWorldOpen)
            CloseWorld();
        else {
            CloseSettings();
        }
    }

    private void OnClickClose() {
        BackAndSaveSettings();
    }

    private void OnClickDiscard() {
        settingsData.LoadSettings();
        RefreshSettings();
        BackSettings();
    }

    private void OnClickResetToDefaults() {
        resetToDefaultsText.text = s_reset_confirmed;
        settingsData.ResetToDefaults();
        RefreshSettings();
    }
}