using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu, which displays the Title Screen, Scene Select Menu, etc.
/// </summary>
public class MainMenu : MonoBehaviour {

    private static MainMenu instance;
    private static Transform canvas;

    private ControlSchemeScreen controlSchemeScreen;
    private DataManagementScreen dataManagementScreen;
    private TitleScreen titleScreen;
    private SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;
    private ArticlesMenu articlesMenu;

    public static bool IsOpen => instance.gameObject.activeSelf;

    #region clearing
    private void Awake() {
        instance = this;
        canvas = GameObject.FindGameObjectWithTag("Canvas").transform;

        dataManagementScreen = GetComponentInChildren<DataManagementScreen>();
        controlSchemeScreen = GetComponentInChildren<ControlSchemeScreen>();
        titleScreen = GetComponentInChildren<TitleScreen>();
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();
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
        }
    }

    // Effectively starts the game over
    public static void Reset() {
        Open();
        if (FlagsController.GetData("controlSchemeChosen")) {
            instance.controlSchemeScreen.Close(false);
            OpenTitleScreen();
        } else {
            instance.titleScreen.Close();
            OpenControlSchemeScreen();
        }
    }
    #endregion

    private void Update() {
        if (Keybinds.ExitMenu() && !controlSchemeScreen.IsOpen) {
            if (sceneSelectMenu.IsOpen) {
                CloseSceneSelectMenu();
            } else if (articlesMenu.IsOpen) {
                CloseArticlesMenu();
            } else if (settingsMenu.IsOpen) {
                if (settingsMenu.BackAndSaveSettings())
                    OpenTitleScreen();
            } else if (dataManagementScreen.IsOpen) {
                dataManagementScreen.Close();
            } else {
                OpenSettingsMenu();
            }
        }
    }

    #region opening
    public static void Open() {
        instance.gameObject.SetActive(true);
    }

    public static void Close() {
        instance.gameObject.SetActive(false);
    }

    public static void OpenControlSchemeScreen() {
        instance.controlSchemeScreen.Open();
        FocusOnButton(instance.controlSchemeScreen.transform);
    }

    public static void OpenDataManagementScreen() {
        instance.titleScreen.Close();
        instance.dataManagementScreen.Open();
    }

    public static void OpenTitleScreen() {
        instance.titleScreen.Open();
    }

    public static void OpenSceneSelectMenu() {
        instance.titleScreen.Close();
        instance.sceneSelectMenu.Open();
    }
    public static void CloseSceneSelectMenu() {
        instance.sceneSelectMenu.Close();
    }

    public static void OpenArticlesMenu() {
        instance.titleScreen.Close();
        instance.articlesMenu.Open();
    }
    public static void CloseArticlesMenu() {
        instance.articlesMenu.Close();
    }

    public static void OpenSettingsMenu() {
        instance.titleScreen.Close();
        instance.settingsMenu.Open();
    }
    public static void CloseSettingsMenu() {
        instance.settingsMenu.Close();
        instance.titleScreen.Open();
    }
    #endregion

    // Finds the first button, slider, etc. in the newly opened menu for gamepad and keyboard control of the menu
    // Works for Title Screen and Scene Select Menu
    public static void FocusOnButton(Button button) {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
    public static void FocusOnButton(Transform parent) {
        EventSystem.current.SetSelectedGameObject(parent.GetComponentInChildren<Selectable>().gameObject);
    }
}
