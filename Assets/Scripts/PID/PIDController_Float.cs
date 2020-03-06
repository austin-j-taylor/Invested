using UnityEngine;
using System.Collections;

/*
 * Attached to any object that uses PID control for floats.
 * Not using generic typing because you can't use operators with them.
 */

public class PIDController_float {

    float gainP = 25, gainI = 0, gainD = 0;

    private float cmd_I, last_error, last_command;


    // Use this for initialization
    public PIDController_float(float p, float i, float d) {
        gainP = p;
        gainI = i;
        gainD = d;
        cmd_I = 0;
        last_error = 0;
        last_command = 0;
    }

    public void Clear() {
        cmd_I = 0;
        last_error = 0;
        last_command = 0;
    }

    public void SetParams(float p, float i, float d) {
        gainP = p;
        gainI = i;
        gainD = d;
    }

    public float Step(float feedback, float reference) {

        float error = reference - feedback;

        float cmd_P = gainP * error;
        cmd_I += gainI * error;
        float cmd_D = gainD * (error - last_error);

        float command = cmd_P + cmd_I + cmd_D;

        //float delta = last_command - command;
        //Debug.Log("P:        " + cmd_P);
        //Debug.Log("I:        " + cmd_I);
        //Debug.Log("D:        " + cmd_D);
        //Debug.Log("delta:    " + delta);
        //Debug.Log("reference:" + reference);
        //Debug.Log("feedback: " + feedback);
        //Debug.Log("error:    " + error);
        //Debug.Log("command:  " + command);

        last_error = error;
        last_command = command;

        return command;
    }
}
