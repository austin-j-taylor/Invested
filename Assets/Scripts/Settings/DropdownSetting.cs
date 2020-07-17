using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static UnityEngine.UI.Dropdown;
using System.Linq;

/// <summary>
/// Represents a setting that is controlled by a dropdown menu, powered by TextMesh Pro.
/// Functionally similar to a button that cycles through options, except each option appears in the dropdown menu.
/// </summary>
public class DropdownSetting : Setting {

    private int data;
    public UnityEvent[] buttonCalls;

    [HideInInspector]
    public Dropdown dropdown;
    private Text detailsText;
    private bool hasDetails;
    private int optionsCount = 2; // one-based
    // Assigned in inspector
    [TextArea]
    public string[] detailStrings;
    public Transform[] childrenHeaders;

    protected override void Awake() {
        base.Awake();
        dropdown = GetComponentInChildren<Dropdown>();
        detailsText = transform.Find("Dropdown/Details").GetComponent<Text>();
        optionsCount = dropdown.options.Count;
        hasDetails = detailStrings.Length != 0;

        if (optionsCount < 2) {
            Debug.LogError("Not enough options provided for ButtonSetting");
        }
        if (hasDetails) {
            if (detailStrings.Length != optionsCount)
                Debug.LogError("buttonStrings and detailStrings do not match size.");
        }

        // Assign font and button size
        RectTransform rectButton = dropdown.GetComponent<RectTransform>();
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

        dropdown.onValueChanged.AddListener(DropdownValueChanged);
    }
    

    /*
     * Checks SettingsData for this button's data
     */
    public override void RefreshData() {
        data = parentSettings.GetDataInt(id);
        // interpret -1 as the maximum possible value
        if (data == -1)
            data = dropdown.options.Count - 1;
        // Update other functions
        if (data < buttonCalls.Length) {
            if (buttonCalls[data] != null)
                buttonCalls[data].Invoke();
        }
    }

    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public override void RefreshText() {
        dropdown.value = data;

        if (hasDetails)
            detailsText.text = detailStrings[data];
        if (childrenHeaders.Length > 0) {
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

    public void DropdownValueChanged(int arg0) {
        data = arg0;
        parentSettings.SetData(id, data);

        // Update other functions
        if (data < buttonCalls.Length)
            if (buttonCalls[data] != null)
                buttonCalls[data].Invoke();
        RefreshText();
    }
}
