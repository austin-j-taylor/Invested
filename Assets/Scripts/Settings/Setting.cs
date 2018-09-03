using UnityEngine;
using UnityEngine.UI;

/*
 * represents a single setting, such as a button or slider
 */
public abstract class Setting : MonoBehaviour {

    // Assigned in inspector
    public string id;
    public float settingSize = 130;
    public float detailsFontSize = 10;

    public abstract void RefreshData();

    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public abstract void RefreshText();

}