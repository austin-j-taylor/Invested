using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// When the game is opened for the first time, this screen opens to ask the user for their control scheme.
/// </summary>
public class ControlSchemeMenu : Menu {

    private Button mouseKeyboardButton;
    private Button gamepadButton;
    private Button mkEQButton;
    private Button mk54Button;

    private void Awake() {
        Button[] buttons = GetComponentsInChildren<Button>();
        mouseKeyboardButton = buttons[0];
        gamepadButton = buttons[1];
        mkEQButton = buttons[2];
        mk54Button = buttons[3];

        mouseKeyboardButton.onClick.AddListener(OnClickedMouseKeyboard);
        gamepadButton.onClick.AddListener(OnClickedGamepad);
        mkEQButton.onClick.AddListener(OnClickedMKEQ);
        mk54Button.onClick.AddListener(OnClickedMK54);
    }

    public override void Open() {
        base.Open();
        mouseKeyboardButton.gameObject.SetActive(true);
        gamepadButton.gameObject.SetActive(true);
        mkEQButton.gameObject.SetActive(false);
        mk54Button.gameObject.SetActive(false);
        MainMenu.FocusOnButton(transform);
    }

    public void Close(bool refreshing) {
        if (refreshing) {
            SettingsMenu.settingsGameplay.SaveSettings();
            //Messages.Refresh();
            HUD.UpdateText();
            FlagsController.SetFlag("controlSchemeChosen");
        }
        base.Close();
        GameManager.MenusController.mainMenu.titleScreen.Open();
    }

    #region OnClicks
    public void OnClickedMouseKeyboard() {
        mouseKeyboardButton.gameObject.SetActive(false);
        gamepadButton.gameObject.SetActive(false);
        mkEQButton.gameObject.SetActive(true);
        mk54Button.gameObject.SetActive(true);
        MainMenu.FocusOnButton(transform);
    }
    public void OnClickedGamepad() {
        SettingsMenu.settingsGameplay.controlScheme = JSONSettings_Gameplay.Gamepad;
        SettingsMenu.settingsGameplay.RefreshSettings();
        Close(true);
    }
    public void OnClickedMKEQ() {
        SettingsMenu.settingsGameplay.controlScheme = JSONSettings_Gameplay.MKEQ;
        SettingsMenu.settingsGameplay.RefreshSettings();
        Close(true);
    }
    public void OnClickedMK54() {
        SettingsMenu.settingsGameplay.controlScheme = JSONSettings_Gameplay.MK54;
        SettingsMenu.settingsGameplay.RefreshSettings();
        Close(true);
    }
    #endregion
}
