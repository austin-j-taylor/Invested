using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausedMenu : MonoBehaviour {

    private const string mk45 = "Mouse and Keyboard (MB 4 & 5)";
    private const string mkQE = "Mouse and Keyboard (Keys Q & E)";
    private const string game = "Gamepad";
    private const string disa = "Disabled";
    private const string enab = "Enabled";
    private const string forc = "Set force magnitude. While pushing, pushes will always have that magnitude (if possible).";
    private const string perc = "Set percentage of maximum possible force. Cannot directly modify force magnitude.";
    private const string inve = "F ∝ 1 / d² where d = distance between Allomancer and target\n(Inverse square law)";
    private const string line = "F ∝ 1 - d / R where d = distance between Allomancer and target;\nR = maximum range of push (Linear)";

    [SerializeField]
    private Image pauseMenu;
    [SerializeField]
    private Button controlSchemeButton;
    [SerializeField]
    private Text rumbleControl;
    [SerializeField]
    private Slider sensitivity;
    [SerializeField]
    private Slider smoothing;
    [SerializeField]
    private Slider forceConstant;
    [SerializeField]
    private Button quitButton;
    [SerializeField]
    private Button resetButton;
    [SerializeField]
    private Button forceStyleButton;
    [SerializeField]
    private Button forceModeButton;

    private Button rumbleButton;
    private Text rumbleText;
    private Text controlSchemeText;
    private Text forceStyleText;
    private Text forceModeText;
    private Text sensitivityText;
    private Text smoothingText;
    private Text forceConstantText;

    private bool paused;

    // Use this for initialization
    void Start() {
        controlSchemeText = controlSchemeButton.GetComponentInChildren<Text>();
        rumbleButton = rumbleControl.GetComponentInChildren<Button>();
        rumbleText = rumbleButton.GetComponentInChildren<Text>();
        forceStyleText = forceStyleButton.GetComponentInChildren<Text>();
        forceModeText = forceModeButton.GetComponentInChildren<Text>();

        sensitivityText = sensitivity.GetComponentInChildren<Text>();
        smoothingText = smoothing.GetComponentInChildren<Text>();
        forceConstantText = forceConstant.GetComponentInChildren<Text>();

        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        smoothing.onValueChanged.AddListener(OnSmoothingChanged);
        forceConstant.onValueChanged.AddListener(OnForceConstantChanged);
        quitButton.onClick.AddListener(Quit);
        resetButton.onClick.AddListener(ClickReset);
        forceStyleButton.onClick.AddListener(ClickForceStyle);
        forceModeButton.onClick.AddListener(ClickForceButton);

        pauseMenu.gameObject.SetActive(false);
        rumbleControl.gameObject.SetActive(false);
        paused = false;
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        controlSchemeText.text = mk45;
        forceStyleText.text = perc;

        sensitivity.value = FPVCameraLock.Sensitivity;
        smoothing.value = FPVCameraLock.Smoothing;
        forceConstant.value = AllomanticIronSteel.AllomanticConstant;

        sensitivityText.text = sensitivity.value.ToString();
        smoothingText.text = smoothing.value.ToString();
        forceConstantText.text = forceConstant.value.ToString();
    }

    public void TogglePaused() {
        if (paused)
            UnPause();
        else
            Pause();
    }

    private void Pause() {
        //Cursor.visible = true;
        FPVCameraLock.UnlockCamera();
        Time.timeScale = 0f;
        pauseMenu.gameObject.SetActive(true);
        paused = true;
    }

    private void UnPause() {
        //Cursor.visible = false;
        FPVCameraLock.LockCamera();
        Time.timeScale = 1f;
        pauseMenu.gameObject.SetActive(false);
        paused = false;
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

    private void Quit() {
        Application.Quit();
    }

    private void ClickReset() {
        UnPause();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
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
        switch(PhysicsController.calculationMode) {
            case ForceCalculationMode.InverseSquareLaw: {
                    forceModeText.text = line;
                    PhysicsController.calculationMode = ForceCalculationMode.Linear;
                    forceConstant.value /= 10;
                    break;
                }
            default: {
                    forceModeText.text = inve;
                    PhysicsController.calculationMode = ForceCalculationMode.InverseSquareLaw;
                    forceConstant.value *= 10;
                    break;
                }
        }
    }

}
