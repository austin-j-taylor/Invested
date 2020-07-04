using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a setting that is controlled by a slider.
/// In the inspector, assign the variables associated with the slider.
/// Unlike a ButtonSetting, the details text is written normally through the heirarchy since that doesn't need to change with different slider values, unlike the buttons.
/// </summary>
public class SliderSetting : Setting {
    private float data;

    [HideInInspector]
    public Slider slider;
    [HideInInspector]
    public Text valueText;
    // Assigned in inspector
    public float min = 0;
    public float max = 100;
    public float defaultValue = 0;
    public bool updateTextWhenChanged = true;
    public bool showDecimals;

    protected override void Awake() {
        base.Awake();
        slider = GetComponentInChildren<Slider>();
        Text detailsText = transform.GetChild(0).GetChild(0).GetComponent<Text>();
        valueText = transform.GetChild(0).GetChild(1).GetComponent<Text>();

        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;

        // Assign font and button size
        RectTransform rectButton = slider.GetComponent<RectTransform>();
        RectTransform rectDetails = detailsText.GetComponent<RectTransform>();
        Vector2 right = rectButton.offsetMax;
        Vector2 left = rectDetails.offsetMax;
        left.x = 900 + 180 - settingSize;
        right.x = settingSize;
        rectButton.offsetMax = right;
        rectDetails.offsetMax = left;

        detailsText.fontSize = detailsFontSize;

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    /*
     * Checks SettingsData for this slider's data
     */
    public override void RefreshData() {
        data = parentSettings.GetDataFloat(id);
        slider.value = data;
    }

    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public override void RefreshText() {
        if (updateTextWhenChanged) {
            if (showDecimals)
                valueText.text = ((int)(100 * data) / 100f).ToString(); // rounds to two decimal places
            else
                valueText.text = ((int)data).ToString();
        }

    }

    public void OnValueChanged(float value) {
        data = value;
        parentSettings.SetData(id, data);
        RefreshText();
    }
}
