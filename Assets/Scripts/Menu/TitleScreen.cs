using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour {

    private Button highlitButton;

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

        highlitButton = playButton;
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
        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
            MainMenu.FocusOnButton(highlitButton);
        }
    }

    public void Close() {
        if(gameObject.activeSelf) {
            highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            gameObject.SetActive(false);
        }
    }
}
