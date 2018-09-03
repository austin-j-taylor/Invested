using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Enumerations for settings
// Gameplay
public enum ForceStyle { ForceMagnitude, Percentage }
public enum ControlScheme { MouseKeyboard45, MouseKeyboardQE, Gamepad }
// Interface
public enum ForceDisplayUnits { Newtons, Gs }
public enum InterfaceComplexity { Simple, Sums }
// Allomancy
public enum ForceDistanceRelationship { InverseSquareLaw, Linear, Exponential }
public enum AnchorBoostMode { AllomanticNormalForce, ExponentialWithVelocity, None }
public enum NormalForceMinimum { Zero, ZeroAndNegate, Disabled }
public enum NormalForceMaximum { AllomanticForce, Disabled }
public enum ExponentialWithVelocitySignage { AllVelocityDecreasesForce, OnlyBackwardsDecreasesForce, BackwardsDecreasesAndForwardsIncreasesForce }
public enum ExponentialWithVelocityRelativity { Relative, Absolute }


public class SettingsMenu : MonoBehaviour {

    // String constants for button text
    private const string s_settings = "Settings";
    private const string s_glossary = "Glossary";
    private const string s_gameplay = "Gameplay";
    private const string s_interface = "Interface";
    private const string s_allomancy = "Allomancy Physics";
    private const string s_world = "World Physics";
    // Gameplay
    private const string s_mk45 = "Mouse and Keyboard\n(MB 4 & 5)";
    private const string s_mkQE = "Mouse and Keyboard\n(Keys Q & E)";
    private const string s_game = "Gamepad";
    private const string s_gameDetails = "Disconnect and reconnect gamepad if not working.";
    private const string s_disabled = "Disabled";
    private const string s_enabled = "Enabled";
    private const string s_forc = "Control Force Magntitude";
    private const string s_forcDetails = "Player sets a target force magnitude. Pushes will always try to have that magnitude.";
    private const string s_perc = "Control Force Percentage";
    private const string s_percDetails = "Player sets a percentage of their maximum possible force to Push with.";
    private const string s_perspectiveFirstPerson = "First Person";
    private const string s_perspectiveThirdPerson = "Third Person";
    private const string s_clampingClamped = "Clamped";
    private const string s_clampingClampedDetails = "Camera cannot be rotated beyond 90° from the horizontal. Acts like any other game.";
    private const string s_clampingUnclamped = "Unclamped";
    private const string s_clampingUnclampedDetails = "Camera can be rotated without limits and wraps vertically around the player.";

    // Interface
    private const string s_blueLinesDetails = "Disable if you are having performance/framerate issues.";
    private const string s_newt = "Newtons";
    private const string s_newtDetails = "Pushes will be expressed as forces in units of Newtons.";
    private const string s_grav = "G's";
    private const string s_gravDetails = "Pushes will be expressed as accelerations in units of G's.";
    private const string s_interfaceSimple = "Only Net";
    private const string s_interfaceSimpleDetails = "HUD will only display net forces on the player and targets.";
    private const string s_interfaceSums = "Sums";
    private const string s_interfaceSumsDetails = "HUD will display net forces as well as each individual Allomantic Force and Anchor Push Boost.";
    private const string s_targetForcesEnabled = "Enabled";
    private const string s_targetForcesEnabledDetails = "Targets will display force(s) acting on them.";
    private const string s_targetForcesDisabled = "Disabled";
    private const string s_targetForcesDisabledDetails = "Targets will not display force(s) acting on them.";
    private const string s_targetMassesEnabled = "Enabled";
    private const string s_targetMassesEnabledDetails = "Highlighted metals will display their mass.";
    private const string s_targetMassesDisabled = "Disabled";
    private const string s_targetMassesDisabledDetails = "Highlighted metals will not display their mass.";

    // Allomancy
    private const string s_norm = "Allomantic Normal Force\n(ANF)";
    private const string s_normDetails = "Push on an anchored coin. The coin Pushes on the ground. The ground Pushes back on the coin. That force with which the ground resists your Push is returned to you.";
    private const string s_expV = "Exponential with Velocity\n(EWV)";
    private const string s_expVDetails = "AF ∝ e ^ -v/V where v = velocity of Allomancor or target; V = Exponential Constant";
    private const string s_anchorDisabled = "Disabled";
    private const string s_anchorDisabledDetails = "Anchors will provide no stronger Pushes than unanchored targets.";
    private const string s_aNFMinZero = "Zero";
    private const string s_aNFMinZeroDetails = "ANF will never be negative relative to the AF. Realistic behavior.";
    private const string s_aNFMinZeroNegate = "Zero & Negate";
    private const string s_aNFMinZeroNegateDetails = "ANF cannot be negative, but values that would be negative have their sign swapped and improve your Push instead. Unrealistic behavior.";
    private const string s_aNFMinDisabled = "Disabled";
    private const string s_aNFMinDisabledDetails = "ANF can be negative. Unrealistic behavior. You can Push but actually move towards your target, if your target resists you really well.";
    private const string s_aNFMaxAF = "Allomantic Force";
    private const string s_aNFMaxAFDetails = "ANF will never be higher than the AF. Realistic behavior. You cannot be resisted harder than you can Push.";
    private const string s_aNFMaxDisabled = "Disabled";
    private const string s_aNFMaxDisabledDetails = "ANF is uncapped. You can be resisted more than you can Push. Sometimes unrealistic behavior.";
    private const string s_aNFEqualityEqual = "Equal";
    private const string s_aNFEqualityEqualDetails = "The ANF on the target and Allomancer will be equal for both, calculated from whichever is more anchored.";
    private const string s_aNFEqualityUnequal = "Unequal";
    private const string s_aNFEqualityUnequalDetails = "The ANFs for the target and Allomancer will be calculated independently, depending on how each is individually anchored.";
    private const string s_eWVAlwaysDecreasing = "No Changes";
    private const string s_eWVAlwaysDecreasingDetails = "The higher the speed of the target, the weaker the Push. Period. Realistic behavior.";
    private const string s_eWVOnlyBackwardsDecreases = "Only When Moving Away";
    private const string s_eWVOnlyBackwardsDecreasesDetails = "If a target is moving away from you, the force is weaker. If a target is moving towards you, the force is unaffected.";
    private const string s_eWVChangesWithSign = "Symmetrical";
    private const string s_eWVChangesWithSignDetails = "The faster a target is moving away from you, the weaker the Push. The faster a targed is moving towards you, the stronger the Push.";
    private const string s_eWVRelative = "Relative";
    private const string s_eWVRelativeDetails = "v = Relative velocity of the target and Allomancer. The net forces on the target and Allomancer are equal.";
    private const string s_eWVAbsolute = "Absolute";
    private const string s_eWVAbsoluteDetails = "v = Absolute velocities of the target and Allomancer. The net forces on the target and Allomancer will be unequal and dependent on their individual velocities.";
    private const string s_inverse = "Inverse Square";
    private const string s_inverseDetails = "AF ∝ 1 / d² where d = distance between Allomancer and target";
    private const string s_linear = "Linear";
    private const string s_linearDetails = "AF ∝ 1 - d / R where d = distance between Allomancer and target; R = maximum range of Push";
    private const string s_eWD = "Exponential with Distance";
    private const string s_eWDDetails = "AF ∝ e ^ -d/D where d = distance between Allomancer and target; D = Exponential Constant";
    private const string s_alloConstantDetails = "All Pushes are proportional to this constant.";
    private const string s_maxRDetails = "Nearby metals can be detected within this range. Only impactful with \"Linear\" Force-Distance Relationship. ";

    // Variables for settings - TBDeleted
    public static bool UsingMB45 { get; set; } = true;
    public static ControlScheme currentControlScheme = ControlScheme.MouseKeyboard45;
    public static ForceStyle currentForceStyle = ForceStyle.Percentage;
    public static ForceDisplayUnits displayUnits = ForceDisplayUnits.Newtons;
    public static InterfaceComplexity interfaceComplexity = InterfaceComplexity.Simple;
    public static bool interfaceTargetForces = true;
    public static bool interfaceTargetMasses = false;
    public static bool renderBlueLines = true;

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

    // Gameplay
    private ButtonSetting controlScheme;
    private ButtonSetting rumble;
    private ButtonSetting pushControlStyle;
    private ButtonSetting perspective;
    private ButtonSetting clamping;
    private SliderSetting sensX;
    private SliderSetting sensY;

    private Button controlSchemeButton;
    private Text controlSchemeButtonText;
    private Text controlSchemeDetails;
    private Text rumbleLabel;
    private Button rumbleButton;
    private Text rumbleButtonText;
    private Button pushControlStyleButton;
    private Text pushControlStyleButtonText;
    private Text pushControlStyleDetails;
    private Button perspectiveButton;
    private Text perspectiveButtonText;
    private Button clampingButton;
    private Text clampingButtonText;
    private Text clampingDetails;
    private Slider sensitivityX;
    private Text sensitivityXValueText;
    private Slider sensitivityY;
    private Text sensitivityYValueText;

    // Interface
    private ButtonSetting renderblueLines;
    private ButtonSetting forceUnits;
    private ButtonSetting complexity;
    private ButtonSetting forceLabels;
    private ButtonSetting massLabels;

    private Text blueLinesButtonText;
    private Button blueLinesButton;
    private Text blueLinesDetails;
    private Button forceUnitsButton;
    private Text forceUnitsButtonText;
    private Text forceUnitsDetails;
    private Button interfaceComplexityButton;
    private Text interfaceComplexityButtonText;
    private Text interfaceComplexityDetails;
    private Button targetForcesButton;
    private Text targetForcesButtonText;
    private Text targetForcesDetails;
    private Button targetMassesButton;
    private Text targetMassesButtonText;
    private Text targetMassesDetails;

    // Allomancy
    private ButtonSetting anchoredBoost;
    private ButtonSetting normalMinimum;
    private ButtonSetting normalMaximum;
    private ButtonSetting normalEquality;
    private ButtonSetting expvelSignage;
    private ButtonSetting expvelRelativity;
    private SliderSetting expvelConstant;
    private ButtonSetting distanceRelationship;
    private SliderSetting distanceConstant;
    private SliderSetting allomanticConstant;
    private SliderSetting maxRange;

    private Button closeButton;
    private Button anchoredBoostButton;
    private Text anchoredBoostButtonText;
    private Text anchoredBoostDetails;
    private Button aNFMinimumButton;
    private Text aNFMinimumLabel;
    private Text aNFMinimumButtonText;
    private Text aNFMinimumDetails;
    private Text aNFMaximumLabel;
    private Button aNFMaximumButton;
    private Text aNFMaximumButtonText;
    private Text aNFMaximumDetails;
    private Text aNFEqualityLabel;
    private Button aNFEqualityButton;
    private Text aNFEqualityButtonText;
    private Text aNFEqualityDetails;
    private Text eWVSignageLabel;
    private Button eWVSignageButton;
    private Text eWVSignageButtonText;
    private Text eWVSignageDetails;
    private Text eWVRelativityLabel;
    private Button eWVRelativityButton;
    private Text eWVRelativityButtonText;
    private Text eWVRelativityDetails;
    private Text velocityConstantLabel;
    private Slider velocityConstantSlider;
    private Text velocityConstantValueText;
    private Button distanceRelationshipButton;
    private Text distanceRelationshipButtonText;
    private Text distanceRelationshipDetails;
    private Text distanceConstantLabel;
    private Slider distanceConstantSlider;
    private Text distanceConstantValueText;
    private Slider forceConstantSlider;
    private Text forceConstantValueText;
    private Text forceConstantDetails;
    private Slider maxRangeSlider;
    private Text maxRangeValueText;
    private Text maxRangeDetails;

    // World
    private ButtonSetting gravity;
    private ButtonSetting drag;

    private Button gravityButton;
    private Text gravityButtonText;
    private Button dragButton;
    private Text dragButtonText;

    private Setting[] settings;

    public static SettingsData settingsData;
    private Rigidbody playerRb;

    void Awake() {
        settings = GetComponentsInChildren<Setting>();
        settingsData = gameObject.AddComponent<SettingsData>();
        playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

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

        controlSchemeButton = gameplayHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        controlSchemeButtonText = controlSchemeButton.GetComponentInChildren<Text>();
        controlSchemeDetails = controlSchemeButton.transform.GetChild(1).GetComponent<Text>();

        rumbleLabel = gameplayHeader.GetChild(1).GetComponent<Text>();
        rumbleButton = rumbleLabel.GetComponentInChildren<Button>();
        rumbleButtonText = rumbleButton.GetComponentInChildren<Text>();

        pushControlStyleButton = gameplayHeader.GetChild(2).GetChild(0).GetComponent<Button>();
        pushControlStyleButtonText = pushControlStyleButton.transform.GetChild(0).GetComponent<Text>();
        pushControlStyleDetails = pushControlStyleButton.transform.GetChild(1).GetComponent<Text>();

        perspectiveButton = gameplayHeader.GetChild(3).GetChild(0).GetComponent<Button>();
        perspectiveButtonText = perspectiveButton.GetComponentInChildren<Text>();

        clampingButton = gameplayHeader.GetChild(4).GetChild(0).GetComponent<Button>();
        clampingButtonText = clampingButton.GetComponentInChildren<Text>();
        clampingDetails = clampingButton.transform.GetChild(1).GetComponent<Text>();

        sensitivityX = gameplayHeader.GetChild(5).GetComponentInChildren<Slider>();
        sensitivityXValueText = sensitivityX.GetComponentInChildren<Text>();

        sensitivityY = gameplayHeader.GetChild(6).GetComponentInChildren<Slider>();
        sensitivityYValueText = sensitivityY.GetComponentInChildren<Text>();

        // Interface Header
        interfaceHeader = transform.GetChild(4);

        blueLinesButton = interfaceHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        blueLinesButtonText = blueLinesButton.GetComponentInChildren<Text>();
        blueLinesDetails = blueLinesButton.transform.GetChild(1).GetComponent<Text>();

        forceUnitsButton = interfaceHeader.GetChild(1).GetChild(0).GetComponent<Button>();
        forceUnitsButtonText = forceUnitsButton.GetComponentInChildren<Text>();
        forceUnitsDetails = forceUnitsButton.transform.GetChild(1).GetComponent<Text>();

        interfaceComplexityButton = interfaceHeader.GetChild(2).GetChild(0).GetComponent<Button>();
        interfaceComplexityButtonText = interfaceComplexityButton.GetComponentInChildren<Text>();
        interfaceComplexityDetails = interfaceComplexityButton.transform.GetChild(1).GetComponent<Text>();

        targetForcesButton = interfaceHeader.GetChild(4).GetChild(0).GetComponent<Button>();
        targetForcesButtonText = targetForcesButton.GetComponentInChildren<Text>();
        targetForcesDetails = targetForcesButton.transform.GetChild(1).GetComponent<Text>();

        targetMassesButton = interfaceHeader.GetChild(5).GetChild(0).GetComponent<Button>();
        targetMassesButtonText = targetMassesButton.GetComponentInChildren<Text>();
        targetMassesDetails = targetMassesButton.transform.GetChild(1).GetComponent<Text>();

        // Allomancy Header
        allomancyHeader = transform.GetChild(5);

        anchoredBoostButton = allomancyHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        anchoredBoostButtonText = anchoredBoostButton.transform.GetChild(0).GetComponent<Text>();
        anchoredBoostDetails = anchoredBoostButton.transform.GetChild(1).GetComponent<Text>();

        aNFMinimumLabel = allomancyHeader.GetChild(1).GetComponent<Text>();
        aNFMinimumButton = aNFMinimumLabel.GetComponentInChildren<Button>();
        aNFMinimumButtonText = aNFMinimumButton.transform.GetChild(0).GetComponent<Text>();
        aNFMinimumDetails = aNFMinimumButton.transform.GetChild(1).GetComponent<Text>();

        aNFMaximumLabel = allomancyHeader.GetChild(2).GetComponent<Text>();
        aNFMaximumButton = aNFMaximumLabel.GetComponentInChildren<Button>();
        aNFMaximumButtonText = aNFMaximumButton.transform.GetChild(0).GetComponent<Text>();
        aNFMaximumDetails = aNFMaximumButton.transform.GetChild(1).GetComponent<Text>();

        aNFEqualityLabel = allomancyHeader.GetChild(3).GetComponent<Text>();
        aNFEqualityButton = aNFEqualityLabel.GetComponentInChildren<Button>();
        aNFEqualityButtonText = aNFEqualityButton.transform.GetChild(0).GetComponent<Text>();
        aNFEqualityDetails = aNFEqualityButton.transform.GetChild(1).GetComponent<Text>();

        eWVSignageLabel = allomancyHeader.GetChild(4).GetComponent<Text>();
        eWVSignageButton = eWVSignageLabel.GetComponentInChildren<Button>();
        eWVSignageButtonText = eWVSignageButton.transform.GetChild(0).GetComponent<Text>();
        eWVSignageDetails = eWVSignageButton.transform.GetChild(1).GetComponent<Text>();

        eWVRelativityLabel = allomancyHeader.GetChild(5).GetComponent<Text>();
        eWVRelativityButton = eWVRelativityLabel.GetComponentInChildren<Button>();
        eWVRelativityButtonText = eWVRelativityButton.transform.GetChild(0).GetComponent<Text>();
        eWVRelativityDetails = eWVRelativityButton.transform.GetChild(1).GetComponent<Text>();

        velocityConstantLabel = allomancyHeader.GetChild(6).GetComponent<Text>();
        velocityConstantSlider = velocityConstantLabel.GetComponentInChildren<Slider>();
        velocityConstantValueText = velocityConstantSlider.GetComponentInChildren<Text>();

        distanceRelationshipButton = allomancyHeader.GetChild(7).GetChild(0).GetComponent<Button>();
        distanceRelationshipButtonText = distanceRelationshipButton.GetComponentInChildren<Text>();
        distanceRelationshipDetails = distanceRelationshipButton.transform.GetChild(1).GetComponent<Text>();

        distanceConstantLabel = allomancyHeader.GetChild(8).GetComponent<Text>();
        distanceConstantSlider = distanceConstantLabel.GetComponentInChildren<Slider>();
        distanceConstantValueText = distanceConstantSlider.GetComponentInChildren<Text>();

        forceConstantSlider = allomancyHeader.GetChild(9).GetComponentInChildren<Slider>();
        forceConstantValueText = forceConstantSlider.GetComponentInChildren<Text>();
        forceConstantDetails = forceConstantSlider.transform.GetChild(4).GetComponent<Text>();

        maxRangeSlider = allomancyHeader.GetChild(10).GetComponentInChildren<Slider>();
        maxRangeValueText = maxRangeSlider.GetComponentInChildren<Text>();
        maxRangeDetails = maxRangeSlider.transform.GetChild(4).GetComponent<Text>();

        // World Header
        worldHeader = transform.GetChild(6);
        gravityButton = worldHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        gravityButtonText = gravityButton.transform.GetChild(0).GetComponent<Text>();

        dragButton = worldHeader.GetChild(1).GetChild(0).GetComponent<Button>();
        dragButtonText = dragButton.transform.GetChild(0).GetComponent<Text>();

        // Close Button
        closeButton = transform.GetChild(7).GetComponent<Button>();


        // Command listeners assignment
        // Settings
        glossaryButton.onClick.AddListener(OpenGlossary);
        gameplayButton.onClick.AddListener(OpenGameplay);
        interfaceButton.onClick.AddListener(OpenInterface);
        allomancyButton.onClick.AddListener(OpenAllomancy);
        worldButton.onClick.AddListener(OpenWorld);
        closeButton.onClick.AddListener(OnClickClose);
        // Gameplay
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        pushControlStyleButton.onClick.AddListener(OnClickPushControlStyle);
        perspectiveButton.onClick.AddListener(OnClickPerspectiveButton);
        clampingButton.onClick.AddListener(OnClickClampingButton);
        sensitivityX.onValueChanged.AddListener(OnSensitivityXChanged);
        sensitivityY.onValueChanged.AddListener(OnSensitivityYChanged);
        // Interface
        blueLinesButton.onClick.AddListener(OnBlueLinesButton);
        forceUnitsButton.onClick.AddListener(OnClickForceUnits);
        interfaceComplexityButton.onClick.AddListener(OnClickInterfaceComplexity);
        targetForcesButton.onClick.AddListener(OnClickTargetForces);
        targetMassesButton.onClick.AddListener(OnClickTargetMasses);
        // Allomancy
        anchoredBoostButton.onClick.AddListener(OnClickAnchoredBoost);
        aNFMinimumButton.onClick.AddListener(OnClickANFMinimum);
        aNFMaximumButton.onClick.AddListener(OnClickANFMaximum);
        aNFEqualityButton.onClick.AddListener(OnClickANFEquality);
        eWVSignageButton.onClick.AddListener(OnClickEWVSignage);
        eWVRelativityButton.onClick.AddListener(OnClickEWVRelativity);
        velocityConstantSlider.onValueChanged.AddListener(OnVelocityConstantChanged);
        distanceRelationshipButton.onClick.AddListener(OnClickDistanceRelationshipButton);
        distanceConstantSlider.onValueChanged.AddListener(OnDistanceConstantChanged);
        forceConstantSlider.onValueChanged.AddListener(OnForceConstantChanged);
        maxRangeSlider.onValueChanged.AddListener(OnMaxRangeChanged);
        // World
        gravityButton.onClick.AddListener(OnClickGravityButton);
        dragButton.onClick.AddListener(OnClickDragButton);

        // Initial field assignments
        titleText.text = s_settings;
        // Gameplay
        controlSchemeDetails.text = "";
        controlSchemeButtonText.text = s_mk45;
        rumbleButtonText.text = s_enabled;
        pushControlStyleButtonText.text = s_perc;
        pushControlStyleDetails.text = s_percDetails;
        perspectiveButtonText.text = s_perspectiveThirdPerson;
        clampingButtonText.text = s_clampingClamped;
        clampingDetails.text = s_clampingClampedDetails;
        sensitivityX.value = CameraController.SensitivityX;
        sensitivityY.value = CameraController.SensitivityY;
        // Interface
        blueLinesButtonText.text = s_enabled;
        blueLinesDetails.text = s_blueLinesDetails;
        forceUnitsButtonText.text = s_newt;
        forceUnitsDetails.text = s_newtDetails;
        interfaceComplexityButtonText.text = s_interfaceSimple;
        interfaceComplexityDetails.text = s_interfaceSimpleDetails;
        targetForcesButtonText.text = s_targetForcesEnabled;
        targetForcesDetails.text = s_targetForcesEnabledDetails;
        targetMassesButtonText.text = s_targetMassesDisabled;
        targetMassesDetails.text = s_targetMassesDisabledDetails;
        // Allomancy
        anchoredBoostButtonText.text = s_norm;
        anchoredBoostDetails.text = s_normDetails;
        aNFMinimumButtonText.text = s_aNFMinZero;
        aNFMinimumDetails.text = s_aNFMinZeroDetails;
        aNFMaximumButtonText.text = s_aNFMaxAF;
        aNFMaximumDetails.text = s_aNFMaxAFDetails;
        aNFEqualityButtonText.text = s_aNFEqualityEqual;
        aNFEqualityDetails.text = s_aNFEqualityEqualDetails;
        eWVSignageButtonText.text = s_eWVAlwaysDecreasing;
        eWVSignageDetails.text = s_eWVAlwaysDecreasingDetails;
        eWVRelativityButtonText.text = s_eWVRelative;
        eWVRelativityDetails.text = s_eWVRelativeDetails;
        distanceRelationshipButtonText.text = s_eWD;
        distanceRelationshipDetails.text = s_eWDDetails;
        forceConstantDetails.text = s_alloConstantDetails;
        distanceConstantSlider.value = PhysicsController.distanceConstant;
        velocityConstantSlider.value = PhysicsController.velocityConstant;
        forceConstantSlider.value = AllomanticIronSteel.AllomanticConstant;
        maxRangeSlider.value = AllomanticIronSteel.MaxRange;
        // World
        maxRangeDetails.text = s_maxRDetails;
        gravityButtonText.text = s_enabled;
        dragButtonText.text = s_enabled;


        sensitivityXValueText.text = sensitivityX.value.ToString();
        sensitivityYValueText.text = sensitivityY.value.ToString();
        distanceConstantValueText.text = distanceConstantSlider.value.ToString();
        velocityConstantValueText.text = velocityConstantSlider.value.ToString();
        forceConstantValueText.text = forceConstantSlider.value.ToString();
        maxRangeValueText.text = maxRangeSlider.value.ToString();


        // Now, set up the scene to start with only the Title Screen visible
        settingsHeader.gameObject.SetActive(false);
        glossaryHeader.gameObject.SetActive(false);
        gameplayHeader.gameObject.SetActive(false);
        interfaceHeader.gameObject.SetActive(false);
        allomancyHeader.gameObject.SetActive(false);
        worldHeader.gameObject.SetActive(false);
        rumbleLabel.gameObject.SetActive(false);
        eWVRelativityLabel.gameObject.SetActive(false);
        eWVSignageLabel.gameObject.SetActive(false);
        velocityConstantLabel.gameObject.SetActive(false);
    }

    private void Start() {
        // Refresh all settings after they've been loaded
        //settingsData.LoadSettings();
        foreach (Setting setting in settings) {
            setting.RefreshData();
            setting.RefreshText();
        }
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

    // On Button Click methods

    private void OnClickClose() {
        BackSettings();
    }

    // Gameplay

    private void OnClickControlScheme() {
        switch (currentControlScheme) {
            case ControlScheme.MouseKeyboard45: {
                    currentControlScheme = ControlScheme.MouseKeyboardQE;
                    controlSchemeButtonText.text = s_mkQE;
                    UsingMB45 = false;
                    break;
                }
            case ControlScheme.MouseKeyboardQE: {
                    currentControlScheme = ControlScheme.Gamepad;
                    controlSchemeButtonText.text = s_game;
                    controlSchemeDetails.text = s_gameDetails;
                    GamepadController.UsingGamepad = true;
                    rumbleLabel.gameObject.SetActive(true);
                    CameraController.SensitivityX *= 2;
                    CameraController.SensitivityY *= 2;
                    sensitivityX.value *= 2;
                    sensitivityY.value *= 2;
                    break;
                }
            default: {
                    currentControlScheme = ControlScheme.MouseKeyboard45;
                    controlSchemeButtonText.text = s_mk45;
                    controlSchemeDetails.text = "";
                    UsingMB45 = true;
                    GamepadController.UsingGamepad = false;
                    rumbleLabel.gameObject.SetActive(false);
                    CameraController.SensitivityX /= 2;
                    CameraController.SensitivityY /= 2;
                    sensitivityX.value /= 2;
                    sensitivityY.value /= 2;
                    break;
                }
        }
    }

    private void OnClickRumble() {
        if (GamepadController.UsingRumble) {
            rumbleButtonText.text = s_disabled;
            GamepadController.UsingRumble = false;
        } else {
            rumbleButtonText.text = s_enabled;
            GamepadController.UsingRumble = true;
        }
    }

    private void OnClickPushControlStyle() {
        switch (currentForceStyle) {
            case ForceStyle.ForceMagnitude: {
                    pushControlStyleButtonText.text = s_perc;
                    pushControlStyleDetails.text = s_percDetails;
                    currentForceStyle = ForceStyle.Percentage;
                    break;
                }
            default: {
                    pushControlStyleButtonText.text = s_forc;
                    pushControlStyleDetails.text = s_forcDetails;
                    currentForceStyle = ForceStyle.ForceMagnitude;
                    break;
                }
        }
    }

    private void OnClickPerspectiveButton() {
        if (CameraController.FirstPerson) {
            CameraController.FirstPerson = false;
            perspectiveButtonText.text = s_perspectiveThirdPerson;
        } else {
            CameraController.FirstPerson = true;
            perspectiveButtonText.text = s_perspectiveFirstPerson;
        }
    }

    private void OnClickClampingButton() {
        if (CameraController.ClampCamera) {
            CameraController.ClampCamera = false;
            clampingButtonText.text = s_clampingUnclamped;
            clampingDetails.text = s_clampingUnclampedDetails;
        } else {
            CameraController.ClampCamera = true;
            clampingButtonText.text = s_clampingClamped;
            clampingDetails.text = s_clampingClampedDetails;
        }
    }

    private void OnSensitivityXChanged(float value) {
        CameraController.SensitivityX = value;
        sensitivityXValueText.text = ((int)(100 * value) / 100f).ToString();
    }

    private void OnSensitivityYChanged(float value) {
        CameraController.SensitivityY = value;
        sensitivityYValueText.text = ((int)(100 * value) / 100f).ToString();
    }

    // Interface

    private void OnBlueLinesButton() {
        if (renderBlueLines) {
            blueLinesButtonText.text = s_disabled;
            renderBlueLines = false;
        } else {
            blueLinesButtonText.text = s_enabled;
            renderBlueLines = true;
        }
    }

    private void OnClickForceUnits() {
        switch (displayUnits) {
            case ForceDisplayUnits.Newtons: {
                    forceUnitsButtonText.text = s_grav;
                    forceUnitsDetails.text = s_gravDetails;
                    displayUnits = ForceDisplayUnits.Gs;
                    break;
                }
            default: {
                    forceUnitsButtonText.text = s_newt;
                    forceUnitsDetails.text = s_newtDetails;
                    displayUnits = ForceDisplayUnits.Newtons;
                    break;
                }
        }
    }

    private void OnClickInterfaceComplexity() {
        switch (interfaceComplexity) {
            case InterfaceComplexity.Simple: {
                    interfaceComplexityButtonText.text = s_interfaceSums;
                    interfaceComplexityDetails.text = s_interfaceSumsDetails;
                    interfaceComplexity = InterfaceComplexity.Sums;
                    break;
                }
            default: {
                    interfaceComplexityButtonText.text = s_interfaceSimple;
                    interfaceComplexityDetails.text = s_interfaceSimpleDetails;
                    interfaceComplexity = InterfaceComplexity.Simple;
                    HUD.BurnRateMeter.InterfaceRefresh();
                    HUD.TargetOverlayController.InterfaceRefresh();
                    break;
                }
        }
    }

    private void OnClickTargetForces() {
        if (interfaceTargetForces) {
            targetForcesButtonText.text = s_targetForcesDisabled;
            targetForcesDetails.text = s_targetForcesDisabledDetails;
            interfaceTargetForces = false;
            HUD.TargetOverlayController.InterfaceRefresh();
        } else {
            targetForcesButtonText.text = s_targetForcesEnabled;
            targetForcesDetails.text = s_targetForcesEnabledDetails;
            interfaceTargetForces = true;
        }
    }

    private void OnClickTargetMasses() {
        if (interfaceTargetMasses) {
            targetMassesButtonText.text = s_targetMassesDisabled;
            targetMassesDetails.text = s_targetMassesDisabledDetails;
            interfaceTargetMasses = false;
            HUD.TargetOverlayController.InterfaceRefresh();
        } else {
            targetMassesButtonText.text = s_targetMassesEnabled;
            targetMassesDetails.text = s_targetMassesEnabledDetails;
            interfaceTargetMasses = true;
        }
    }

    // Allomancy

    private void OnClickAnchoredBoost() {
        switch (PhysicsController.anchorBoostMode) {
            case AnchorBoostMode.AllomanticNormalForce: {
                    aNFMaximumLabel.gameObject.SetActive(false);
                    aNFMinimumLabel.gameObject.SetActive(false);
                    aNFEqualityLabel.gameObject.SetActive(false);
                    eWVSignageLabel.gameObject.SetActive(true);
                    eWVRelativityLabel.gameObject.SetActive(true);
                    velocityConstantLabel.gameObject.SetActive(true);

                    PhysicsController.anchorBoostMode = AnchorBoostMode.ExponentialWithVelocity;
                    anchoredBoostButtonText.text = s_expV;
                    anchoredBoostDetails.text = s_expVDetails;
                    AllomanticIronSteel.AllomanticConstant *= 2;
                    forceConstantSlider.value *= 2;
                    break;
                }
            case AnchorBoostMode.ExponentialWithVelocity: {
                    eWVSignageLabel.gameObject.SetActive(false);
                    eWVRelativityLabel.gameObject.SetActive(false);
                    velocityConstantLabel.gameObject.SetActive(false);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.None;
                    anchoredBoostButtonText.text = s_anchorDisabled;
                    anchoredBoostDetails.text = s_anchorDisabledDetails;
                    break;
                }
            default: {
                    aNFMaximumLabel.gameObject.SetActive(true);
                    aNFMinimumLabel.gameObject.SetActive(true);
                    aNFEqualityLabel.gameObject.SetActive(true);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
                    anchoredBoostButtonText.text = s_norm;
                    anchoredBoostDetails.text = s_normDetails;
                    AllomanticIronSteel.AllomanticConstant /= 2;
                    forceConstantSlider.value /= 2;
                    break;
                }
        }
    }

    private void OnClickANFMinimum() {
        switch (PhysicsController.normalForceMinimum) {
            case NormalForceMinimum.Zero: {
                    PhysicsController.normalForceMinimum = NormalForceMinimum.ZeroAndNegate;
                    aNFMinimumButtonText.text = s_aNFMinZeroNegate;
                    aNFMinimumDetails.text = s_aNFMinZeroNegateDetails;
                    break;
                }
            case NormalForceMinimum.ZeroAndNegate: {
                    PhysicsController.normalForceMinimum = NormalForceMinimum.Disabled;
                    aNFMinimumButtonText.text = s_aNFMinDisabled;
                    aNFMinimumDetails.text = s_aNFMinDisabledDetails;
                    break;
                }
            default: { // Disabled
                    PhysicsController.normalForceMinimum = NormalForceMinimum.Zero;
                    aNFMinimumButtonText.text = s_aNFMinZero;
                    aNFMinimumDetails.text = s_aNFMinZeroDetails;
                    break;
                }
        }
    }

    private void OnClickANFMaximum() {
        switch (PhysicsController.normalForceMaximum) {
            case NormalForceMaximum.AllomanticForce: {
                    PhysicsController.normalForceMaximum = NormalForceMaximum.Disabled;
                    aNFMaximumButtonText.text = s_aNFMaxDisabled;
                    aNFMaximumDetails.text = s_aNFMaxDisabledDetails;
                    break;
                }
            default: { // Disabled
                    PhysicsController.normalForceMaximum = NormalForceMaximum.AllomanticForce;
                    aNFMaximumButtonText.text = s_aNFMaxAF;
                    aNFMaximumDetails.text = s_aNFMaxAFDetails;
                    break;
                }
        }
    }

    private void OnClickANFEquality() {
        if (PhysicsController.normalForceEquality) {
            aNFEqualityButtonText.text = s_aNFEqualityUnequal;
            aNFEqualityDetails.text = s_aNFEqualityUnequalDetails;
            PhysicsController.normalForceEquality = false;
        } else {
            aNFEqualityButtonText.text = s_aNFEqualityEqual;
            aNFEqualityDetails.text = s_aNFEqualityEqualDetails;
            PhysicsController.normalForceEquality = true;
        }
    }

    private void OnClickEWVSignage() {
        switch (PhysicsController.exponentialWithVelocitySignage) {
            case ExponentialWithVelocitySignage.AllVelocityDecreasesForce: {
                    PhysicsController.exponentialWithVelocitySignage = ExponentialWithVelocitySignage.BackwardsDecreasesAndForwardsIncreasesForce;
                    eWVSignageButtonText.text = s_eWVChangesWithSign;
                    eWVSignageDetails.text = s_eWVChangesWithSignDetails;
                    break;
                }
            case ExponentialWithVelocitySignage.BackwardsDecreasesAndForwardsIncreasesForce: {
                    PhysicsController.exponentialWithVelocitySignage = ExponentialWithVelocitySignage.OnlyBackwardsDecreasesForce;
                    eWVSignageButtonText.text = s_eWVOnlyBackwardsDecreases;
                    eWVSignageDetails.text = s_eWVOnlyBackwardsDecreasesDetails;
                    break;
                }
            default: { // Disabled
                    PhysicsController.exponentialWithVelocitySignage = ExponentialWithVelocitySignage.AllVelocityDecreasesForce;
                    eWVSignageButtonText.text = s_eWVAlwaysDecreasing;
                    eWVSignageDetails.text = s_eWVAlwaysDecreasingDetails;
                    break;
                }
        }
    }

    private void OnClickEWVRelativity() {
        switch (PhysicsController.exponentialWithVelocityRelativity) {
            case ExponentialWithVelocityRelativity.Absolute: {
                    PhysicsController.exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Relative;
                    eWVRelativityButtonText.text = s_eWVRelative;
                    eWVRelativityDetails.text = s_eWVRelativeDetails;
                    break;
                }
            default: { // Relative
                    PhysicsController.exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Absolute;
                    eWVRelativityButtonText.text = s_eWVAbsolute;
                    eWVRelativityDetails.text = s_eWVAbsoluteDetails;
                    break;
                }
        }
    }

    private void OnVelocityConstantChanged(float value) {
        PhysicsController.velocityConstant = ((int)value);
        velocityConstantValueText.text = ((int)value).ToString();
    }

    private void OnClickDistanceRelationshipButton() {
        switch (PhysicsController.distanceRelationshipMode) {
            case ForceDistanceRelationship.InverseSquareLaw: {
                    distanceRelationshipButtonText.text = s_linear;
                    distanceRelationshipDetails.text = s_linearDetails;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Linear;
                    forceConstantSlider.value /= 40f / 12f;
                    break;
                }
            case ForceDistanceRelationship.Linear: {
                    distanceConstantLabel.gameObject.SetActive(true);
                    distanceRelationshipButtonText.text = s_eWD;
                    distanceRelationshipDetails.text = s_eWDDetails;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Exponential;
                    break;
                }
            case ForceDistanceRelationship.Exponential: {
                    distanceConstantLabel.gameObject.SetActive(false);
                    distanceRelationshipButtonText.text = s_inverse;
                    distanceRelationshipDetails.text = s_inverseDetails;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.InverseSquareLaw;
                    forceConstantSlider.value *= 40f / 12f;
                    break;
                }
        }
    }

    private void OnDistanceConstantChanged(float value) {
        PhysicsController.distanceConstant = ((int)value);
        distanceConstantValueText.text = ((int)value).ToString();
    }

    private void OnForceConstantChanged(float value) {
        AllomanticIronSteel.AllomanticConstant = ((int)value);
        forceConstantValueText.text = ((int)value).ToString();
    }

    // World

    private void OnClickGravityButton() {
        if (PhysicsController.gravityEnabled) {
            playerRb.useGravity = false;
            gravityButtonText.text = s_disabled;
            PhysicsController.gravityEnabled = false;
        } else {
            playerRb.useGravity = true;
            gravityButtonText.text = s_enabled;
            PhysicsController.gravityEnabled = true;
        }
    }

    private void OnClickDragButton() {
        if (PhysicsController.airResistanceEnabled) {
            dragButtonText.text = s_disabled;
            PhysicsController.airResistanceEnabled = false;
        } else {
            dragButtonText.text = s_enabled;
            PhysicsController.airResistanceEnabled = true;
        }
    }

    private void OnMaxRangeChanged(float value) {
        AllomanticIronSteel.MaxRange = ((int)value);
        maxRangeValueText.text = ((int)value).ToString();
    }
}