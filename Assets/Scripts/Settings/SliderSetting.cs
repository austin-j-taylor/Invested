using UnityEngine.UI;

public class SliderSetting : Setting {

    private Slider slider;
    private Text valueText;
    // Assigned in inspector
    public float min = 0;
    public float max = 100;
    public float defaultValue = 0;

    protected override void Awake() {
        base.Awake();
        slider = GetComponentInChildren<Slider>();
        valueText = GetComponentInChildren<Text>();
        slider.onValueChanged.AddListener(OnValueChanged);

        slider.minValue = min;
        slider.maxValue = max;
        slider.value = defaultValue;
        hasDetails = false;
    }

    public override void Refresh() {
        base.Refresh();
    }

    public void OnValueChanged(float value) {
        valueText.text = ((int)value).ToString();
    }
}
