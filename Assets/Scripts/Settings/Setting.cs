using UnityEngine;
using UnityEngine.UI;

/*
 * represents a single setting, such as a button or slider
 */
public abstract class Setting : MonoBehaviour {
    protected Text detailsText;
    protected bool hasDetails;
    // Assigned in inspector
    public string id;
    public float settingSize = 130;
    public float textFontSize = 10;

    protected virtual void Awake() {
        detailsText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
    }

    public virtual void Refresh() {
    }

    public virtual void Enable() {
        gameObject.SetActive(true);
    }
    public virtual void Disable() {
        gameObject.SetActive(true);
    }
}