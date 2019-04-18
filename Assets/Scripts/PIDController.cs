using UnityEngine;
using System.Collections;

/*
 * Attached to any object that uses PID control.
 */

public class PIDController : MonoBehaviour {

    [SerializeField]
    float gainP = 0, gainI = 0, gainD = 0;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
