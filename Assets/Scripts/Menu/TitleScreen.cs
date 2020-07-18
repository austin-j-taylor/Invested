using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the Title Screen for the game, which appears on startup.
/// </summary>
public class TitleScreen : Menu {

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

    public override void Open() {
        if (!IsOpen) {
            base.Open();
            MainMenu.FocusOnButton(highlitButton);
        }
    }

    public override void Close() {
        if (IsOpen) {
            if (EventSystem.current.currentSelectedGameObject)
                highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            base.Close();
        }
    }

    #region OnClicks
    private void OnClickedPlay() {
        Close();
        GameManager.MenusController.mainMenu.sceneSelectMenu.Open();
    }
    private void OnClickedSettings() {
        Close();
        GameManager.MenusController.settingsMenu.Open();
    }
    private void OnClickedArticles() {
        Close();
        GameManager.MenusController.mainMenu.articlesMenu.Open();
    }
    private void OnClickedDataManagement() {
        Close();
        GameManager.MenusController.mainMenu.dataManagementMenu.Open();
    }

    private void OnClickedQuit() {
        Application.Quit();
    }
    #endregion
}
