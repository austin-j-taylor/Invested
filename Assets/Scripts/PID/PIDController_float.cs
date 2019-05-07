using UnityEngine;
using System.Collections;

/*
 * Attached to any object that uses PID control for floats.
 * Not using generic typing because you can't use operators with them.
 */

public class PIDController_float : MonoBehaviour {

    [SerializeField]
    float gainP, gainI, gainD, maxDelta;

    bool updating, lastWasStepped;
    
    private float cmd_I, last_error, last_command;


    // Use this for initialization
    void Awake() {
        updating = false;
        lastWasStepped = false;
        cmd_I = 0;
        last_error = 0;
        last_command = 0;
    }

    private void FixedUpdate() {
        updating = lastWasStepped;

        lastWasStepped = false;
    }

    public float Step(float feedback, float reference) {

        // If this controller was not updating recently, discard old values
        if(!updating) {
            last_error = 0;
            last_command = 0;
        }

        float error = reference - feedback;

        float cmd_P = gainP * error;
        cmd_I += gainI * error;
        float cmd_D = gainD * (error - last_error);

        float command = cmd_P + cmd_I + cmd_D;

        float delta = last_command - command;
        if (delta > maxDelta) {
            command = last_command - maxDelta * delta;
        }

        //Debug.Log("P:        " + cmd_P + " -> " + cmd_P);
        //Debug.Log("I:        " + cmd_I + " -> " + cmd_I);
        //Debug.Log("D:        " + cmd_D + " -> " + cmd_D);
        //Debug.Log("delta:    " + delta + " -> " + delta);
        //Debug.Log("reference:" + reference + " -> " + reference);
        //Debug.Log("feedback: " + feedback + " -> " + feedback);
        //Debug.Log("error:    " + error + " -> " + error);
        //Debug.Log("command:  " + command + " -> " + command);

        lastWasStepped = true;
        last_error = error;
        last_command = command;

        return command;
    }

    // A step in which no inputs are needed, but last_* fields should be cleared
    public void ClearStep() {
    }
}
