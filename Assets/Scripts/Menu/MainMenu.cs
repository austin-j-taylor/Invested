using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    private Image titleScreenBG;
    private SettingsMenu settingsMenu;
    private SceneSelectMenu sceneSelectMenu;

    // Use this for initialization
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

        DontDestroyOnLoad(transform.parent.parent.gameObject);
        DontDestroyOnLoad(GameObject.FindGameObjectWithTag("GameController"));
        DontDestroyOnLoad(GameObject.FindGameObjectWithTag("Player"));

    }

    public void OpenMenu() {
        titleScreenBG.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void CloseMenu() {
        gameObject.SetActive(false);
    }

    private void OnClickedPlay() {
        sceneSelectMenu.OpenMenu();
        CloseMenu();
    }

    private void OnClickedSettings() {
        settingsMenu.OpenSettings();
    }

    private void OnClickedQuit() {
        Application.Quit();
    }
}
