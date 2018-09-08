using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
    
    // String constants
    private const string s_settings = "Settings";
    private const string s_glossary = "Glossary";
    private const string s_gameplay = "Gameplay";
    private const string s_interface = "Interface";
    private const string s_allomancy = "Allomancy Physics";
    private const string s_world = "World Physics";

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
    private Button allomancyButton;
    private Button worldButton;
    private Transform glossaryHeader;
    private Transform gameplayHeader;
    private Transform interfaceHeader;
    private Transform allomancyHeader;
    private Transform worldHeader;
    private Button closeButton;
    
    public static SettingsData settingsData;

    void Awake() {
        settingsData = gameObject.AddComponent<SettingsData>();

        // Settings Header
        titleText = transform.GetChild(0).GetComponent<Text>();
        settingsHeader = transform.GetChild(1);
        Button[] settingsHeaderButtons = settingsHeader.GetComponentsInChildren<Button>();
        glossaryButton = settingsHeaderButtons[0];
        gameplayButton = settingsHeaderButtons[1];
        interfaceButton = settingsHeaderButtons[2];
        allomancyButton = settingsHeaderButtons[3];
        worldButton = settingsHeaderButtons[4];
        // Glossary
        glossaryHeader = transform.GetChild(2);
        // Gameplay Header
        gameplayHeader = transform.GetChild(3);
        // Interface Header
        interfaceHeader = transform.GetChild(4);
        // Allomancy Header
        allomancyHeader = transform.GetChild(5);
        // World Header
        worldHeader = transform.GetChild(6);
        // Close Button
        closeButton = transform.GetChild(7).GetComponent<Button>();

        // Command listeners assignment
        glossaryButton.onClick.AddListener(OpenGlossary);
        gameplayButton.onClick.AddListener(OpenGameplay);
        interfaceButton.onClick.AddListener(OpenInterface);
        allomancyButton.onClick.AddListener(OpenAllomancy);
        worldButton.onClick.AddListener(OpenWorld);
        closeButton.onClick.AddListener(OnClickClose);
        // Initial field assignments
        titleText.text = s_settings;

    }

    private void Start() {
        // Refresh all settings after they've been loaded
        foreach (Setting setting in GetComponentsInChildren<Setting>()) {
            setting.RefreshData();
            setting.RefreshText();
        }

        // Now, set up the scene to start with only the Title Screen visible
        settingsHeader.gameObject.SetActive(false);
        glossaryHeader.gameObject.SetActive(false);
        gameplayHeader.gameObject.SetActive(false);
        interfaceHeader.gameObject.SetActive(false);
        allomancyHeader.gameObject.SetActive(false);
        worldHeader.gameObject.SetActive(false);
    }

    public void OpenSettings() {
        gameObject.SetActive(true);
    }

    public void CloseSettings() {
        CloseGlossary();
        CloseInterface();
        CloseGameplay();
        CloseAllomancy();
        CloseWorld();
        gameObject.SetActive(false);
    }

    private void OpenGlossary() {
        titleText.text = s_glossary;
        settingsHeader.gameObject.SetActive(false);
        glossaryHeader.gameObject.SetActive(true);
    }

    private void CloseGlossary() {
        titleText.text = s_settings;
        settingsHeader.gameObject.SetActive(true);
        glossaryHeader.gameObject.SetActive(false);
    }

    private void OpenGameplay() {
        titleText.text = s_gameplay;
        settingsHeader.gameObject.SetActive(false);
        gameplayHeader.gameObject.SetActive(true);
    }

    private void CloseGameplay() {
        titleText.text = s_settings;
        settingsHeader.gameObject.SetActive(true);
        gameplayHeader.gameObject.SetActive(false);
    }

    private void OpenInterface() {
        titleText.text = s_interface;
        settingsHeader.gameObject.SetActive(false);
        interfaceHeader.gameObject.SetActive(true);
    }

    private void CloseInterface() {
        titleText.text = s_settings;
        settingsHeader.gameObject.SetActive(true);
        interfaceHeader.gameObject.SetActive(false);
    }

    private void OpenAllomancy() {
        titleText.text = s_allomancy;
        settingsHeader.gameObject.SetActive(false);
        allomancyHeader.gameObject.SetActive(true);
    }

    private void CloseAllomancy() {
        titleText.text = s_settings;
        settingsHeader.gameObject.SetActive(true);
        allomancyHeader.gameObject.SetActive(false);
    }

    private void OpenWorld() {
        titleText.text = s_world;
        settingsHeader.gameObject.SetActive(false);
        worldHeader.gameObject.SetActive(true);
    }

    private void CloseWorld() {
        titleText.text = s_settings;
        settingsHeader.gameObject.SetActive(true);
        worldHeader.gameObject.SetActive(false);
    }

    public void BackSettings() {

        if (IsGlossaryOpen)
            CloseGlossary();
        else if (IsGameplayOpen)
            CloseGameplay();
        else if (IsInterfaceOpen)
            CloseInterface();
        else if (IsAllomancyOpen)
            CloseAllomancy();
        else if (IsWorldOpen)
            CloseWorld();
        else {
            // And save settings
            settingsData.SaveSettings();
            CloseSettings();
        }
    }

    private void OnClickClose() {
        BackSettings();
    }
}