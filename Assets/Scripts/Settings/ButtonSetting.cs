using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
 * Represents a setting that is controlled by a button.
 * In the inspector, assign the text that appears with each setting, both on the button and
 *      in the details field.
 */
public class ButtonSetting : Setting {
    private int data;
    public UnityEvent extraCall;

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

        // Assign font and button size
        RectTransform rectButton = button.GetComponent<RectTransform>();
        RectTransform rectDetails = detailsText.GetComponent<RectTransform>();
        Vector2 right = rectButton.offsetMax;
        Vector2 left = rectDetails.offsetMax;
        left.x = 420 - settingSize;
        right.x = settingSize;
        rectButton.offsetMax = right;
        rectDetails.offsetMax = left;

        detailsText.fontSize = detailsFontSize;

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
        extraCall.Invoke();
        SettingsMenu.settingsData.SetData(id, data);
        RefreshText();
    }

    protected virtual void SetFontSize() {

    }
}