using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    private Image titleScreenBG;
    private SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;
    
    private void Start () {
        titleScreenBG = transform.parent.GetComponent<Image>();
        settingsMenu = transform.parent.parent.GetComponentInChildren<SettingsMenu>();
        sceneSelectMenu = transform.parent.GetComponentInChildren<SceneSelectMenu>();

        Button[] buttons = GetComponentsInChildren<Button>();
        playButton = buttons[0];
        settingsButton = buttons[1];
        quitButton = buttons[2];

        playButton.onClick.AddListener(OnClickedPlay);
        settingsButton.onClick.AddListener(OnClickedSettings);
        quitButton.onClick.AddListener(OnClickedQuit);

        // Set up the Player, Canvas, and EventSystem to persist between scenes
        //DontDestroyOnLoad(Player.PlayerInstance);
        DontDestroyOnLoad(transform.parent.parent.gameObject);
        DontDestroyOnLoad(GameObject.FindGameObjectWithTag("GameController"));

        Player.PlayerInstance.gameObject.SetActive(false);
        settingsMenu.CloseSettings();
        sceneSelectMenu.CloseSceneSelect();
        HUD.DisableHUD();
    }

    private void Update() {
        if(Keybinds.EscapeDown()) {
            if(sceneSelectMenu.IsOpen) {
                sceneSelectMenu.CloseSceneSelect();
                //OpenMainMenu();
            } else if(settingsMenu.IsOpen) {
                settingsMenu.BackAndSaveSettings();
            } else {
                settingsMenu.OpenSettings();
            }
        }
    }

    public void OpenMainMenu() {
        titleScreenBG.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void CloseMainMenu() {
        gameObject.SetActive(false);
    }

    private void OnClickedPlay() {
        sceneSelectMenu.OpenSceneSelect();
        //CloseMainMenu();
    }

    private void OnClickedSettings() {
        settingsMenu.OpenSettings();
    }

    private void OnClickedQuit() {
        Application.Quit();
    }
}
