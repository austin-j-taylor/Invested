using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu, which displays the Title Screen, Scene Select Menu, etc.
/// </summary>
public class MainMenu : Menu {

    public TitleScreen titleScreen;
    public ControlSchemeMenu controlSchemeMenu;
    public DataManagementMenu dataManagementMenu;
    public SceneSelectMenu sceneSelectMenu;
    public ArticlesMenu articlesMenu;

    #region clearing
    private void Awake() {
        controlSchemeMenu = GetComponentInChildren<ControlSchemeMenu>();
        dataManagementMenu = GetComponentInChildren<DataManagementMenu>();
        titleScreen = GetComponentInChildren<TitleScreen>();
        sceneSelectMenu = GetComponentInChildren<SceneSelectMenu>();
        articlesMenu = GetComponentInChildren<ArticlesMenu>();

        SceneManager.sceneLoaded += ClearAfterSceneChange;
        // Set up the Player, Canvas, and EventSystem to persist between scenes
        //DontDestroyOnLoad(Player.PlayerInstance);
        DontDestroyOnLoad(transform.parent.gameObject);
        DontDestroyOnLoad(EventSystem.current);
    }

    private void Start() {
        Reset();
    }
    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (scene.buildIndex == SceneSelectMenu.sceneTitleScreen) {
            Open();
            if (sceneSelectMenu.IsOpen)
                titleScreen.Close();
            if (sceneSelectMenu.isActiveAndEnabled)
                FocusOnButton(sceneSelectMenu.HighlitButton);
        } else {
            Close();
        }
    }

    // Effectively starts the game over
    public void Reset() {
        Open();
        if (FlagsController.GetData("controlSchemeChosen")) {
            controlSchemeMenu.Close(false);
        } else {
            titleScreen.Close();
            controlSchemeMenu.Open();
        }
    }
    #endregion

    private void Update() {
        if (Keybinds.ExitMenu() && !controlSchemeMenu.IsOpen) {
            if (sceneSelectMenu.IsOpen) {
                sceneSelectMenu.Close();
            } else if (articlesMenu.IsOpen) {
                articlesMenu.Close();
            } else if (dataManagementMenu.IsOpen) {
                dataManagementMenu.Close();
            } else if (GameManager.MenusController.settingsMenu.IsOpen) {
                GameManager.MenusController.settingsMenu.BackAndSaveSettings();
                //if (GameManager.MenusController.settingsMenu.BackAndSaveSettings())
                //    titleScreen.Open();
            } else {
                GameManager.MenusController.settingsMenu.Open();
                titleScreen.Close();
            }
        }
    }


    // Finds the first button, slider, etc. in the newly opened menu for gamepad and keyboard control of the menu
    // Works for Title Screen and Scene Select Menu
    public static void FocusOnButton(Button button) {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
    public static void FocusOnButton(Transform parent) {
        EventSystem.current.SetSelectedGameObject(parent.GetComponentInChildren<Selectable>().gameObject);
    }
}
