using UnityEngine;
using UnityEngine.UI;


public class ButtonSetting : Setting {
    private int data;

    private Button button;
    private Text buttonText;
    private int optionsCount = 1; // data can = 0, data can = 1
    private bool hasButtonChildren;
    private ButtonSetting[] buttonChildren;
    // Assigned in inspector
    public string[] buttonStrings;
    public string[] detailStrings;

    protected override void Awake() {
        base.Awake();
        button = GetComponentInChildren<Button>();
        buttonText = transform.GetChild(0).GetChild(1).GetComponent<Text>();
        buttonChildren = new ButtonSetting[0];// ; GetComponentsInChildren<ButtonSetting>();
        hasButtonChildren = buttonChildren.Length != 0;
        hasDetails = detailStrings[0] != string.Empty;

        button.onClick.AddListener(OnClick);
    }

    public override void Refresh() {
        base.Refresh();
        buttonText.text = buttonStrings[data];
        if(hasDetails)
            detailsText.text = detailStrings[data];
        if(hasButtonChildren)
            foreach (ButtonSetting child in buttonChildren)
                child.Refresh();
    }

    public void OnClick() {
        data = SettingsMenu.settingsData.GetDataInt(id);
        Debug.Log(data);
        if (data >= optionsCount) {
            data = 0;
        } else {
            data++;
        }
        SettingsMenu.settingsData.SetData(id, data);
        Refresh();
    }
    
    public override void Enable() {
        if (hasButtonChildren)
            foreach (ButtonSetting child in buttonChildren)
                child.Enable();
        base.Enable();
    }

    public override void Disable() {
        if (hasButtonChildren)
            foreach (ButtonSetting child in buttonChildren)
                child.Disable();
        base.Disable();
    }
}