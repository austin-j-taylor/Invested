using UnityEngine;

/*
 * Controls feruchemical zinc.
 * Controls "bullet time" effect.
 * 
 * Opening the Control Wheel also activates Zinc Time.
 */
public class FeruchemicalZinc : MonoBehaviour {

    // intensity formula constants
    private const float a = 2f;
    private const float b = 42;
    private const float c = 0.67f;
    private const float d = 11f;
    private const float f = 2.4f;
    private const float g = 72f;
    private const float h = 50;
    // The maximum time that zinc will slow down for
    private const float maxTime = 8;
    // the time scale that zinc slows time down to
    // interestingly, 1/8 is about the same that in-world speed bubbles slow time down by (Alloy of Law, 2 minutes into about 15s)
    private const float slowPercent = 1 / 8f;
    // Pitch of all audio when in zinc time (%)
    private const float slowPitch = slowPercent * 2;

    public bool InZincTime { get; private set; }
    private bool recovering;
    private double startReserve; // the reserve that the player last entered zinc time at
    private double endReserve; // the reserve that the player last exited zinc time at
    // percentage of available zinc
    // 100% -> do not move
    // 0% -> maximum movement
    public double Reserve { get; private set; }
    private double lastReserve;
    public double Rate { get; private set; }
    public float Intensity { get; private set; }
    
    //// Use this for initialization
    //void Start() {
    //    Clear();
    //}

    public void Clear() {
        InZincTime = false;
        recovering = false;
        Reserve = 1;
        startReserve = 1;
        endReserve = 1;
        Rate = 0;
        Intensity = 0;
        lastReserve = Reserve;
        TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
        GameManager.GraphicsController.SetZincEffect(false);
    }

    // Update is called once per frame
    void Update() {
        if (!PauseMenu.IsPaused) {
            if (InZincTime) {
                Rate = -Time.deltaTime / slowPercent / maxTime;
                Reserve += Rate;
                if (Reserve < 0) {
                    Reserve = 0;
                    Rate = 0;
                }

                if (!(Keybinds.ZincTime() || Keybinds.ControlWheel())|| Reserve == 0 || !Player.CanControl || !Player.CanControlZinc) {
                    // Exit zinc time
                    InZincTime = false;
                    endReserve = Reserve;
                    HUD.ZincMeterController.SideEnabled = false;
                    TimeController.CurrentTimeScale = SettingsMenu.settingsData.timeScale;
                    GameManager.AudioManager.SetMasterPitch(1);
                    if (Reserve == 0) {
                        Intensity = GameManager.GraphicsController.SetZincEffect(false);
                        recovering = false;
                    } else {
                        Intensity = GameManager.GraphicsController.SetZincEffect(true, CalculateRecovering());
                        recovering = true;
                    }
                } else {
                    // in zinc time, proceed with zinc effect
                    Intensity = GameManager.GraphicsController.SetZincEffect(true, CalculateIntensity((float)Reserve));
                }
                HUD.ZincMeterController.ChangeSpikePosition((float)Reserve);
            } else {
                if (Reserve < 1) {
                    Rate = Time.deltaTime / maxTime;
                    Reserve += Rate;
                    if (Reserve > 1) {
                        if (recovering) {
                            recovering = false;
                            Intensity = GameManager.GraphicsController.SetZincEffect(false);
                        }
                        Reserve = 1;
                        Rate = 0;
                    }
                    HUD.ZincMeterController.ChangeSpikePosition((float)Reserve);
                }

                if ((Keybinds.ZincTimeDown() || Keybinds.ControlWheelDown()) && Reserve > 0 && Player.CanControl && Player.CanControlZinc) {
                    // Enter zinc time
                    InZincTime = true;
                    recovering = false;
                    startReserve = Reserve;
                    HUD.ZincMeterController.SideEnabled = true;
                    TimeController.CurrentTimeScale = slowPercent * SettingsMenu.settingsData.timeScale;
                    GameManager.AudioManager.SetMasterPitch(slowPitch);
                    Intensity = GameManager.GraphicsController.SetZincEffect(true, CalculateIntensity((float)Reserve));
                } else if (recovering) {
                    // player recently exiting zinc time; continue showing screen effect until it's gone
                    Intensity = GameManager.GraphicsController.SetZincEffect(true, CalculateRecovering());

                    // if done recovering, truly end zinc effect
                    if (!recovering) {
                        Intensity = GameManager.GraphicsController.SetZincEffect(false);
                    }
                }
            }
        }
    }
    
    private float CalculateIntensity(float x) {
        float fStart = (float)startReserve;
        // hot formula that makes a nice curve
        //float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f * (x - 1)) - Mathf.Exp(g * (x - 1)));
        float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f / fStart * (x - fStart)) - Mathf.Exp(g * (x - fStart)));

        return intensity;
    }

    private float CalculateRecovering() {
        float fReserve = (float)Reserve;
        float fEnd = (float)endReserve;
        // hot formula that makes a nice curve
        //float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f * (x - 1)) - Mathf.Exp(g * (x - 1)));
        float intensity = CalculateIntensity(fEnd) * (2 - Mathf.Exp(-h * (fEnd - fReserve)));

        if(intensity < 0) {
            recovering = false;
            return 0;
        }

        return intensity;
    }

}
