using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    // String constants for button text
    private const string mk45 = "Mouse and Keyboard (MB 4 & 5)";
    private const string mkQE = "Mouse and Keyboard (Keys Q & E)";
    private const string game = "Gamepad (Disconnect and reconnect gamepad if not working)";
    private const string disa = "Disabled";
    private const string enab = "Enabled";
    private const string forc = "Control force magnitude. Pushes will always\ntry to have that magnitude.";
    private const string perc = "Control percentage of maximum possible force.\nCannot directly control force magnitude.";
    private const string inve = "F ∝ 1 / d² where d = distance between Allomancer and target\n(Inverse square law)";
    private const string line = "F ∝ 1 - d / R where d = distance between Allomancer and target;\nR = maximum range of push (Linear)";
    private const string expD = "F ∝ e ^ -d/D where d = distance between Allomancer and target;\nD = Exponential Constant (Exponential)";
    private const string newt = "Newtons (a force)";
    private const string gs = "G's (an acceleration)";
    private const string norm = "If an anchored target is Pushed, the player experiences the resistance the target gets from that Push (Allomantic Normal Force)";
    private const string expV = "F ∝ e ^ -v/V where v = velocity of Allomancor or target;\nV = Exponential Constant (Exponential with Velocity)";

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
    private Text rumbleLabel;
    private Button rumbleButton;
    private Text rumbleButtonText;
    private Slider sensitivity;
    private Text sensitivityValueText;
    private Slider smoothing;
    private Text smoothingValueText;
    private Slider distanceConstantSlider;
    private Text distanceConstantValueText;
    private Slider velocityConstantSlider;
    private Text velocityConstantValueText;
    private Slider forceConstantSlider;
    private Text forceConstantValueText;
    private Slider maxRangeSlider;
    private Text maxRangeValueText;

    private Button closeButton;
    private Button anchoredBoostButton;
    private Text anchoredBoostButtonText;
    private Button pushControlStyleButton;
    private Text pushControlStyleButtonText;
    private Button distanceRelationshipButton;
    private Text distanceRelationshipButtonText;
    private Button forceUnitsButton;
    private Text forceUnitsButtonText;

    private Text controlSchemeButtonText;

    private Text distanceConstantLabel;
    private Text velocityConstantLabel;
    
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

        rumbleLabel = gameplayHeader.GetChild(1).GetComponent<Text>();
        rumbleButton = rumbleLabel.GetComponentInChildren<Button>();
        rumbleButtonText = rumbleButton.GetComponentInChildren<Text>();

        pushControlStyleButton = gameplayHeader.GetChild(2).GetChild(0).GetComponent<Button>();
        pushControlStyleButtonText = pushControlStyleButton.GetComponentInChildren<Text>();

        sensitivity = gameplayHeader.GetChild(3).GetComponentInChildren<Slider>();
        sensitivityValueText = sensitivity.GetComponentInChildren<Text>();
        smoothing = gameplayHeader.GetChild(4).GetComponentInChildren<Slider>();
        smoothingValueText = smoothing.GetComponentInChildren<Text>();

        // Physics Header
        physicsHeader = transform.GetChild(3);
        anchoredBoostButton = physicsHeader.GetChild(0).GetChild(0).GetComponent<Button>();
        anchoredBoostButtonText = anchoredBoostButton.GetComponentInChildren<Text>();

        velocityConstantLabel = physicsHeader.GetChild(1).GetComponent<Text>();
        velocityConstantSlider = velocityConstantLabel.GetComponentInChildren<Slider>();
        velocityConstantValueText = velocityConstantSlider.GetComponentInChildren<Text>();

        distanceRelationshipButton = physicsHeader.GetChild(2).GetChild(0).GetComponent<Button>();
        distanceRelationshipButtonText = distanceRelationshipButton.GetComponentInChildren<Text>();

        distanceConstantLabel = physicsHeader.GetChild(3).GetComponent<Text>();
        distanceConstantSlider = distanceConstantLabel.GetComponentInChildren<Slider>();
        distanceConstantValueText = distanceConstantSlider.GetComponentInChildren<Text>();

        forceUnitsButton = physicsHeader.GetChild(4).GetChild(0).GetComponent<Button>();
        forceUnitsButtonText = forceUnitsButton.GetComponentInChildren<Text>();

        forceConstantSlider = physicsHeader.GetChild(5).GetComponentInChildren<Slider>();
        forceConstantValueText = forceConstantSlider.GetComponentInChildren<Text>();
        maxRangeSlider = physicsHeader.GetChild(6).GetComponentInChildren<Slider>();
        maxRangeValueText = maxRangeSlider.GetComponentInChildren<Text>();



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

        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        smoothing.onValueChanged.AddListener(OnSmoothingChanged);
        distanceConstantSlider.onValueChanged.AddListener(OnDistanceConstantChanged);
        velocityConstantSlider.onValueChanged.AddListener(OnVelocityConstantChanged);
        forceConstantSlider.onValueChanged.AddListener(OnForceConstantChanged);
        maxRangeSlider.onValueChanged.AddListener(OnMaxRangeChanged);

        anchoredBoostButton.onClick.AddListener(OnClickAnchoredBoost);
        pushControlStyleButton.onClick.AddListener(OnClickForceStyle);
        distanceRelationshipButton.onClick.AddListener(OnClickDistanceRelationshipButton);
        forceUnitsButton.onClick.AddListener(OnClickForceUnitsButton);

        gameplayButton.onClick.AddListener(OpenGameplay);
        physicsButton.onClick.AddListener(OpenPhysics);
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        closeButton.onClick.AddListener(OnClickClose);

        controlSchemeButtonText.text = mk45;
        anchoredBoostButtonText.text = norm;
        pushControlStyleButtonText.text = perc;
        distanceRelationshipButtonText.text = expD;
        forceUnitsButtonText.text = newt;

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

        rumbleLabel.gameObject.SetActive(false);
        distanceConstantLabel.gameObject.SetActive(true);
        velocityConstantLabel.gameObject.SetActive(false);
        gameplayButton.gameObject.SetActive(true);
        physicsButton.gameObject.SetActive(true);
        gameplayHeader.gameObject.SetActive(false);
        physicsHeader.gameObject.SetActive(false);
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

    private void OnSensitivityChanged(float value) {
        FPVCameraLock.Sensitivity = value;
        sensitivityValueText.text = value.ToString();
    }

    private void OnSmoothingChanged(float value) {
        FPVCameraLock.Smoothing = value;
        smoothingValueText.text = value.ToString();
    }

    private void OnClickAnchoredBoost() {
        switch(PhysicsController.anchorBoostMode) {
            case AnchorBoostMode.AllomanticNormalForce: {
                    velocityConstantLabel.gameObject.SetActive(true);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.ExponentialWithVelocity;
                    anchoredBoostButtonText.text = expV;
                    break;
                }
            case AnchorBoostMode.ExponentialWithVelocity: {
                    velocityConstantLabel.gameObject.SetActive(false);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.None;
                    anchoredBoostButtonText.text = disa;
                    break;
                }
            default: {
                    PhysicsController.anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
                    anchoredBoostButtonText.text = norm;
                    break;
                }
        }
    }

    private void OnVelocityConstantChanged(float value) {
        PhysicsController.velocityConstant = value;
        velocityConstantValueText.text = value.ToString();
    }

    private void OnClickDistanceRelationshipButton() {
        switch (PhysicsController.distanceRelationshipMode) {
            case ForceDistanceRelationship.InverseSquareLaw: {
                    distanceRelationshipButtonText.text = line;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Linear;
                    forceConstantSlider.value /= 40f / 12f;
                    break;
                }
            case ForceDistanceRelationship.Linear: {
                    distanceConstantLabel.gameObject.SetActive(true);
                    distanceRelationshipButtonText.text = expD;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Exponential;
                    break;
                }
            case ForceDistanceRelationship.Exponential: {
                    distanceConstantLabel.gameObject.SetActive(false);
                    distanceRelationshipButtonText.text = inve;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.InverseSquareLaw;
                    forceConstantSlider.value *= 40f / 12f;
                    break;
                }
        }
    }

    private void OnDistanceConstantChanged(float value) {
        PhysicsController.distanceConstant = value;
        distanceConstantValueText.text = value.ToString();
    }

    private void OnClickForceStyle() {
        switch(GamepadController.currentForceStyle) {
            case ForceStyle.ForceMagnitude: {
                    pushControlStyleButtonText.text = perc;
                    GamepadController.currentForceStyle = ForceStyle.Percentage;
                    break;
                }
            default: {
                    pushControlStyleButtonText.text = forc;
                    GamepadController.currentForceStyle = ForceStyle.ForceMagnitude;
                    break;
                }
        }
    }

    private void OnClickForceUnitsButton() {
        switch (PhysicsController.displayUnits) {
            case ForceDisplayUnits.Newtons: {
                    forceUnitsButtonText.text = gs;
                    PhysicsController.displayUnits = ForceDisplayUnits.Gs;
                    break;
                }
            default: {
                    forceUnitsButtonText.text = newt;
                    PhysicsController.displayUnits = ForceDisplayUnits.Newtons;
                    break;
                }
        }
    }

    private void OnForceConstantChanged(float value) {
        AllomanticIronSteel.AllomanticConstant = value;
        forceConstantValueText.text = value.ToString();
    }

    private void OnMaxRangeChanged(float value) {
        AllomanticIronSteel.maxRange = value;
        maxRangeValueText.text = value.ToString();
    }
}
