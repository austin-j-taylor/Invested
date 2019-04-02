using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Simulation : MonoBehaviour {

    protected int ResetTime { get; set; } = 1;
    
    public virtual void StartSimulation() {
        StartCoroutine(ResetSim());
    }

    private IEnumerator ResetSim() {
        yield return new WaitForSeconds(ResetTime);
        SceneSelectMenu.ReloadScene();
    }
}
