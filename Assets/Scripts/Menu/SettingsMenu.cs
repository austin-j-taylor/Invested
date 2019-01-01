using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
    
    // String constants
    private const string s_settings = "Settings";
    private const string s_glossary = "Glossary";
    private const string s_gameplay = "Gameplay";
    private const string s_interface = "Interface";
    private const string s_graphics = "Graphics";
    private const string s_allomancy = "Allomancy Physics";
    private const string s_world = "World Physics";
    private const string s_back = "Back";
    private const string s_save = "Save & Back";
    private const string s_reset = "Reset to Defaults";
    private const string s_reset_confirmed = "Settings reset.";
    
    public bool IsOpen {
        get {
            return gameObject.activeSelf;
        }
    }
    public bool AreHeadersClosed {
        get {
            return settingsHeader.gameObject.activeSelf;
        }
    }
    public bool IsGlossaryOpen {
        get {
            return glossaryHeader.gameObject.activeSelf;
        }
    }
    public bool IsGameplayOpen {
        get {
            return gameplayHeader.gameObject.activeSelf;
        }
    }
    public bool IsInterfaceOpen {
        get {
            return interfaceHeader.gameObject.activeSelf;
        }
    }
    public bool IsGraphicsOpen {
        get {
            return graphicsHeader.gameObject.activeSelf;
        }
    }
    public bool IsAllomancyOpen {
        get {
            return allomancyHeader.gameObject.activeSelf;
        }
    }
    public bool IsWorldOpen {
        get {
            return worldHeader.gameObject.activeSelf;
        }
    }

    // Settings
    private Text titleText;
    private Text tooltipText;
    private Transform settingsHeader;
    private Button glossaryButton;
    private Button gameplayButton;
    private Button interfaceButton;
    private Button graphicsButton;
    private Button allomancyButton;
    private Button worldButton;
    private Transform glossaryHeader;
    private Transform gameplayHeader;
    private Transform interfaceHeader;
    private Transform graphicsHeader;
    private Transform allomancyHeader;
    private Transform worldHeader;
    private Button closeButton;
    private Text closeText;
    private Button discardButton;
    private Button resetToDefaultsButton;
    private Text resetToDefaultsText;

    private Setting[] settings;

    public static SettingsData settingsData;

    void Awake() {
        settings = GetComponentsInChildren<Setting>();
        settingsData = gameObject.AddComponent<SettingsData>();

        // Settings Header
        titleText = transform.Find("TitleText").GetComponent<Text>();
        settingsHeader = transform.Find("SettingsHeader");
        tooltipText = settingsHeader.Find("Tooltip").GetComponent<Text>();
        Button[] settingsHeaderButtons = settingsHeader.GetComponentsInChildren<Button>();
        glossaryButton = settingsHeaderButtons[0];
        gameplayButton = settingsHeaderButtons[1];
        interfaceButton = settingsHeaderButtons[2];
        graphicsButton = settingsHeaderButtons[3];
        allomancyButton = settingsHeaderButtons[4];
        worldButton = settingsHeaderButtons[5];
        // Glossary
        glossaryHeader = transform.Find("GlossaryHeader");
        // Gameplay Header
        gameplayHeader = transform.Find("GameplayHeader");
        // Interface Header
        interfaceHeader = transform.Find("InterfaceHeader");
        // Graphics Header
        graphicsHeader = transform.Find("GraphicsHeader");
        // Allomancy Header
        allomancyHeader = transform.Find("AllomancyHeader");
        // World Header
        worldHeader = transform.Find("WorldHeader");
        // Close Button
        closeButton = transform.Find("CloseButton").GetComponent<Button>();

        closeText = closeButton.GetComponentInChildren<Text>();
        discardButton = transform.Find("Discardbutton").GetComponent<Button>();
        resetToDefaultsButton = transform.Find("ResetToDefaultsButton").GetComponent<Button>();
        resetToDefaultsText = resetToDefaultsButton.GetComponentInChildren<Text>();

        // Command listeners assignment
        glossaryButton.onClick.AddListener(OpenGlossary);
        gameplayButton.onClick.AddListener(OpenGameplay);
        interfaceButton.onClick.AddListener(OpenInterface);
        graphicsButton.onClick.AddListener(OpenGraphics);
        allomancyButton.onClick.AddListener(OpenAllomancy);
        worldButton.onClick.AddListener(OpenWorld);
        closeButton.onClick.AddListener(OnClickClose);
        discardButton.onClick.AddListener(OnClickDiscard);
        resetToDefaultsButton.onClick.AddListener(OnClickResetToDefaults);
        // Initial field assignments
        titleText.text = s_settings;
        closeText.text = s_back;
        resetToDefaultsText.text = s_reset;
    }

    private void Start() {
        // Refresh all settings after they've been loaded
        RefreshSettings();

        discardButton.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(false);
    }

    private void RefreshSettings() {
        foreach (Setting setting in settings) {
            setting.RefreshData();
            setting.RefreshText();
        }
    }

    public void Open() {
        gameObject.SetActive(true);
        resetToDefaultsButton.gameObject.SetActive(true);
    }

    public void Close() {
        resetToDefaultsText.text = s_reset;
        resetToDefaultsButton.gameObject.SetActive(false);
        CloseGlossary();
        CloseInterface();
        CloseGraphics();
        CloseGameplay();
        CloseAllomancy();
        CloseWorld();
        gameObject.SetActive(false);
    }

    private void OpenHeader() {
        resetToDefaultsText.text = s_reset;
        settingsHeader.gameObject.SetActive(false);
        discardButton.gameObject.SetActive(true);
        resetToDefaultsButton.gameObject.SetActive(false);
        closeText.text = s_save;
    }

    private void CloseHeader() {
        settingsHeader.gameObject.SetActive(true);
        discardButton.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(true);
        closeText.text = s_back;
    }

    private void OpenGlossary() {
        titleText.text = s_glossary;
        glossaryHeader.gameObject.SetActive(true);
        settingsHeader.gameObject.SetActive(false);
        resetToDefaultsButton.gameObject.SetActive(false);
    }

    private void CloseGlossary() {
        titleText.text = s_settings;
        glossaryHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenGameplay() {
        titleText.text = s_gameplay;
        gameplayHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseGameplay() {
        titleText.text = s_settings;
        gameplayHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenInterface() {
        titleText.text = s_interface;
        interfaceHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseInterface() {
        titleText.text = s_settings;
        interfaceHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenGraphics() {
        titleText.text = s_graphics;
        graphicsHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseGraphics() {
        titleText.text = s_settings;
        graphicsHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenAllomancy() {
        titleText.text = s_allomancy;
        allomancyHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseAllomancy() {
        titleText.text = s_settings;
        allomancyHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    private void OpenWorld() {
        titleText.text = s_world;
        worldHeader.gameObject.SetActive(true);
        OpenHeader();
    }

    private void CloseWorld() {
        titleText.text = s_settings;
        worldHeader.gameObject.SetActive(false);
        CloseHeader();
    }

    public void SetTooltip(string tip) {
        tooltipText.text = tip;
    }

    // Returns true if calling this returns to the Title Screen
    public bool BackAndSaveSettings() {
        if (!AreHeadersClosed)
            settingsData.SaveSettings();
        return BackSettings();
    }

    // Returns true if calling this returns to the Title Screen
    private bool BackSettings() {
        if (IsGlossaryOpen)
            CloseGlossary();
        else if (IsGameplayOpen)
            CloseGameplay();
        else if (IsInterfaceOpen)
            CloseInterface();
        else if (IsGraphicsOpen)
            CloseGraphics();
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
        resetToDefaultsText.text = s_reset_confirmed;
        settingsData.ResetToDefaults();
        RefreshSettings();
    }
}