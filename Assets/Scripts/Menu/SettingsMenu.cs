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

    private Button gameplayButton;
    private Button physicsButton;
    private Text gameplayHeader;
    private Text physicsHeader;

    private Button controlSchemeButton;
    private Text rumbleLabel;
    private Slider sensitivity;
    private Slider smoothing;
    private Text distanceConstantLabel;
    private Text velocityConstantLabel;
    private Slider distanceConstant;
    private Slider velocityConstant;
    private Slider forceConstant;
    private Slider maxRange;
    private Button closeButton;
    private Button anchoredBoostButton;
    private Button forceStyleButton;
    private Button distanceRelationshipButton;
    private Button forceUnitsButton;

    private Button rumbleButton;
    private Text rumbleText;
    private Text controlSchemeText;
    private Text anchoredBoostText;
    private Text forceStyleText;
    private Text distanceRelationshipText;
    private Text forceUnitsText;
    private Text sensitivityText;
    private Text smoothingText;
    private Text distanceConstantText;
    private Text velocityConstantText;
    private Text forceConstantText;
    private Text maxRangeText;

    //public Button[] buttons;
    //public Text[] texts;

    // Use this for initialization
    void Start() {
        //buttons = GetComponentsInChildren<Button>();
        //sliders = GetComponentsInChildren<Slider>();
        //texts = GetComponentsInChildren<Text>();
        Button[] buttons = GetComponentsInChildren<Button>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        Text[] texts = GetComponentsInChildren<Text>();
        gameplayButton = buttons[0];
        physicsButton = buttons[1];
        controlSchemeButton = buttons[2];
        rumbleButton = buttons[3];
        forceStyleButton = buttons[4];
        anchoredBoostButton = buttons[5];
        distanceRelationshipButton = buttons[6];
        forceUnitsButton = buttons[7];
        closeButton = buttons[8];

        sensitivity = sliders[0];
        smoothing = sliders[1];
        distanceConstant = sliders[2];
        velocityConstant = sliders[3];
        forceConstant = sliders[4];
        maxRange = sliders[5];

        gameplayHeader = texts[4];
        physicsHeader = texts[13];
        rumbleLabel = texts[7];
        distanceConstantLabel = texts[20];
        velocityConstantLabel = texts[22];

        rumbleButton = rumbleLabel.GetComponentInChildren<Button>();
        controlSchemeText = controlSchemeButton.GetComponentInChildren<Text>();
        rumbleText = rumbleButton.GetComponentInChildren<Text>();
        anchoredBoostText = anchoredBoostButton.GetComponentInChildren<Text>();
        forceStyleText = forceStyleButton.GetComponentInChildren<Text>();
        distanceRelationshipText = distanceRelationshipButton.GetComponentInChildren<Text>();
        forceUnitsText = forceUnitsButton.GetComponentInChildren<Text>();

        sensitivityText = sensitivity.GetComponentInChildren<Text>();
        smoothingText = smoothing.GetComponentInChildren<Text>();
        distanceConstantText = distanceConstant.GetComponentInChildren<Text>();
        velocityConstantText = velocityConstant.GetComponentInChildren<Text>();
        forceConstantText = forceConstant.GetComponentInChildren<Text>();
        maxRangeText = maxRange.GetComponentInChildren<Text>();

        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        smoothing.onValueChanged.AddListener(OnSmoothingChanged);
        distanceConstant.onValueChanged.AddListener(OnDistanceConstantChanged);
        velocityConstant.onValueChanged.AddListener(OnVelocityConstantChanged);
        forceConstant.onValueChanged.AddListener(OnForceConstantChanged);
        maxRange.onValueChanged.AddListener(OnMaxRangeChanged);

        anchoredBoostButton.onClick.AddListener(OnClickAnchoredBoost);
        forceStyleButton.onClick.AddListener(OnClickForceStyle);
        distanceRelationshipButton.onClick.AddListener(OnClickDistanceRelationshipButton);
        forceUnitsButton.onClick.AddListener(OnClickForceUnitsButton);

        gameplayButton.onClick.AddListener(OpenGameplay);
        physicsButton.onClick.AddListener(OpenPhysics);
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        closeButton.onClick.AddListener(OnClickClose);

        controlSchemeText.text = mk45;
        anchoredBoostText.text = norm;
        forceStyleText.text = perc;
        distanceRelationshipText.text = expD;
        forceUnitsText.text = newt;

        sensitivity.value = FPVCameraLock.Sensitivity;
        smoothing.value = FPVCameraLock.Smoothing;
        distanceConstant.value = PhysicsController.distanceConstant;
        velocityConstant.value = PhysicsController.velocityConstant;
        forceConstant.value = AllomanticIronSteel.AllomanticConstant;
        maxRange.value = AllomanticIronSteel.maxRange;

        sensitivityText.text = sensitivity.value.ToString();
        smoothingText.text = smoothing.value.ToString();
        distanceConstantText.text = distanceConstant.value.ToString();
        velocityConstantText.text = velocityConstant.value.ToString();
        forceConstantText.text = forceConstant.value.ToString();
        maxRangeText.text = maxRange.value.ToString();

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

    private void OnClickAnchoredBoost() {
        switch(PhysicsController.anchorBoostMode) {
            case AnchorBoostMode.AllomanticNormalForce: {
                    velocityConstantLabel.gameObject.SetActive(true);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.ExponentialWithVelocity;
                    anchoredBoostText.text = expV;
                    forceConstant.value *= 2f;
                    break;
                }
            case AnchorBoostMode.ExponentialWithVelocity: {
                    velocityConstantLabel.gameObject.SetActive(false);
                    PhysicsController.anchorBoostMode = AnchorBoostMode.None;
                    anchoredBoostText.text = disa;
                    forceConstant.value /= 2f;
                    break;
                }
            default: {
                    PhysicsController.anchorBoostMode = AnchorBoostMode.AllomanticNormalForce;
                    anchoredBoostText.text = norm;
                    break;
                }
        }
    }

    private void OnClickControlScheme() {
        switch (GamepadController.currentControlScheme) {
            case ControlScheme.MouseKeyboard45: {
                    GamepadController.currentControlScheme = ControlScheme.MouseKeyboardQE;
                    controlSchemeText.text = mkQE;
                    GamepadController.UsingMB45 = false;
                    break;
                }
            case ControlScheme.MouseKeyboardQE: {
                    GamepadController.currentControlScheme = ControlScheme.Gamepad;
                    controlSchemeText.text = game;
                    GamepadController.UsingGamepad = true;
                    rumbleLabel.gameObject.SetActive(true);
                    break;
                    //currentControlScheme = ControlScheme.MouseKeyboard45;
                    //controlSchemeText.text = "Mouse and Keyboard (MB 4 & 5)";
                    //break;
                }
            default: {
                    GamepadController.currentControlScheme = ControlScheme.MouseKeyboard45;
                    controlSchemeText.text = mk45;
                    GamepadController.UsingMB45 = true;
                    GamepadController.UsingGamepad = false;
                    rumbleLabel.gameObject.SetActive(false);
                    break;
                }
        }
    }

    private void OnClickRumble() {
        if(GamepadController.UsingRumble) {
            rumbleText.text = disa;
            GamepadController.UsingRumble = false;
        } else {
            rumbleText.text = enab;
            GamepadController.UsingRumble = true;
        }
    }

    private void OnSensitivityChanged(float value) {
        FPVCameraLock.Sensitivity = value;
        sensitivityText.text = value.ToString();
    }

    private void OnSmoothingChanged(float value) {
        FPVCameraLock.Smoothing = value;
        smoothingText.text = value.ToString();
    }

    private void OnDistanceConstantChanged(float value) {
        PhysicsController.distanceConstant = value;
        distanceConstantText.text = value.ToString();
    }

    private void OnVelocityConstantChanged(float value) {
        PhysicsController.velocityConstant = value;
        velocityConstantText.text = value.ToString();
    }

    private void OnForceConstantChanged(float value) {
        AllomanticIronSteel.AllomanticConstant = value;
        forceConstantText.text = value.ToString();
    }

    private void OnMaxRangeChanged(float value) {
        AllomanticIronSteel.maxRange = value;
        maxRangeText.text = value.ToString();
    }

    private void OnClickClose() {
        if (gameplayHeader.gameObject.activeSelf)
            CloseGameplay();
        else if (physicsHeader.gameObject.activeSelf)
            ClosePhysics();
        else
            CloseSettings();
    }

    private void OnClickForceStyle() {
        switch(GamepadController.currentForceStyle) {
            case ForceStyle.ForceMagnitude: {
                    forceStyleText.text = perc;
                    GamepadController.currentForceStyle = ForceStyle.Percentage;
                    break;
                }
            default: {
                    forceStyleText.text = forc;
                    GamepadController.currentForceStyle = ForceStyle.ForceMagnitude;
                    break;
                }
        }
    }

    private void OnClickDistanceRelationshipButton() {
        switch (PhysicsController.distanceRelationshipMode) {
            case ForceDistanceRelationship.InverseSquareLaw: {
                    distanceRelationshipText.text = line;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Linear;
                    forceConstant.value /= 40f / 12f;
                    break;
                }
            case ForceDistanceRelationship.Linear: {
                    distanceConstantLabel.gameObject.SetActive(true);
                    distanceRelationshipText.text = expD;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.Exponential;
                    break;
                }
            case ForceDistanceRelationship.Exponential: {
                    distanceConstantLabel.gameObject.SetActive(false);
                    distanceRelationshipText.text = inve;
                    PhysicsController.distanceRelationshipMode = ForceDistanceRelationship.InverseSquareLaw;
                    forceConstant.value *= 40f / 12f;
                    break;
                }
        }
    }

    private void OnClickForceUnitsButton() {
        switch (PhysicsController.displayUnits) {
            case ForceDisplayUnits.Newtons: {
                    forceUnitsText.text = gs;
                    PhysicsController.displayUnits = ForceDisplayUnits.Gs;
                    break;
                }
            default: {
                    forceUnitsText.text = newt;
                    PhysicsController.displayUnits = ForceDisplayUnits.Newtons;
                    break;
                }
        }
    }
}
