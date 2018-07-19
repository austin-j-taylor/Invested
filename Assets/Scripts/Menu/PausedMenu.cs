using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausedMenu : MonoBehaviour {

    [SerializeField]
    private Image pauseMenu;
    [SerializeField]
    private Button controlSchemeButton;
    private Text controlSchemeText;
    [SerializeField]
    private Text rumbleControl;
    private Button rumbleButton;
    private Text rumbleText;
    [SerializeField]
    private Slider sensitivity;
    [SerializeField]
    private Slider smoothing;
    [SerializeField]
    private Button quitButton;
    [SerializeField]
    private Button resetButton;

    private bool paused;
    private enum ControlScheme { MouseKeyboard45, MouseKeyboardQE, Gamepad }
    private ControlScheme currentControlScheme;

    // Use this for initialization
    void Start() {
        controlSchemeText = controlSchemeButton.GetComponentInChildren<Text>();
        rumbleButton = rumbleControl.GetComponentInChildren<Button>();
        rumbleText = rumbleButton.GetComponentInChildren<Text>();
        sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
        quitButton.onClick.AddListener(Quit);
        resetButton.onClick.AddListener(Reset);

        pauseMenu.gameObject.SetActive(false);
        rumbleControl.gameObject.SetActive(false);
        paused = false;
        currentControlScheme = ControlScheme.MouseKeyboard45;
        controlSchemeButton.onClick.AddListener(OnClickControlScheme);
        rumbleButton.onClick.AddListener(OnClickRumble);
        controlSchemeText.text = "Mouse and Keyboard (MB 4 & 5)";
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
        switch (currentControlScheme) {
            case ControlScheme.MouseKeyboard45: {
                    currentControlScheme = ControlScheme.MouseKeyboardQE;
                    controlSchemeText.text = "Mouse and Keyboard (Keys Q & E)";
                    GamepadController.UsingMB45 = false;
                    break;
                }
            case ControlScheme.MouseKeyboardQE: {
                    currentControlScheme = ControlScheme.Gamepad;
                    controlSchemeText.text = "Gamepad";
                    GamepadController.UsingGamepad = true;
                    rumbleControl.gameObject.SetActive(true);
                    break;
                    //currentControlScheme = ControlScheme.MouseKeyboard45;
                    //controlSchemeText.text = "Mouse and Keyboard (MB 4 & 5)";
                    //break;
                }
            default: {
                    currentControlScheme = ControlScheme.MouseKeyboard45;
                    controlSchemeText.text = "Mouse and Keyboard (MB 4 & 5)";
                    GamepadController.UsingMB45 = true;
                    GamepadController.UsingGamepad = false;
                    rumbleControl.gameObject.SetActive(false);
                    break;
                }
        }
    }

    private void OnClickRumble() {
        if(GamepadController.UsingRumble) {
            rumbleText.text = "Disabled";
            GamepadController.UsingRumble = false;
        } else {
            rumbleText.text = "Enabled";
            GamepadController.UsingRumble = true;
        }
    }

    private void OnSensitivityChanged(float value) {
        FPVCameraLock.Sensitivity = value;
    }

    private void OnSmoothingChanged(float value) {
        FPVCameraLock.Smoothing = value;
    }

    private void Quit() {
        Application.Quit();
    }

    private void Reset() {
        UnPause();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

}
