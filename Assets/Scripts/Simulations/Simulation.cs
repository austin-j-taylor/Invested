using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Simulation : MonoBehaviour {

    protected int ResetTime { get; set; } = 1;

    // still can slow time by pressing tab in simulations
    public bool InZincTime { get; private set; }
    private const float slowPercent = 1 / 8f;
    protected float desiredTimeScale;

    public virtual void StartSimulation() {
        InZincTime = false;
        desiredTimeScale = 1;
        StartCoroutine(ResetSim());
    }

    private IEnumerator ResetSim() {
        yield return new WaitForSeconds(ResetTime);
        SceneSelectMenu.ReloadScene();
    }

    protected virtual void Update() {

        if (!GameManager.MenusController.pauseMenu.IsOpen) {
            if (InZincTime) {
                if (Keybinds.ZincTime()) {
                    TimeController.CurrentTimeScale = slowPercent * desiredTimeScale;
                } else {
                    InZincTime = false;
                    TimeController.CurrentTimeScale = desiredTimeScale;
                }
            } else {
                if (Keybinds.ZincTimeDown()) {
                    InZincTime = true;
                    TimeController.CurrentTimeScale = slowPercent * desiredTimeScale;
                } else {
                    TimeController.CurrentTimeScale = desiredTimeScale;
                }
            }
        }
    }
}
