using UnityEngine;
using System.Collections;

public class Environment_SpawnRoom : Environment {

    [SerializeField]
    private Light flickeringSpotlight = null;

    
    // Update is called once per frame
    void Update() {
        flickeringSpotlight.intensity = 17 * Mathf.Sin(Time.time);
    }
}
