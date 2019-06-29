using UnityEngine;
using System.Collections;

/*
 * Controls feruchemical zinc.
 * Controls "bullet time" effect.
 */
public class FeruchemicalZinc : MonoBehaviour {

    // The maximum time that zinc will slow down for
    private const float maxTime = 8;
    // the time scale that zinc slows time down to
    // interestingly, 1/8 is about the same that in-world speed bubbles slow time down by (Alloy of Law, 2 minutes into about 15s)
    private const float slowPercent = 1 / 8f;

    private bool inZincTime;
    private double startReserve; // the reserve that the player last entered zinc time at
    // percentage of available zinc
    // 100% -> do not move
    // 0% -> maximum movement
    public double Reserve { get; private set; }
    private double lastReserve;
    public double Rate { get; private set; }
    
    // Use this for initialization
    void Start() {
        Clear();
    }

    public void Clear() {
        inZincTime = false;
        Reserve = 1;
        Rate = 0;
        lastReserve = Reserve;
        TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
        GameManager.GraphicsController.SetMotionBlur(SettingsMenu.settingsData.motionBlur == 1);
    }

    // Update is called once per frame
    void Update() {
        if (Player.CanControlPlayer) {
            if (inZincTime) {
                Rate = -Time.deltaTime / slowPercent / maxTime;
                Reserve += Rate;
                if (Reserve < 0) {
                    Reserve = 0;
                    Rate = 0;
                }

                if(!Keybinds.ZincTime() || Reserve == 0) {
                    inZincTime = false;
                    HUD.ZincMeterController.SideEnabled = false;
                    TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
                    GameManager.GraphicsController.SetZincEffect(false);
                } else {
                    GameManager.GraphicsController.SetZincEffect(true, (float)Reserve, (float)(startReserve));
                }
                HUD.ZincMeterController.ChangeSpikePosition((float)Reserve);
            } else {
                Rate = Time.deltaTime / maxTime;
                Reserve += Rate;
                if (Reserve > 1) {
                    Reserve = 1;
                    Rate = 0;
                } else {
                    HUD.ZincMeterController.ChangeSpikePosition((float)Reserve);
                }

                if (Keybinds.ZincTimeDown() && Reserve > 0) {
                    inZincTime = true;
                    startReserve = Reserve;
                    HUD.ZincMeterController.SideEnabled = true;
                    TimeController.CurrentTimeScale = slowPercent * SettingsMenu.settingsData.timeScale;
                    GameManager.GraphicsController.SetZincEffect(true, (float)Reserve, (float)(startReserve));
                }
            }
        }
    }
}
