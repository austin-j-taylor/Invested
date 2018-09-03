using UnityEngine;
using UnityEngine.UI;


public class ButtonSetting : Setting {
    private int data;

    protected bool hasChildren;
    protected Setting[] children;
    private Button button;
    private Text buttonText;
    private Text detailsText;
    private bool hasDetails;
    private int optionsCount = 2; // one-based
    // Assigned in inspector
    public string[] buttonStrings;
    public string[] detailStrings;

    /*
     * Example Heirarchy:
     * 
     * ButtonSetting
     *      Button
     *          0
     *              ButtonChild0
     *              ButtonChild1
     *          1
     *              ButtonChild0
     *          2
     *              ButtonChild0
     *              ButtonChild1
     * 
     * 
     */

    void Awake() {

        button = GetComponentInChildren<Button>();
        buttonText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        detailsText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        optionsCount = buttonStrings.Length;
        hasDetails = detailStrings != null;

        if(optionsCount < 2) {
            Debug.LogError("Not enough options provided for ButtonSetting");
        }
        if(hasDetails) {
            if (detailStrings.Length != buttonStrings.Length)
                Debug.LogError("buttonStrings and detailStrings do not match size.");
        } else {

        }
        
        // Get children
        Setting[] includingThis = GetComponentsInChildren<Setting>();
        if (includingThis.Length > 1) {
            Setting[] excludingThis = new Setting[includingThis.Length - 1];
            for (int i = 0; i < excludingThis.Length; i++)
                excludingThis[i] = includingThis[i + 1];
            children = includingThis;
        }
        hasChildren = children != null;

        button.onClick.AddListener(OnClick);
    }

    /*
     * Checks SettingsData for this button's data
     */
    public override void RefreshData() {
        data = SettingsMenu.settingsData.GetDataInt(id);
    }
    
    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public override void RefreshText() {
        buttonText.text = buttonStrings[data];
        if(hasDetails)
            detailsText.text = detailStrings[data];
        if(hasChildren) {
            for(int i = 0; i < optionsCount; i++) {
                if(i == data) { // This option is active. Enable all children.
                    if (i + 1 < transform.childCount) {
                        Transform header = transform.GetChild(i + 1);
                        header.gameObject.SetActive(true);
                        foreach (Setting child in header.GetComponentsInChildren<Setting>()) {
                            child.RefreshText();
                        }
                    }
                } else { // This is not the active option. Disable all children.
                    if (i + 1 < transform.childCount) {
                        transform.GetChild(i + 1).gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void OnClick() {
        data++;
        if (data >= optionsCount) {
            data = 0;
        }
        SettingsMenu.settingsData.SetData(id, data);
        RefreshText();
    }
}