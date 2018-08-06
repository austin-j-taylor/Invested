using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    // String constants for button text
    private const string mk45 = "Mouse and Keyboard\n(MB 4 & 5)";
    private const string mkQE = "Mouse and Keyboard\n(Keys Q & E)";
    private const string game = "Gamepad";
    private const string gameDetails = "Disconnect and reconnect gamepad if not working.";
    private const string disa = "Disabled";
    private const string enab = "Enabled";
    private const string forc = "Control Force Magntitude";
    private const string forcDetails = "Player sets a target force magnitude. Pushes will always try to have that magnitude.";
    private const string perc = "Control Force Percentage";
    private const string percDetails = "Player sets a percentage of their maximum possible force to push with.";
    private const string newt = "Newtons";
    private const string newtDetails = "Pushes/pulls will be expressed as forces in units of Newtons.";
    private const string grav = "G's";
    private const string gravDetails = "Pushes/pulls will be expressed as accelerations in units of G's.";
    private const string norm = "Allomantic Normal Force\n(ANF)";
    private const string normDetails = "Push on an anchored coin. The coin pushes on the ground. The ground pushes back on the coin. That force with which the ground resists your push is returned to you.";
    private const string expV = "Exponential with Velocity\n(EWV)";
    private const string expVDetails = "AF ∝ e ^ -v/V where v = velocity of Allomancor or target; V = Exponential Constant";
    private const string anchorDisabled = "Disabled";
    private const string anchorDisabledDetails = "Anchors will provide no stronger push than unanchored targets.";
    private const string aNFMinZero = "Zero";
    private const string aNFMinZeroDetails = "ANF will never be negative relative to the AF. Realistic behavior.";
    private const string aNFMinZeroNegate = "Zero & Negate";
    private const string aNFMinZeroNegateDetails = "ANF cannot be negative, but values that would be negative have their sign swapped and improve your push instead. Unrealistic behavior.";
    private const string aNFMinDisabled = "Disabled";
    private const string aNFMinDisabledDetails = "ANF can be negative. Unrealistic behavior. You can push but actually move towards your target, if your target resists you really well.";
    private const string aNFMaxAF = "Allomantic Force";
    private const string aNFMaxAFDetails = "ANF will never be higher than the AF. Realistic behavior. You cannot be resisted harder than you can push.";
    private const string aNFMaxDisabled = "Disabled";
    private const string aNFMaxDisabledDetails = "ANF is uncapped. You can be resisted more than you can push. Extremely glitchy and causes feedback loops. Bad idea.";
    private const string eWVAlwaysDecreasing = "No Changes";
    private const string eWVAlwaysDecreasingDetails = "The higher the speed of the target, the weaker the push or pull. Period. Realistic behavior.";
    private const string eWVOnlyBackwardsDecreases = "Only When Moving Away";
    private const string eWVOnlyBackwardsDecreasesDetails = "If a target is moving away from you, the force is weaker. If a target is moving towards you, the force is unaffected.";
    private const string eWVChangesWithSign = "Symmetrical";
    private const string eWVChangesWithSignDetails = "The faster a target is moving away from you, the weaker the push/pull. The faster a targed is moving towards you, the stronger the push/pull.";
    private const string eWVRelative = "Relative";
    private const string eWVRelativeDetails = "v = Relative velocity of the target and Allomancer. The net forces on the target and Allomancer are equal.";
    private const string eWVAbsolute = "Absolute";
    private const string eWVAbsoluteDetails = "v = Absolute velocities of the target and Allomancer. The net forces on the target and Allomancer will be unequal and dependent on their individual velocities.";
    private const string inverse = "Inverse Square";
    private const string inverseDetails = "AF ∝ 1 / d² where d = distance between Allomancer and target";
    private const string linear = "Linear";
    private const string linearDetails = "AF ∝ 1 - d / R where d = distance between Allomancer and target; R = maximum range of push";
    private const string eWD = "Exponential with Distance";
    private const string eWDDetails = "AF ∝ e ^ -d/D where d = distance between Allomancer and target; D = Exponential Constant";
    private const string alloConstantDetails = "All pushes/pulls are proportional to this constant.";
    private const string maxRDetails = "Nearby metals can be detected within this range.";

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }
    public bool IsGameplayOpen {
        get {
            return gameplayHeader.gameObject.activeSelf;
        }
    }
    public bool IsPhysicsOpen {
        get {
            return physicsHeader.gameObject.activeSelf;
        }
    }

    private Button gameplayButton;
    private Button physicsButton;
    private Transform gameplayHeader;
    private Transform physicsHeader;

    private Button controlSchemeButton;
    private Text controlSchemeButtonText;
    private Text controlSchemeDetails;
    private Text rumbleLabel;
    private Button rumbleButton;
    private Text rumbleButtonText;
    private Button pushControlStyleButton;
    private Text pushControlStyleButtonText;
    private Text pushControlStyleDetails;
    private Slider sensitivity;
    private Text sensitivityValueText;
    private Slider smoothing;
    private Text smoothingValueText;
    private Button forceUnitsButton;
    private Text forceUnitsButtonText;
    private Text forceUnitsDetails;

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



    void Start() {
        
        // Settings Header
        Button[] settingsHeaderButtons = transform.GetChild(1).GetComponentsInChildren<Button>();
        gameplayButton = settingsHeaderButtons[0];
        physicsButton = settingsHeaderButtons[1];
        closeButton = settingsHeaderButtons[2];

        // Gameplay Header
        gameplayHeader = transform.GetChild(2);
        controlSchemeButton = gameplayHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        controlSchemeButtonText = controlSchemeButton.GetComponentInChildren<Text>();
        controlSchemeDetails = controlSchemeButton.transform.GetChild(1).GetComponent<Text>();

        rumbleLabel = gameplayHeader.GetChild(1).GetComponent<Text>();
        rumbleButton = rumbleLabel.GetComponentInChildren<Button>();
        rumbleButtonText = rumbleButton.GetComponentInChildren<Text>();

        pushControlStyleButton = gameplayHeader.GetChild(2).GetChild(0).GetComponent<Button>();
        pushControlStyleButtonText = pushControlStyleButton.transform.GetChild(0).GetComponent<Text>();
        pushControlStyleDetails = pushControlStyleButton.transform.GetChild(1).GetComponent<Text>();

        forceUnitsButton = gameplayHeader.GetChild(3).GetChild(0).GetComponent<Button>();
        forceUnitsButtonText = forceUnitsButton.GetComponentInChildren<Text>();
        forceUnitsDetails = forceUnitsButton.transform.GetChild(1).GetComponent<Text>();

        sensitivity = gameplayHeader.GetChild(4).GetComponentInChildren<Slider>();
        sensitivityValueText = sensitivity.GetComponentInChildren<Text>();

        smoothing = gameplayHeader.GetChild(5).GetComponentInChildren<Slider>();
        smoothingValueText = smoothing.GetComponentInChildren<Text>();

        // Physics Header
        physicsHeader = transform.GetChild(3);
        anchoredBoostButton = physicsHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        anchoredBoostButtonText = anchoredBoostButton.transform.GetChild(0).GetComponent<Text>();
        anchoredBoostDetails = anchoredBoostButton.transform.GetChild(1).GetComponent<Text>();

        aNFMinimumLabel = physicsHeader.GetChild(1).GetComponent<Text>();
        aNFMinimumButton = aNFMinimumLabel.GetComponentInChildren<Button>();
        aNFMinimumButtonText = aNFMinimumButton.transform.GetChild(0).GetComponent<Text>();
        aNFMinimumDetails = aNFMinimumButton.transform.GetChild(1).GetComponent<Text>();

        aNFMaximumLabel = physicsHeader.GetChild(2).GetComponent<Text>();
        aNFMaximumButton = aNFMaximumLabel.GetComponentInChildren<Button>();
        aNFMaximumButtonText = aNFMaximumButton.transform.GetChild(0).GetComponent<Text>();
        aNFMaximumDetails = aNFMaximumButton.transform.GetChild(1).GetComponent<Text>();

        eWVSignageLabel = physicsHeader.GetChild(3).GetComponent<Text>();
        eWVSignageButton = eWVSignageLabel.GetComponentInChildren<Button>();
        eWVSignageButtonText = eWVSignageButton.transform.GetChild(0).GetComponent<Text>();
        eWVSignageDetails = eWVSignageButton.transform.GetChild(1).GetComponent<Text>();

        eWVRelativityLabel = physicsHeader.GetChild(4).GetComponent<Text>();
        eWVRelativityButton = eWVRelativityLabel.GetComponentInChildren<Button>();
        eWVRelativityButtonText = eWVRelativityButton.transform.GetChild(0).GetComponent<Text>();
        eWVRelativityDetails = eWVRelativityButton.transform.GetChild(1).GetComponent<Text>();

        velocityConstantLabel = physicsHeader.GetChild(5).GetComponent<Text>();
        velocityConstantSlider = velocityConstantLabel.GetComponentInChildren<Slider>();
        velocityConstantValueText = velocityConstantSlider.GetComponentInChildren<Text>();

        distanceRelationshipButton = physicsHeader.GetChild(6).GetChild(0).GetComponent<Button>();
        distanceRelationshipButtonText = distanceRelationshipButton.GetComponentInChildren<Text>();
        distanceRelationshipDetails = distanceRelationshipButton.transform.GetChild(1).GetComponent<Text>();

        distanceConstantLabel = physicsHeader.GetChild(7).GetComponent<Text>();
        distanceConstantSlider = distanceConstantLabel.GetComponentInChildren<Slider>();
        distanceConstantValueText = distanceConstantSlider.GetComponentInChildren<Text>();

        forceConstantSlider = physicsHeader.GetChild(8).GetComponentInChildren<Slider>();
        forceConstantValueText = forceConstantSlider.GetComponentInChildren<Text>();
        forceConstantDetails = forceConstantSlider.transform.GetChild(4).GetComponent<Text>();
        maxRangeSlider = physicsHeader.GetChild(9).GetComponentInChildren<Slider>();
        maxRangeValueText = maxRangeSlider.GetComponentInChildren<Text>();
        maxRangeDetails = maxRangeSlider.transform.GetChild(4).GetComponent<Text>();



        // old hardcoded indexing
        //buttons = GetComponentsInChildren<Button>();

        ////gameplayButton = buttons[0];
        ////physicsButton = buttons[1];
        ////closeButton = buttons[2];
        //controlSchemeButton = buttons[3];
        //rumbleButton = buttons[4];
        //pushControlStyleButton = buttons[5];
        //anchoredBoostButton = buttons[6];
        //distanceRelationshipButton = buttons[7];
        //forceUnitsButton = buttons[8];

        //sensitivity = sliders[0];
        //smoothing = sliders[1];
        //distanceConstantSlider = sliders[2];
        //velocityConstantSlider = sliders[3];
        //forceConstantSlider = sliders[4];
        //maxRangeSlider = sliders[5];

        //gameplayHeader = texts[5];
        //physicsHeader = texts[16];
        //rumbleLabel = texts[8];
        //distanceConstantValueText = texts[21];
        //velocityConstantValueText = texts[23];

        //controlSchemeButtonText = controlSchemeButton.GetComponentInChildren<Text>();
        //rumbleButtonText = rumbleButton.GetComponentInChildren<Text>();
        //anchoredBoostButtonText = anchoredBoostButton.GetComponentInChildren<Text>();
        //pushControlStyleButtonText = pushControlStyleButton.GetComponentInChildren<Text>();
        //distanceRelationshipButtonText = distanceRelationshipButton.GetComponentInChildren<Text>();
        //forceUnitsButtonText = forceUnitsButton.GetComponentInChildren<Text>();

        //sensitivityValueText = sensitivity.GetComponentInChildren<Text>();
        //smoothingValueText = smoothing.GetComponentInChildren<Text>();
        //distanceConstantLabel = distanceConstantSlider.GetComponentInChildren<Text>();
        //velocityConstantLabel = velocityConstantSlider.GetComponentInChildren<Text>();
        //forceConstantValueText = forceConstantSlider.GetComponentInChildren<Text>();
        //maxRangeValueText = maxRangeSlider.GetComponentInChildren<Text>();
        pushControlStyleButton.onClick.AddListener(OnClickPushControlStyle);
        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        smoothing.onValueChanged.AddListener(OnSmoothingChanged);
        distanceConstantSlider.onValueChanged.AddListener(OnDistanceConstantChanged);
        velocityConstantSlider.onValueChanged.AddListener(OnVelocityConstantChanged);
        forceConstantSlider.onValueChanged.AddListener(OnForceConstantChanged);
        maxRangeSlider.onValueChanged.AddListener(OnMaxRangeChanged);

        anchoredBoostButton.onClick.AddListener(OnClickAnchoredBoost);
        aNFMinimumButton.onClick.AddListener(OnClickANFMinimum);
        aNFMaximumButton.onClick.AddListener(OnClickANFMaximum);
        eWVSignageButton.onClick.AddListener(OnClickEWVSignage);
        eWVRelativityButton.onClick.AddListener(OnClickEWVRelativity);

        distanceRelationshipButton.onClick.AddListener(OnClickDistanceRelationshipButton);
        forceUnitsButton.onClick.AddListener(OnClickForceUnitsButton);

        gameplayButton.onClick.AddListener(OpenGameplay);
        physicsButton.onClick.AddListener(OpenPhysics);
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        closeButton.onClick.AddListener(OnClickClose);

        controlSchemeDetails.text = "";
        controlSchemeButtonText.text = mk45;
        pushControlStyleButtonText.text = perc;
        pushControlStyleDetails.text = percDetails;
        forceUnitsButtonText.text = newt;
        forceUnitsDetails.text = newtDetails;
        anchoredBoostButtonText.text = norm;
        anchoredBoostDetails.text = normDetails;
        aNFMinimumButtonText.text = aNFMinZero;
        aNFMinimumDetails.text = aNFMinZeroDetails;
        aNFMaximumButtonText.text = aNFMaxAF;
        aNFMaximumDetails.text = aNFMaxAFDetails;
        eWVSignageButtonText.text = eWVAlwaysDecreasing;
        eWVSignageDetails.text = eWVAlwaysDecreasingDetails;
        eWVRelativityButtonText.text = eWVRelative;
        eWVRelativityDetails.text = eWVRelativeDetails;
        distanceRelationshipButtonText.text = eWD;
        distanceRelationshipDetails.text = eWDDetails;
        forceConstantDetails.text = alloConstantDetails;
        maxRangeDetails.text = maxRDetails;

        sensitivity.value = FPVCameraLock.Sensitivity;
        smoothing.value = FPVCameraLock.Smoothing;
        distanceConstantSlider.value = PhysicsController.distanceConstant;
        velocityConstantSlider.value = PhysicsController.velocityConstant;
        forceConstantSlider.value = AllomanticIronSteel.AllomanticConstant;
        maxRangeSlider.value = AllomanticIronSteel.maxRange;

        sensitivityValueText.text = sensitivity.value.ToString();
        smoothingValueText.text = smoothing.value.ToString();
        distanceConstantValueText.text = distanceConstantSlider.value.ToString();
        velocityConstantValueText.text = velocityConstantSlider.value.ToString();
        forceConstantValueText.text = forceConstantSlider.value.ToString();
        maxRangeValueText.text = maxRangeSlider.value.ToString();

        gameplayButton.gameObject.SetActive(true);
        physicsButton.gameObject.SetActive(true);
        gameplayHeader.gameObject.SetActive(false);
        physicsHeader.gameObject.SetActive(false);
        rumbleLabel.gameObject.SetActive(false);
        aNFMinimumLabel.gameObject.SetActive(true);
        aNFMaximumLabel.gameObject.SetActive(true);
        eWVRelativityLabel.gameObject.SetActive(false);
        eWVSignageLabel.gameObject.SetActive(false);
        distanceConstantLabel.gameObject.SetActive(true);
        velocityConstantLabel.gameObject.SetActive(false);
        CloseSettings();
    }

    public void OpenSettings() {
        gameObject.SetActive(true);
    }

    public void CloseSettings() {
        CloseGameplay();
        ClosePhysics();
        gameObject.SetActive(false);
    }

    private void OpenGameplay() {
        gameplayButton.gameObject.SetActive(false);
        physicsButton.gameObject.SetActive(false);
        gameplayHeader.gameObject.SetActive(true);
    }

    private void CloseGameplay() {
        gameplayButton.gameObject.SetActive(true);
        physicsButton.gameObject.SetActive(true);
        gameplayHeader.gameObject.SetActive(false);
    }

    private void OpenPhysics() {
        gameplayButton.gameObject.SetActive(false);
        physicsButton.gameObject.SetActive(false);
        physicsHeader.gameObject.SetActive(true);
    }

    private void ClosePhysics() {
        gameplayButton.gameObject.SetActive(true);
        physicsButton.gameObject.SetActive(true);
        physicsHeader.gameObject.SetActive(false);
    }

    public void BackSettings() {
        if (IsGameplayOpen)
            CloseGameplay();
        else if (IsPhysicsOpen)
            ClosePhysics();
        else
            CloseSettings();
    }

    // On Button Click methods

    private void OnClickClose() {
        BackSettings();
    }

    private void OnClickControlScheme() {
        switch (GamepadController.currentControlScheme) {
            case ControlScheme.MouseKeyboard45: {
                    GamepadController.currentControlScheme = ControlScheme.MouseKeyboardQE;
                    controlSchemeButtonText.text = mkQE;
                    GamepadController.UsingMB45 = false;
                    break;
                }
            case ControlScheme.MouseKeyboardQE: {
                    GamepadController.currentControlScheme = ControlScheme.Gamepad;
                    controlSchemeButtonText.text = game;
                    controlSchemeDetails.text = gameDetails;
                    GamepadController.UsingGamepad = true;
                    rumbleLabel.gameObject.SetActive(true);
                    break;
                    //currentControlScheme = ControlScheme.MouseKeyboard45;
                    //controlSchemeButtonText.text = "Mouse and Keyboard (MB 4 & 5)";
                    //break;
                }
            default: {
                    GamepadController.currentControlScheme = ControlScheme.MouseKeyboard45;
                    controlSchemeButtonText.text = mk45;
                    controlSchemeDetails.text = "";
                    GamepadController.UsingMB45 = true;
                    GamepadController.UsingGamepad = false;
                    rumbleLabel.gameObject.SetActive(false);
                    break;
                }
        }
    }

    private void OnClickRumble() {
        if (GamepadController.UsingRumble) {
            rumbleButtonText.text = disa;
            GamepadController.UsingRumble = false;
        } else {
            rumbleButtonText.text = enab;
            GamepadController.UsingRumble = true;
        }
    }

    private void OnClickPushControlStyle() {
        switch (GamepadController.currentForceStyle) {
            case ForceStyle.ForceMagnitude: {
                    pushControlStyleButtonText.text = perc;
                    pushControlStyleDetails.text = percDetails;
                    GamepadController.currentForceStyle = ForceStyle.Percentage;
                    break;
                }
            default: {
                    pushControlStyleButtonText.text = forc;
                    pushControlStyleDetails.text = forcDetails;
                    GamepadController.currentForceStyle = ForceStyle.ForceMagnitude;
                    break;
                }
        }
    }

    private void OnClickForceUnitsButton() {
        switch (PhysicsController.displayUnits) {
            case ForceDisplayUnits.Newtons: {
                    forceUnitsButtonText.text = grav;
                    forceUnitsDetails.text = gravDetails;
                    PhysicsController.displayUnits = ForceDisplayUnits.Gs;
                    break;
                }
            default: {
                    forceUnitsButtonText.text = newt;
                    forceUnitsDetails.text = newtDetails;
                    PhysicsController.displayUnits = ForceDisplayUnits.Newtons;
                    break;
                }
        }
    }

    private void OnSensitivityChanged(float value) {
        FPVCameraLock.Sensitivity = value;
        sensitivityValueText.text = value.ToString();
    }

    private void OnSmoothingChanged(float value) {
        FPVCameraLock.Smoothing = value;
        smoothingValueText.text = value.ToString();
    }

    private void OnClickAnchoredBoost() {
        switch (PhysicsController.anchorBoostMode) {
            case AnchorBoostMode.AllomanticNormalForce: {
                    velocityConstantLabel.gameObject.SetActive(true);
                    aNFMaximumLabel.gameObject.SetActive(false);
                    aNFMinimumLabel.gameObject.SetActive(false);
                    eWVSignageLabel.gameObject.SetActive(true);
                    eWVRelativityLabel.gameObject.SetActive(true);

                    PhysicsController.anchorBoostMode = AnchorBoostMode.ExponentialWithVelocity;
                    anchoredBoostButtonText.text = expV;
                    anchoredBoostDetails.text = expVDetails;
                    forceConstantSlider.value *= 2;
                    break;
                }
            case AnchorBoostMode.ExponentialWithVelocity: {
                    velocityConstantLabel.gameObject.SetActive(false);
                    eWVSignageLabel.gameObject.SetActive(false);
                    eWVRelativityLabel.gameObject.SetActive(false);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.None;
                    anchoredBoostButtonText.text = anchorDisabled;
                    anchoredBoostDetails.text = anchorDisabledDetails;
                    forceConstantSlider.value /= 2;
                    break;
                }
            default: {
                    aNFMaximumLabel.gameObject.SetActive(true);
                    aNFMinimumLabel.gameObject.SetActive(true);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
                    anchoredBoostButtonText.text = norm;
                    anchoredBoostDetails.text = normDetails;
                    break;
                }
        }
    }

    private void OnClickANFMinimum() {
        switch (PhysicsController.normalForceMinimum) {
            case NormalForceMinimum.Zero: {
                    PhysicsController.normalForceMinimum = NormalForceMinimum.ZeroAndNegate;
                    aNFMinimumButtonText.text = aNFMinZeroNegate;
                    aNFMinimumDetails.text = aNFMinZeroNegateDetails;
                    break;
                }
            case NormalForceMinimum.ZeroAndNegate: {
                    PhysicsController.normalForceMinimum = NormalForceMinimum.Disabled;
                    aNFMinimumButtonText.text = aNFMinDisabled;
                    aNFMinimumDetails.text = aNFMinDisabledDetails;
                    break;
                }
            default: { // Disabled
                    PhysicsController.normalForceMinimum = NormalForceMinimum.Zero;
                    aNFMinimumButtonText.text = aNFMinZero;
                    aNFMinimumDetails.text = aNFMinZeroDetails;
                    break;
                }
        }
    }

    private void OnClickANFMaximum() {
        switch (PhysicsController.normalForceMaximum) {
            case NormalForceMaximum.AllomanticForce: {
                    PhysicsController.normalForceMaximum = NormalForceMaximum.Disabled;
                    aNFMaximumButtonText.text = aNFMaxDisabled;
                    aNFMaximumDetails.text = aNFMaxDisabledDetails;
                    break;
                }
            default: { // Disabled
                    PhysicsController.normalForceMaximum = NormalForceMaximum.AllomanticForce;
                    aNFMaximumButtonText.text = aNFMaxAF;
                    aNFMaximumDetails.text = aNFMaxAFDetails;
                    break;
                }
        }
    }

    private void OnClickEWVSignage() {
        switch (PhysicsController.exponentialWithVelocitySignage) {
            case ExponentialWithVelocitySignage.AllVelocityDecreasesForce: {
                    PhysicsController.exponentialWithVelocitySignage = ExponentialWithVelocitySignage.BackwardsDecreasesAndForwardsIncreasesForce;
                    eWVSignageButtonText.text = eWVChangesWithSign;
                    eWVSignageDetails.text = eWVChangesWithSignDetails;
                    break;
                }
            case ExponentialWithVelocitySignage.BackwardsDecreasesAndForwardsIncreasesForce: {
                    PhysicsController.exponentialWithVelocitySignage = ExponentialWithVelocitySignage.OnlyBackwardsDecreasesForce;
                    eWVSignageButtonText.text = eWVOnlyBackwardsDecreases;
                    eWVSignageDetails.text = eWVOnlyBackwardsDecreasesDetails;
                    break;
                }
            default: { // Disabled
                    PhysicsController.exponentialWithVelocitySignage = ExponentialWithVelocitySignage.AllVelocityDecreasesForce;
                    eWVSignageButtonText.text = eWVAlwaysDecreasing;
                    eWVSignageDetails.text = eWVAlwaysDecreasingDetails;
                    break;
                }
        }
    }

    private void OnClickEWVRelativity() {
        switch (PhysicsController.exponentialWithVelocityRelativity) {
            case ExponentialWithVelocityRelativity.Absolute: {
                    PhysicsController.exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Relative;
                    eWVRelativityButtonText.text = eWVRelative;
                    eWVRelativityDetails.text = eWVRelativeDetails;
                    break;
                }
            default: { // Relative
                    PhysicsController.exponentialWithVelocityRelativity = ExponentialWithVelocityRelativity.Absolute;
                    eWVRelativityButtonText.text = eWVAbsolute;
                    eWVRelativityDetails.text = eWVAbsoluteDetails;
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
                    distanceRelationshipButtonText.text = linear;
                    distanceRelationshipDetails.text = linearDetails;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Linear;
                    forceConstantSlider.value /= 40f / 12f;
                    break;
                }
            case ForceDistanceRelationship.Linear: {
                    distanceConstantLabel.gameObject.SetActive(true);
                    distanceRelationshipButtonText.text = eWD;
                    distanceRelationshipDetails.text = eWDDetails;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Exponential;
                    break;
                }
            case ForceDistanceRelationship.Exponential: {
                    distanceConstantLabel.gameObject.SetActive(false);
                    distanceRelationshipButtonText.text = inverse;
                    distanceRelationshipDetails.text = inverseDetails;
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

    private void OnMaxRangeChanged(float value) {
        AllomanticIronSteel.maxRange = ((int)value);
        maxRangeValueText.text = ((int)value).ToString();
    }
}
