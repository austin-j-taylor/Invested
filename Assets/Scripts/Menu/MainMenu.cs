using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    SettingsMenu settingsMenu;

    // Use this for initialization
    private void Start () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>(); ;

        Button[] buttons = GetComponentsInChildren<Button>();
        playButton = buttons[0];
        settingsButton = buttons[1];
        quitButton = buttons[2];

        playButton.onClick.AddListener(OnClickedPlay);
        settingsButton.onClick.AddListener(OnClickedSettings);
        quitButton.onClick.AddListener(OnClickedQuit);
    }

    private void OnClickedPlay() {

    }

    private void OnClickedSettings() {
        settingsMenu.OpenSettings();
    }

    private void OnClickedQuit() {
        Application.Quit();
    }
}
