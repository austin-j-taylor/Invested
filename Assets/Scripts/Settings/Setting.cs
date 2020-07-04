using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single setting, such as a button or slider.
/// Assign the font size and size of the setting in the inspector.
/// </summary>
public abstract class Setting : MonoBehaviour {

    // Assigned in inspector
    public string id;
    public float settingSize = 130;
    public int detailsFontSize = 10;
    protected JSONSettings parentSettings;

    public abstract void RefreshData();

    protected virtual void Awake() {
        parentSettings = GetComponentInParent<JSONSettings>();
    }
    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public abstract void RefreshText();

}