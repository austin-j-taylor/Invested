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

    private Transform canvas;

    public bool IsAnyMenuOpen => levelCompletedMenu.IsOpen || false;

    private void Awake() {
        canvas = GameObject.FindGameObjectWithTag("Canvas").transform;

        levelCompletedMenu = canvas.GetComponentInChildren<LevelCompletedMenu>();
        pauseMenu = canvas.GetComponentInChildren<PauseMenu>();
        settingsMenu = canvas.GetComponentInChildren<SettingsMenu>();
        mainMenu = canvas.GetComponentInChildren<MainMenu>();
    }

    // Close certain menus when they're open and the player hits Escape
    private void LateUpdate() {
        if (Keybinds.ExitMenu()) {
            if(pauseMenu.IsOpen) {
                if (settingsMenu.IsOpen) {
                    if (settingsMenu.BackAndSaveSettings())
                        pauseMenu.Open();
                } else {
                    pauseMenu.Close();
                }
            } else if(Player.CanPause) {
                pauseMenu.Open();
            }
        }
    }
}
