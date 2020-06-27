using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the Settings Menu, where changing settings are saved to disk.
/// </summary>
public class SettingsMenu : MonoBehaviour {

    #region properties
    public bool IsOpen => gameObject.activeSelf;
    public bool AreHeadersClosed => settingsHeader.gameObject.activeSelf;
    public bool IsGlossaryOpen => glossaryHeader.gameObject.activeSelf;
    public bool IsVideoOpen => videoHeader.gameObject.activeSelf;
    public bool IsGameplayOpen => gameplayHeader.gameObject.activeSelf;
    public bool IsInterfaceOpen => interfaceHeader.gameObject.activeSelf;
    public bool IsGraphicsOpen => graphicsHeader.gameObject.activeSelf;
    public bool IsAudioOpen => audioHeader.gameObject.activeSelf;
    public bool IsAllomancyOpen => allomancyHeader.gameObject.activeSelf;
    public bool IsWorldOpen => worldHeader.gameObject.activeSelf;
    #endregion

    private Button highlitButton; // The currently selected button. Hitting space/enter/A etc. will submit it.

    #region fields
    // Settings
    private Text titleText;
    private Text tooltipText;
    private Transform settingsHeader;
    private Button glossaryButton, videoButton, gameplayButton, interfaceButton, graphicsButton, audioButton, allomancyButton, worldButton;
    private static Transform glossaryHeader, videoHeader, gameplayHeader, interfaceHeader, graphicsHeader, audioHeader, allomancyHeader,worldHeader;
    private Button closeButton;
    private Text closeText;
    private Button discardButton;
    private Button resetToDefaultsButton;
    private Text resetToDefaultsText;
    #endregion

    private Setting[] settings; // All the settings in the menu

    public static SettingsData settingsData; // The values corresponding to each setting

    #region clearing
    void Awake() {
        settings = GetComponentsInChildren<Setting>();
        settingsData = EventSystem.current.GetComponent<SettingsData>();

        // Settings Header
        titleText = transform.Find("TitleText").GetComponent<Text>();
        settingsHeader = transform.Find("SettingsHeader");
        tooltipText = settingsHeader.Find("Tooltip").GetComponent<Text>();
        glossaryButton = settingsHeader.Find("GlossaryButton").GetComponent<Button>();
        videoButton = settingsHeader.Find("VideoButton").GetComponent<Button>();
        gameplayButton = settingsHeader.Find("GameplayButton").GetComponent<Button>();
        interfaceButton = settingsHeader.Find("InterfaceButton").GetComponent<Button>();
        graphicsButton = settingsHeader.Find("GraphicsButton").GetComponent<Button>();
        audioButton = settingsHeader.Find("AudioButton").GetComponent<Button>();
        allomancyButton = settingsHeader.Find("AllomancyButton").GetComponent<Button>();
        worldButton = settingsHeader.Find("WorldButton").GetComponent<Button>();
        glossaryHeader = transform.Find("GlossaryHeader");
        videoHeader = transform.Find("VideoHeader");
        gameplayHeader = transform.Find("GameplayHeader");
        interfaceHeader = transform.Find("InterfaceHeader");
        graphicsHeader = transform.Find("GraphicsHeader");
        audioHeader = transform.Find("AudioHeader");
        allomancyHeader = transform.Find("AllomancyHeader");
        worldHeader = transform.Find("WorldHeader");
        closeButton = transform.Find("CloseButton").GetComponent<Button>();

        closeText = closeButton.GetComponentInChildren<Text>();
        discardButton = transform.Find("Discardbutton").GetComponent<Button>();
        resetToDefaultsButton = transform.Find("ResetToDefaultsButton").GetComponent<Button>();
        resetToDefaultsText = resetToDefaultsButton.GetComponentInChildren<Text>();

        // Command listeners assignment
        glossaryButton.onClick.AddListener(OpenGlossary);
        videoButton.onClick.AddListener(OpenVideo);
        gameplayButton.onClick.AddListener(OpenGameplay);
        interfaceButton.onClick.AddListener(OpenInterface);
        graphicsButton.onClick.AddListener(OpenGraphics);
        audioButton.onClick.AddListener(OpenAudio);
        allomancyButton.onClick.AddListener(OpenAllomancy);
        worldButton.onClick.AddListener(OpenWorld);
        closeButton.onClick.AddListener(OnClickClose);
        discardButton.onClick.AddListener(OnClickDiscard);
        resetToDefaultsButton.onClick.AddListener(OnClickResetToDefaults);
        // Initial field assignments
        titleText.text = "Settings";
        closeText.text = "Back";
        resetToDefaultsText.text = "Reset to Defaults";
    }

    private void Start() {
        // Refresh all settings after they've been loaded
        RefreshSettings();

        discardButton.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(false);
        highlitButton = gameplayButton;

        Close();
    }
    public void Open() {
        gameObject.SetActive(true);
        resetToDefaultsButton.gameObject.SetActive(true);
        tooltipText.text = "";
        MainMenu.FocusOnButton(highlitButton);
    }

    public void Close() {
        if (IsOpen) {
            if (EventSystem.current.currentSelectedGameObject != null)
                highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
            resetToDefaultsText.text = "Reset to Defaults";
            resetToDefaultsButton.gameObject.SetActive(false);
            CloseGlossary();
            CloseVideo();
            CloseInterface();
            CloseGraphics();
            CloseGameplay();
            CloseAudio();
            CloseAllomancy();
            CloseWorld();
            gameObject.SetActive(false);
            if (!PauseMenu.IsPaused) {
                MainMenu.OpenTitleScreen();
            } else {
                PauseMenu.Open();
            }
        }
    }
    #endregion

    #region refreshing
    // Refresh Settings and Refresh Particular Settings to update button texts
    private void RefreshSettings() {
        foreach (Setting setting in settings) {
            setting.RefreshData();
            setting.RefreshText();
        }
    }
    public static void RefreshSettingHelpOverlay(int newHelpOverlay) {
        settingsData.helpOverlay = newHelpOverlay;
        ButtonSetting helpOverlay = interfaceHeader.Find("HUDLabel/Children/HelpOverlayLabel").GetComponent<ButtonSetting>();
        helpOverlay.RefreshData();
        helpOverlay.RefreshText();
        settingsData.SaveSettings();
    }
    public static void RefreshSettingPerspective(int newCameraFirstPerson) {
        settingsData.cameraFirstPerson = newCameraFirstPerson;
        ButtonSetting perspective = gameplayHeader.Find("PerspectiveLabel").GetComponent<ButtonSetting>();
        perspective.RefreshData();
        perspective.RefreshText();
        settingsData.SaveSettings();
    }
    #endregion

    #region headerClearing
    private void OpenHeader() {
        highlitButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        resetToDefaultsText.text = "Reset to Defaults";
        settingsHeader.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(true);
        resetToDefaultsButton.gameObject.SetActive(false);
        closeText.text = "Save & Back";
        MainMenu.FocusOnButton(transform);
    }
    private void CloseHeader() {
        if (!settingsHeader.gameObject.activeSelf) {
            settingsHeader.gameObject.SetActive(true);
            discardButton.gameObject.SetActive(false);
            resetToDefaultsButton.gameObject.SetActive(true);
            closeText.text = "Back";
            MainMenu.FocusOnButton(highlitButton);
        }
    }

    private void OpenGlossary() {
        titleText.text = "Glossary";
        glossaryHeader.gameObject.SetActive(true);
        settingsHeader.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(false);
        MainMenu.FocusOnButton(glossaryHeader);
    }
    private void CloseGlossary() {
        titleText.text = "Settings";
        glossaryHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenVideo() {
        titleText.text = "Video";
        videoHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseVideo() {
        titleText.text = "Settings";
        videoHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenGameplay() {
        titleText.text = "Gameplay";
        gameplayHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseGameplay() {
        titleText.text = "Settings";
        gameplayHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenInterface() {
        titleText.text = "Interface";
        interfaceHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseInterface() {
        titleText.text = "Settings";
        interfaceHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenGraphics() {
        titleText.text = "Graphics";
        graphicsHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseGraphics() {
        titleText.text = "Settings";
        graphicsHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenAudio() {
        titleText.text = "Audio";
        audioHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseAudio() {
        titleText.text = "Settings";
        audioHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenAllomancy() {
        titleText.text = "Allomancy Physics";
        allomancyHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseAllomancy() {
        titleText.text = "Settings";
        allomancyHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenWorld() {
        titleText.text = "World Physics";
        worldHeader.gameObject.SetActive(true);
        OpenHeader();
    }
    private void CloseWorld() {
        titleText.text = "Settings";
        worldHeader.gameObject.SetActive(false);
        CloseHeader();
    }
    #endregion

    #region OnClicks
    public void SetTooltip(string tip) {
        tooltipText.text = tip;
    }

    // Returns true if calling this returns to the Title Screen
    public bool BackAndSaveSettings() {
        if (!AreHeadersClosed)
            settingsData.SaveSettings();

        return BackSettings();
    }

    // Returns true if calling this returns to the Title Screen or Pause Menu
    private bool BackSettings() {
        if (IsGlossaryOpen)
            CloseGlossary();
        else if (IsVideoOpen)
            CloseVideo();
        else if (IsGameplayOpen)
            CloseGameplay();
        else if (IsInterfaceOpen)
            CloseInterface();
        else if (IsGraphicsOpen)
            CloseGraphics();
        else if (IsAudioOpen)
            CloseAudio();
        else if (IsAllomancyOpen)
            CloseAllomancy();
        else if (IsWorldOpen)
            CloseWorld();
        else {
            Close();
            return true;
        }
        return false;
    }

    private void OnClickClose() {
        BackAndSaveSettings();
    }

    private void OnClickDiscard() {
        settingsData.LoadSettings();
        RefreshSettings();
        BackSettings();
    }

    private void OnClickResetToDefaults() {
        resetToDefaultsText.text = "Settings reset.";
        settingsData.ResetToDefaults();
        RefreshSettings();
    }
    #endregion
}