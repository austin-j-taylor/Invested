using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    private static MainMenu instance;
    
    private TitleScreen titleScreen;
    private SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;

    private void Awake() {
        instance = this;
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
    }

    public static void Close() {
        instance.gameObject.SetActive(false);
    }

    public static void OpenTitleScreen() {
        CloseSettingsMenu();
        instance.titleScreen.Open();
    }

    public static void CloseTitleScreen() {
        instance.titleScreen.Close();
    }

    public static void OpenSceneSelectMenu() {
        CloseTitleScreen();
        instance.sceneSelectMenu.Open();
    }

    public static void CloseSceneSelectMenu() {
        instance.sceneSelectMenu.Close();
    }

    public static void OpenSettingsMenu() {
        instance.settingsMenu.Open();
    }

    public static void CloseSettingsMenu() {
        instance.settingsMenu.Close();
    }
}
