using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour {

    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    private void Awake() {
        Button[] buttons = GetComponentsInChildren<Button>();
        playButton = buttons[0];
        settingsButton = buttons[1];
        quitButton = buttons[2];

        playButton.onClick.AddListener(OnClickedPlay);
        settingsButton.onClick.AddListener(OnClickedSettings);
        quitButton.onClick.AddListener(OnClickedQuit);
    }

    private void OnClickedPlay() {
        MainMenu.OpenSceneSelectMenu();
        Close();
    }

    private void OnClickedSettings() {
        MainMenu.OpenSettingsMenu();
        Close();
    }

    private void OnClickedQuit() {
        Application.Quit();
    }

    public void Open() {
        gameObject.SetActive(true);
    }

    public void Close() {
        gameObject.SetActive(false);
    }
}
