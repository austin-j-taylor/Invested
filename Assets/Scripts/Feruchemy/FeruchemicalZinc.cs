using UnityEngine;

/// <summary>
/// Manages Feruchemical Zinc, the bullet time effect.
/// </summary>
public class FeruchemicalZinc : MonoBehaviour {

    #region constants
    // intensity formula constants
    private const float a = 2f, b = 42, c = 0.67f, d = 11f, f = 2.4f, g = 72f, h = 50;
    // The maximum time that zinc will slow down for
    private const float maxTime = 10, recoveryFactor = .001f;
    // the time scale that zinc slows time down to
    // interestingly, 1/8 is about the same that in-world speed bubbles slow time down by (Alloy of Law, 2 minutes into about 15s)
    private const float slowPercent = 1 / 8f;
    // Pitch of all audio when in zinc time (%)
    private const float slowPitch = slowPercent * 2;
    #endregion

    public bool InZincTime { get; private set; }
    private bool recovering;
    private float timeSpentRecovering;
    private double startReserve; // the reserve that the player last entered zinc time at
    private double endReserve; // the reserve that the player last exited zinc time at
    // percentage of available zinc
    // 100% -> do not move
    // 0% -> maximum movement
    public double Reserve { get; private set; }
    private double lastReserve;
    public double Rate { get; private set; }
    public float Intensity { get; private set; }

    public void Clear() {
        InZincTime = false;
        recovering = false;
        timeSpentRecovering = 0;
        Reserve = 1;
        startReserve = 1;
        endReserve = 1;
        Rate = 0;
        Intensity = 0;
        lastReserve = Reserve;
        TimeController.CurrentTimeScale = SettingsMenu.settingsWorld.timeScale;
        GameManager.GraphicsController.SetZincEffect(false);
    }

    void Update() {
        if (!PauseMenu.IsPaused) {
            if (InZincTime) {
                // In Zinc Time
                Rate = -Time.deltaTime / slowPercent / maxTime;
                Reserve += Rate;
                if (Reserve < 0) {
                    Reserve = 0;
                    Rate = 0;
                }

                if (!(Keybinds.ZincTime() || Keybinds.ControlWheel()) || Reserve == 0 || !Player.CanControl || !Player.CanControlZinc) {
                    // Exit zinc time
                    InZincTime = false;
                    endReserve = Reserve;
                    timeSpentRecovering = 0;
                    TimeController.CurrentTimeScale = SettingsMenu.settingsWorld.timeScale;
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
            } else {
                // Not In Zinc Time
                if (Reserve < 1) {
                    // Recharge the bank at a flat rate + time spent recovering, making it recover faster when not used over time
                    Rate = Time.deltaTime / maxTime + timeSpentRecovering;
                    timeSpentRecovering += Time.deltaTime * recoveryFactor;
                    Reserve += Rate;
                    if (Reserve > 1) {
                        if (recovering) {
                            recovering = false;
                            Intensity = GameManager.GraphicsController.SetZincEffect(false);
                        }
                        Reserve = 1;
                        Rate = 0;
                    }
                }

                if ((Keybinds.ZincTimeDown() || Keybinds.ControlWheelDown()) && Reserve > 0 && Player.CanControl && Player.CanControlZinc) {
                    // Enter zinc time
                    InZincTime = true;
                    recovering = false;
                    startReserve = Reserve;
                    TimeController.CurrentTimeScale = slowPercent * SettingsMenu.settingsWorld.timeScale;
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

    /// <summary>
    /// Calculates the intensity for the zinc visual effect.
    /// </summary>
    /// <param name="x">the % of the reserve</param>
    /// <returns>the intensity</returns>
    private float CalculateIntensity(float x) {
        float fStart = (float)startReserve;
        // hot formula that makes a nice curve
        //float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f * (x - 1)) - Mathf.Exp(g * (x - 1)));
        float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f / fStart * (x - fStart)) - Mathf.Exp(g * (x - fStart)));

        return intensity;
    }

    /// <summary>
    /// Calculates the intensity of the zinc visual effect right as it starts recovering
    /// </summary>
    /// <returns>the intensity</returns>
    private float CalculateRecovering() {
        float fReserve = (float)Reserve;
        float fEnd = (float)endReserve;
        // hot formula that makes a nice curve
        //float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f * (x - 1)) - Mathf.Exp(g * (x - 1)));
        float intensity = CalculateIntensity(fEnd) * (2 - Mathf.Exp(-h * (fEnd - fReserve)));

        if (intensity < 0) {
            recovering = false;
            return 0;
        }

        return intensity;
    }
}
