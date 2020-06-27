﻿using UnityEngine;
using System.Collections;

/// <summary>
/// A float-based PID controller.
/// </summary>
public class PIDController_float : MonoBehaviour {

    float gainP = 25, gainI = 0, gainD = 0;

    private float cmd_I, last_error, last_command;

    // Watchdog: Keep true while the controller is updating
    bool watchdog = false, lastWasStepped = false;

    private void FixedUpdate() {
        lastWasStepped = watchdog;
        watchdog = false;
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

    /// <summary>
    /// Performs one step of the feedback loop
    /// </summary>
    /// <param name="feedback">the feedback from the system</param>
    /// <param name="reference">the desired output of the system </param>
    /// <returns></returns>
    public float Step(float feedback, float reference) {

        // If the controller was not updated recently, reset the last error/command.
        if (!lastWasStepped) {
            cmd_I = 0;
            last_error = 0;
            last_command = 0;
        }
        watchdog = true;

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