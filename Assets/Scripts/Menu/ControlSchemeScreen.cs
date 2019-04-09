using UnityEngine;
using UnityEngine.UI;
/*
 * When the game is opened for the first time, this screen opens to ask the user for their control scheme.
 */
public class ControlSchemeScreen : MonoBehaviour {


    private Button mouseKeyboardButton;
    private Button gamepadButton;
    private Button mkEQButton;
    private Button mk54Button;

    public static FlagsController flagsData;

    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }

    private void Awake() {
        flagsData = gameObject.AddComponent<FlagsController>();

        Button[] buttons = GetComponentsInChildren<Button>();
        mouseKeyboardButton = buttons[0];
        gamepadButton = buttons[1];
        mkEQButton = buttons[2];
        mk54Button = buttons[3];
        
        mouseKeyboardButton.onClick.AddListener(OnClickedMouseKeyboard);
        gamepadButton.onClick.AddListener(OnClickedGamepad);
        mkEQButton.onClick.AddListener(OnClickedMKEQ);
        mk54Button.onClick.AddListener(OnClickedMK54);
        mkEQButton.gameObject.SetActive(false);
        mk54Button.gameObject.SetActive(false);
    }

    public void Open() {
        gameObject.SetActive(true);
    }

    public void Close() {
        SettingsMenu.settingsData.SaveSettings();
        FlagsController.ControlSchemeChosen = true;
        gameObject.SetActive(false);
        MainMenu.OpenTitleScreen();
    }


    public void OnClickedMouseKeyboard() {
        mouseKeyboardButton.gameObject.SetActive(false);
        gamepadButton.gameObject.SetActive(false);
        mkEQButton.gameObject.SetActive(true);
        mk54Button.gameObject.SetActive(true);
        MainMenu.FocusOnCurrentMenu(transform);
    }
    public void OnClickedGamepad() {
        SettingsMenu.settingsData.controlScheme = SettingsData.Gamepad;
        Close();
    }
    public void OnClickedMKEQ() {
        SettingsMenu.settingsData.controlScheme = SettingsData.MKEQ;
        Close();
    }
    public void OnClickedMK54() {
        SettingsMenu.settingsData.controlScheme = SettingsData.MK54;
        Close();
    }
}
