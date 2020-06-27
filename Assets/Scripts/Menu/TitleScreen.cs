using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the Title Screen for the game, which appears on startup.
/// </summary>
public class TitleScreen : MonoBehaviour {

    #region fields
    private Button highlitButton;
    private Button playButton;
    private Button settingsButton;
    private Button articlesButton;
    private Button quitButton;
    private Button dataManagmentButton;
    #endregion

    private void Awake() {
        Button[] buttons = GetComponentsInChildren<Button>();
        playButton = transform.Find("PlayButton").GetComponent<Button>();
        settingsButton = transform.Find("SettingsButton").GetComponent<Button>();
        articlesButton = transform.Find("ArticlesButton").GetComponent<Button>();
        quitButton = transform.Find("QuitButton").GetComponent<Button>();
        dataManagmentButton = transform.Find("DataManagementButton").GetComponent<Button>();

        playButton.onClick.AddListener(OnClickedPlay);
        settingsButton.onClick.AddListener(OnClickedSettings);
        articlesButton.onClick.AddListener(OnClickedArticles);
        quitButton.onClick.AddListener(OnClickedQuit);
        dataManagmentButton.onClick.AddListener(OnClickedDataManagement);

        highlitButton = playButton;
        EventSystem.current.SetSelectedGameObject(highlitButton.gameObject);
    }

    public void Open() {
        if (!gameObject.activeSelf) {
            gameObject.SetActive(true);
            MainMenu.FocusOnButton(highlitButton);
        }
    }

    public void Close() {
        if (gameObject.activeSelf) {
            if (EventSystem.current.currentSelectedGameObject)
                highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            gameObject.SetActive(false);
        }
    }

    #region OnClicks
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
    private void OnClickedDataManagement() {
        MainMenu.OpenDataManagementScreen();
    }
    #endregion
}
