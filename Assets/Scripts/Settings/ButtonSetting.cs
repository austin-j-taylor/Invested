using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Represents a setting that is controlled by a button.
/// In the inspector, assign the text that appears with each setting, both on the button and in the details field.
/// </summary>
public class ButtonSetting : Setting {
    private int data;
    public UnityEvent[] buttonCalls;

    private Button button;
    private Text buttonText;
    private Text detailsText;
    private bool hasDetails;
    private int optionsCount = 2; // one-based
    // Assigned in inspector
    [TextArea]
    public string[] buttonStrings;
    [TextArea]
    public string[] detailStrings;
    public Transform[] childrenHeaders;

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

    protected override void Awake() {
        base.Awake();
        button = GetComponentInChildren<Button>();
        buttonText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        detailsText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        optionsCount = buttonStrings.Length;
        hasDetails = detailStrings.Length != 0;

        if (optionsCount < 2) {
            Debug.LogError("Not enough options provided for ButtonSetting");
        }
        if (hasDetails) {
            if (detailStrings.Length != buttonStrings.Length)
                Debug.LogError("buttonStrings and detailStrings do not match size.");
        }

        // Assign font and button size
        RectTransform rectButton = button.GetComponent<RectTransform>();
        RectTransform rectDetails = detailsText.GetComponent<RectTransform>();
        Vector2 buttonRightSide = rectButton.offsetMax;
        Vector2 detailsRightSide = rectDetails.offsetMax;
        Vector2 detailsLeftSide = rectDetails.offsetMin;
        detailsRightSide.x = 840 + 240 - settingSize;
        buttonRightSide.x = settingSize;
        detailsLeftSide.x = 240 - settingSize + 30;
        rectButton.offsetMax = buttonRightSide;
        rectDetails.offsetMax = detailsRightSide;
        rectDetails.offsetMin = detailsLeftSide;

        detailsText.fontSize = detailsFontSize;

        button.onClick.AddListener(OnClick);
    }

    /*
     * Checks SettingsData for this button's data
     */
    public override void RefreshData() {
        data = parentSettings.GetDataInt(id);
        // Update other functions
        if (buttonCalls.Length > 0)
            if (buttonCalls[data] != null)
                buttonCalls[data].Invoke();
    }

    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public override void RefreshText() {
        buttonText.text = buttonStrings[data];
        if (hasDetails)
            detailsText.text = detailStrings[data];
        if (childrenHeaders.Length != 0) {
            for (int i = 0; i < optionsCount; i++) {
                if (i != data) { // This is not the active option. Disable all childrenHeaders.
                    if (i < childrenHeaders.Length && childrenHeaders[i]) {
                        childrenHeaders[i].gameObject.SetActive(false);
                    }
                }
            }
            if (data < childrenHeaders.Length && childrenHeaders[data]) { // This option is active. Enable all childrenHeaders of that index.
                childrenHeaders[data].gameObject.SetActive(true);
                Setting[] childrenForThisOption = childrenHeaders[data].GetComponentsInChildren<Setting>();
                foreach (Setting child in childrenForThisOption) {
                    child.RefreshText();
                }
            }
        }
    }

    public void OnClick() {
        data++;
        if (data >= optionsCount) {
            data = 0;
        }

        parentSettings.SetData(id, data);

        // Update other functions
        if (buttonCalls.Length > 0)
            if (buttonCalls[data] != null)
                buttonCalls[data].Invoke();
        RefreshText();
    }
}