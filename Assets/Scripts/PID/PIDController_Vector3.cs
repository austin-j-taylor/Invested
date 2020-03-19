using UnityEngine;
using System.Collections;

/*
 * Attached to any object that uses PID control for Vector3s.
 * Not using generic typing because you can't use operators with them.
 */

public class PIDController_Vector3 : MonoBehaviour  {

    float gainP = 25, gainI = 0, gainD = 0, maxDelta = 50;

    private Vector3 cmd_I = Vector3.zero, last_error = Vector3.zero, last_command = Vector3.zero;

    // Watchdog: Keep true while the controller is updating
    bool watchdog = false, lastWasStepped = false;

    private void FixedUpdate() {
        lastWasStepped = watchdog;
        watchdog = false;
    }

    public void SetParams(float p, float i, float d, float mD) {
        gainP = p;
        gainI = i;
        gainD = d;
    }

    public Vector3 Step(Vector3 feedback, Vector3 reference) {

        // If the controller was not updated recently, reset the last error/command.
        if(!lastWasStepped) {
            cmd_I = Vector3.zero;
            last_error = Vector3.zero;
            last_command = Vector3.zero;
        }
        watchdog = true;

        Vector3 error = reference - feedback;

        Vector3 cmd_P = gainP * error;
        cmd_I += gainI * error;
        Vector3 cmd_D = gainD * (error - last_error);

        Vector3 command = cmd_P + cmd_I + cmd_D;

        Vector3 delta = last_command - command;
        if (delta.sqrMagnitude > maxDelta * maxDelta) {
            command = last_command - maxDelta * delta.normalized;
        }

        //Debug.Log("P:        " + cmd_P + " -> " + cmd_P.magnitude);
        //Debug.Log("I:        " + cmd_I + " -> " + cmd_I.magnitude);
        //Debug.Log("D:        " + cmd_D + " -> " + cmd_D.magnitude);
        //Debug.Log("delta:    " + delta + " -> " + delta.magnitude);
        //Debug.Log("reference:" + reference + " -> " + reference.magnitude);
        //Debug.Log("feedback: " + feedback + " -> " + feedback.magnitude);
        //Debug.Log("error:    " + error + " -> " + error.magnitude);
        //Debug.Log("command:  " + command + " -> " + command.magnitude);

        last_error = error;
        last_command = command;

        return command;
    }
}
