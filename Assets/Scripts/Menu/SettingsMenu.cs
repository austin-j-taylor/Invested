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
    private const string expo = "F ∝ e ^ -d/C where d = distance between Allomancer and target;\nC = Exponential Constant (Exponential)";
    private const string newt = "Newtons (a force)";
    private const string gs = "G's (an acceleration)";
    
    private Button gameplayButton;
    private Button physicsButton;
    private Text gameplayHeader;
    private Text physicsHeader;

    private Button controlSchemeButton;
    private Text rumbleLabel;
    private Slider sensitivity;
    private Slider smoothing;
    private Text exponentialConstantLabel;
    private Slider exponentialConstant;
    private Slider forceConstant;
    private Slider maxRange;
    private Button closeButton;
    private Button normalForceButton;
    private Button forceStyleButton;
    private Button forceModeButton;
    private Button forceUnitsButton;

    private Button rumbleButton;
    private Text rumbleText;
    private Text controlSchemeText;
    private Text normalForceText;
    private Text forceStyleText;
    private Text forceModeText;
    private Text forceUnitsText;
    private Text sensitivityText;
    private Text smoothingText;
    private Text exponentialConstantText;
    private Text forceConstantText;
    private Text maxRangeText;
    
    // Use this for initialization
    void Start() {
        Button[] buttons = GetComponentsInChildren<Button>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        Text[] texts = GetComponentsInChildren<Text>();
        gameplayButton = buttons[0];
        physicsButton = buttons[1];
        controlSchemeButton = buttons[2];
        rumbleButton = buttons[3];
        normalForceButton = buttons[4];
        forceStyleButton = buttons[5];
        forceModeButton = buttons[6];
        forceUnitsButton = buttons[7];
        closeButton = buttons[8];

        sensitivity = sliders[0];
        smoothing = sliders[1];
        exponentialConstant = sliders[2];
        forceConstant = sliders[3];
        maxRange = sliders[4];

        gameplayHeader = texts[4];
        physicsHeader = texts[13];
        rumbleLabel = texts[7];
        exponentialConstantLabel = texts[20];

        rumbleButton = rumbleLabel.GetComponentInChildren<Button>();
        controlSchemeText = controlSchemeButton.GetComponentInChildren<Text>();
        rumbleText = rumbleButton.GetComponentInChildren<Text>();
        normalForceText = normalForceButton.GetComponentInChildren<Text>();
        forceStyleText = forceStyleButton.GetComponentInChildren<Text>();
        forceModeText = forceModeButton.GetComponentInChildren<Text>();
        forceUnitsText = forceUnitsButton.GetComponentInChildren<Text>();

        sensitivityText = sensitivity.GetComponentInChildren<Text>();
        smoothingText = smoothing.GetComponentInChildren<Text>();
        exponentialConstantText = exponentialConstant.GetComponentInChildren<Text>();
        forceConstantText = forceConstant.GetComponentInChildren<Text>();
        maxRangeText = maxRange.GetComponentInChildren<Text>();

        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        smoothing.onValueChanged.AddListener(OnSmoothingChanged);
        exponentialConstant.onValueChanged.AddListener(OnExponentialConstantChanged);
        forceConstant.onValueChanged.AddListener(OnForceConstantChanged);
        maxRange.onValueChanged.AddListener(OnMaxRangeChanged);

        normalForceButton.onClick.AddListener(OnClickNormalForce);
        forceStyleButton.onClick.AddListener(OnClickForceStyle);
        forceModeButton.onClick.AddListener(OnClickForceButton);
        forceUnitsButton.onClick.AddListener(OnClickForceUnitsButton);

        gameplayButton.onClick.AddListener(OpenGameplay);
        physicsButton.onClick.AddListener(OpenPhysics);
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        closeButton.onClick.AddListener(OnClickClose);

        controlSchemeText.text = mk45;
        normalForceText.text = enab;
        forceStyleText.text = perc;
        forceModeText.text = line;
        forceUnitsText.text = newt;

        sensitivity.value = FPVCameraLock.Sensitivity;
        smoothing.value = FPVCameraLock.Smoothing;
        exponentialConstant.value = PhysicsController.exponentialConstantC;
        forceConstant.value = AllomanticIronSteel.AllomanticConstant;
        maxRange.value = AllomanticIronSteel.maxRange;

        sensitivityText.text = sensitivity.value.ToString();
        smoothingText.text = smoothing.value.ToString();
        exponentialConstantText.text = exponentialConstant.value.ToString();
        forceConstantText.text = forceConstant.value.ToString();
        maxRangeText.text = maxRange.value.ToString();

        rumbleLabel.gameObject.SetActive(false);
        exponentialConstantLabel.gameObject.SetActive(false);
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

    private void OnClickNormalForce() {
        switch(PhysicsController.normalForceMode) {
            case NormalForceMode.Enabled: {
                    PhysicsController.normalForceMode = NormalForceMode.Disabled;
                    normalForceText.text = disa;
                    break;
                }
            default: {
                    PhysicsController.normalForceMode = NormalForceMode.Enabled;
                    normalForceText.text = enab;
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

    private void OnExponentialConstantChanged(float value) {
        PhysicsController.exponentialConstantC = value;
        exponentialConstantText.text = value.ToString();
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

    private void OnClickForceButton() {
        switch (PhysicsController.calculationMode) {
            case ForceCalculationMode.InverseSquareLaw: {
                    forceModeText.text = line;
                    PhysicsController.calculationMode = ForceCalculationMode.Linear;
                    forceConstant.value /= 40f / 12f;
                    break;
                }
            case ForceCalculationMode.Linear: {
                    exponentialConstantLabel.gameObject.SetActive(true);
                    forceModeText.text = expo;
                    PhysicsController.calculationMode = ForceCalculationMode.Exponential;
                    break;
                }
            case ForceCalculationMode.Exponential: {
                    exponentialConstantLabel.gameObject.SetActive(false);
                    forceModeText.text = inve;
                    PhysicsController.calculationMode = ForceCalculationMode.InverseSquareLaw;
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
