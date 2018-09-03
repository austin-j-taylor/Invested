using UnityEngine.UI;

public class SliderSetting : Setting {
    private float data;

    private Slider slider;
    private Text valueText;
    // Assigned in inspector
    public float min = 0;
    public float max = 100;
    public float defaultValue = 0;

    void Awake() {

        slider = GetComponentInChildren<Slider>();
        valueText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    /*
     * Checks SettingsData for this slider's data
     */
    public override void RefreshData() {
        data = SettingsMenu.settingsData.GetDataFloat(id);
        slider.value = data;
    }

    /*
     * Updates the text fields for this setting to reflect the setting's data
     */
    public override void RefreshText() {
        valueText.text = ((int)data).ToString();
    }

    public void OnValueChanged(float value) {
        data = value;
        SettingsMenu.settingsData.SetData(id, data);
        RefreshText();
    }
}
