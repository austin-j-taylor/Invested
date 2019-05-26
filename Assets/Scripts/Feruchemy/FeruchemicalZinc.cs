using UnityEngine;
using System.Collections;

/*
 * Controls feruchemical zinc.
 * Controls "bullet time" effect.
 */
public class FeruchemicalZinc : MonoBehaviour {

    [SerializeField]
    private float slowPercent = 0.1f; // the time scale that zinc slows time down to

    private bool inZincTime;

    // Use this for initialization
    void Start() {
        Clear();
    }

    public void Clear() {
        inZincTime = false;
        TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
        GameManager.GraphicsController.SetMotionBlur(SettingsMenu.settingsData.motionBlur == 1);
    }

    // Update is called once per frame
    void Update() {
        if (Player.CanControlPlayer) {
            if(inZincTime) {
                if(!Keybinds.ZincTime()) {
                    inZincTime = false;
                    TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
                    GameManager.GraphicsController.SetMotionBlur(SettingsMenu.settingsData.motionBlur == 1);
                }
            } else {
                if(Keybinds.ZincTimeDown()) {
                    inZincTime = true;
                    TimeController.CurrentTimeScale = slowPercent * SettingsMenu.settingsData.timeScale;
                    GameManager.GraphicsController.SetMotionBlur(false);
                }
            }
        }
    }
}
