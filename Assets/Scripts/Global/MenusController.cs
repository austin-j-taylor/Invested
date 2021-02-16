using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the menus that the player may see.
/// Some menus are instead in MainMenu.
/// </summary>
public class MenusController : MonoBehaviour {

    public LevelCompletedMenu levelCompletedMenu;
    public PauseMenu pauseMenu;
    public SettingsMenu settingsMenu;
    public MainMenu mainMenu;

    public bool IsAnyMenuOpen => levelCompletedMenu.IsOpen || false;

    private void Awake() {

        levelCompletedMenu = GameManager.Canvas.GetComponentInChildren<LevelCompletedMenu>();
        pauseMenu = GameManager.Canvas.GetComponentInChildren<PauseMenu>();
        settingsMenu = GameManager.Canvas.GetComponentInChildren<SettingsMenu>();
        mainMenu = GameManager.Canvas.GetComponentInChildren<MainMenu>();
        mainMenu = GameManager.Canvas.GetComponentInChildren<MainMenu>();
    }

    // Close certain menus when they're open and the player hits Escape
    private void LateUpdate() {
        if (Keybinds.ExitMenu()) { // B or Escape
            if(HUD.ConsoleController.IsOpen) {
                HUD.ConsoleController.Close();
            } else if(pauseMenu.IsOpen) {
                if (settingsMenu.IsOpen) {
                    settingsMenu.BackAndSaveSettings();
                    //if (settingsMenu.BackAndSaveSettings())
                    //    pauseMenu.Open();
                } else {
                    pauseMenu.Close();
                }
            } else if(Player.CanPause && Keybinds.EscapeDown()) {
                pauseMenu.Open();
            }
        }
    }
}
