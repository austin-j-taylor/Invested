using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour {

    private static MainMenu instance;
    private static Transform canvas;
    private static EventSystem eventSystem;

    private TitleScreen titleScreen;
    private SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;

    public static bool IsOpen {
        get {
            return instance.gameObject.activeSelf;
        }
    }

    private void Awake() {
        instance = this;
        canvas = GameObject.FindGameObjectWithTag("Canvas").transform;
        eventSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<EventSystem>();

        titleScreen = GetComponentInChildren<TitleScreen>();
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();
        sceneSelectMenu = GetComponentInChildren<SceneSelectMenu>();

        // Set up the Player, Canvas, and EventSystem to persist between scenes
        //DontDestroyOnLoad(Player.PlayerInstance);
        DontDestroyOnLoad(transform.parent.gameObject);
        DontDestroyOnLoad(GameObject.FindGameObjectWithTag("GameController"));
    }

    private void Start () {
        Player.PlayerInstance.gameObject.SetActive(false);
        settingsMenu.Close();
        sceneSelectMenu.Close();
        HUD.DisableHUD();
        
        Open();
    }

    private void Update() {
        if(Keybinds.EscapeDown()) {
            if(sceneSelectMenu.IsOpen) {
                CloseSceneSelectMenu();
            } else if(settingsMenu.IsOpen) {
                if (settingsMenu.BackAndSaveSettings())
                    OpenTitleScreen();
            } else {
                OpenSettingsMenu();
            }
        }
    }

    public static void Open() {
        instance.gameObject.SetActive(true);
        FocusOnCurrentMenu(instance.transform);
    }

    public static void Close() {
        instance.gameObject.SetActive(false);
    }

    public static void OpenTitleScreen() {
        instance.titleScreen.Open();
        FocusOnCurrentMenu(instance.titleScreen.transform);
    }

    public static void OpenSceneSelectMenu() {
        instance.titleScreen.Close();
        instance.sceneSelectMenu.Open();
    }

    public static void CloseSceneSelectMenu() {
        instance.sceneSelectMenu.Close();
        //instance.titleScreen.Open();
    }

    public static void OpenSettingsMenu() {
        instance.titleScreen.Close();
        instance.settingsMenu.Open();
    }

    public static void CloseSettingsMenu() {
        instance.settingsMenu.Close();
        instance.titleScreen.Open();
    }

    // Finds the first button, slider, etc. in the newly opened menu for gamepad and keyboard control of the menu
    // Works for Title Screen and Scene Select Menu
    public static void FocusOnCurrentMenu(Transform parent) {
        Selectable child = parent.GetComponentInChildren<Selectable>();
        if(child)
            eventSystem.SetSelectedGameObject(child.gameObject);
    }
}
