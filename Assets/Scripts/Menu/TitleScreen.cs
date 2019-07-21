using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour {

    private Button playButton;
    private Button settingsButton;
    private Button articlesButton;
    private Button quitButton;

    private void Awake() {
        Button[] buttons = GetComponentsInChildren<Button>();
        playButton = buttons[0];
        settingsButton = buttons[1];
        articlesButton = buttons[2];
        quitButton = buttons[3];

        playButton.onClick.AddListener(OnClickedPlay);
        settingsButton.onClick.AddListener(OnClickedSettings);
        articlesButton.onClick.AddListener(OnClickedArticles);
        quitButton.onClick.AddListener(OnClickedQuit);
    }

    private void OnClickedPlay() {
        MainMenu.OpenSceneSelectMenu();
        Close();
    }

    private void OnClickedSettings() {
        MainMenu.OpenSettingsMenu();
    }

    private void OnClickedArticles() {
        MainMenu.OpenArticlesMenu();
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
