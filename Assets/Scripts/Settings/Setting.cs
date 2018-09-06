using UnityEngine;
using UnityEngine.UI;

/*
 * Represents a single setting, such as a button or slider.
 * Assign the font size and size of the setting in the inspector.
 * Bound the setting size at 160 units to prevent clipping.
 */
public abstract class Setting : MonoBehaviour {

    // Assigned in inspector
    public string id;
    public float settingSize = 130;
    public int detailsFontSize = 10;

    public abstract void RefreshData();

    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public abstract void RefreshText();

}