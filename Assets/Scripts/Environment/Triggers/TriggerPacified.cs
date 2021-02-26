using UnityEngine;
using System.Collections;

/// <summary>
/// A trigger that is triggered when all "enemies in the room" are pacified.
/// </summary>
public class TriggerPacified : TriggerEnvironment {

    [SerializeField]
    private Pacifiable[] enemies = null;

    void Update() {
        // If all the "enemies" have been pacified, trigger the routine.
        int i = 0;
        while(i < enemies.Length) {
            if(enemies[i].Hostile) {
                break;
            }
        }
        if(i == enemies.Length) {
            StartCoroutine(routine);
        }
    }
}
