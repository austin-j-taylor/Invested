using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    // String constants for button text
    private const string mk45 = "Mouse and Keyboard (MB 4 & 5)";
    private const string mkQE = "Mouse and Keyboard (Keys Q & E)";
    private const string game = "Gamepad";
    private const string disa = "Disabled";
    private const string enab = "Enabled";
    private const string forc = "Control force magnitude. Pushes will always\ntry to have that magnitude.";
    private const string perc = "Control percentage of maximum possible force.\nCannot directly control force magnitude.";
    private const string inve = "F ∝ 1 / d² where d = distance between Allomancer and target\n(Inverse square law)";
    private const string line = "F ∝ 1 - d / R where d = distance between Allomancer and target;\nR = maximum range of push (Linear)";
    private const string newt = "Newtons (a force)";
    private const string gs = "G's (an acceleration)";
    
    private Button controlSchemeButton;
    private Text rumbleControl;
    private Slider sensitivity;
    private Slider smoothing;
    private Slider forceConstant;
    private Slider maxRange;
    private Button closeButton;
    private Button forceStyleButton;
    private Button forceModeButton;
    private Button forceUnitsButton;

    private Button rumbleButton;
    private Text rumbleText;
    private Text controlSchemeText;
    private Text forceStyleText;
    private Text forceModeText;
    private Text forceUnitsText;
    private Text sensitivityText;
    private Text smoothingText;
    private Text forceConstantText;
    private Text maxRangeText;

    // Use this for initialization
    void Start() {
        Button[] buttons = GetComponentsInChildren<Button>();
        Slider[] sliders = GetComponentsInChildren<Slider>();
        Text[] texts = GetComponentsInChildren<Text>();
        controlSchemeButton = buttons[0];
        rumbleButton = buttons[1];
        forceStyleButton = buttons[2];
        forceModeButton = buttons[3];
        forceUnitsButton = buttons[4];
        closeButton = buttons[5];

        sensitivity = sliders[0];
        smoothing = sliders[1];
        forceConstant = sliders[2];
        maxRange = sliders[3];

        rumbleControl = texts[5];
        
        rumbleButton = rumbleControl.GetComponentInChildren<Button>();
        controlSchemeText = controlSchemeButton.GetComponentInChildren<Text>();
        rumbleText = rumbleButton.GetComponentInChildren<Text>();
        forceStyleText = forceStyleButton.GetComponentInChildren<Text>();
        forceModeText = forceModeButton.GetComponentInChildren<Text>();
        forceUnitsText = forceUnitsButton.GetComponentInChildren<Text>();

        sensitivityText = sensitivity.GetComponentInChildren<Text>();
        smoothingText = smoothing.GetComponentInChildren<Text>();
        forceConstantText = forceConstant.GetComponentInChildren<Text>();
        maxRangeText = maxRange.GetComponentInChildren<Text>();

        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        smoothing.onValueChanged.AddListener(OnSmoothingChanged);
        forceConstant.onValueChanged.AddListener(OnForceConstantChanged);
        maxRange.onValueChanged.AddListener(OnMaxRangeChanged);

        closeButton.onClick.AddListener(ClickClose);
        forceStyleButton.onClick.AddListener(ClickForceStyle);
        forceModeButton.onClick.AddListener(ClickForceButton);
        forceUnitsButton.onClick.AddListener(ClickForceUnitsButton);

        rumbleControl.gameObject.SetActive(false);
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        controlSchemeText.text = mk45;
        forceStyleText.text = perc;
        forceModeText.text = line;
        forceUnitsText.text = newt;

        sensitivity.value = FPVCameraLock.Sensitivity;
        smoothing.value = FPVCameraLock.Smoothing;
        forceConstant.value = AllomanticIronSteel.AllomanticConstant;
        maxRange.value = AllomanticIronSteel.maxRange;

        sensitivityText.text = sensitivity.value.ToString();
        smoothingText.text = smoothing.value.ToString();
        forceConstantText.text = forceConstant.value.ToString();
        maxRangeText.text = maxRange.value.ToString();

        CloseSettings();
    }

    public void OpenSettings() {
        gameObject.SetActive(true);
    }

    public void CloseSettings() {
        gameObject.SetActive(false);
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
                    rumbleControl.gameObject.SetActive(true);
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
                    rumbleControl.gameObject.SetActive(false);
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
        sensitivityText.text = sensitivity.value.ToString();
        FPVCameraLock.Sensitivity = value;
    }

    private void OnSmoothingChanged(float value) {
        FPVCameraLock.Smoothing = value;
        smoothingText.text = smoothing.value.ToString();
    }

    private void OnForceConstantChanged(float value) {
        AllomanticIronSteel.AllomanticConstant = value;
        forceConstantText.text = value.ToString();
    }

    private void OnMaxRangeChanged(float value) {
        AllomanticIronSteel.maxRange = value;
        maxRangeText.text = value.ToString();
    }

    private void ClickClose() {
        CloseSettings();
    }

    private void ClickForceStyle() {
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

    private void ClickForceButton() {
        switch (PhysicsController.calculationMode) {
            case ForceCalculationMode.InverseSquareLaw: {
                    forceModeText.text = line;
                    PhysicsController.calculationMode = ForceCalculationMode.Linear;
                    forceConstant.value /= 40f / 6f;
                    break;
                }
            default: {
                    forceModeText.text = inve;
                    PhysicsController.calculationMode = ForceCalculationMode.InverseSquareLaw;
                    forceConstant.value *= 40f / 6f;
                    break;
                }
        }
    }

    private void ClickForceUnitsButton() {
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
