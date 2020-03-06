using UnityEngine;
using System.Collections;

/*
 * Attached to any object that uses PID control for Vector3s.
 * Not using generic typing because you can't use operators with them.
 */

public class PIDController_Vector3 {

    float gainP = 25, gainI = 0, gainD = 0, maxDelta = 50;

    private Vector3 cmd_I, last_error, last_command;


    // Use this for initialization
    public PIDController_Vector3(float p, float i, float d, float mD) {
        gainP = p;
        gainI = i;
        gainD = d;
        maxDelta = mD;
        cmd_I = Vector3.zero;
        last_error = Vector3.zero;
        last_command = Vector3.zero;
    }

    public Vector3 Step(Vector3 feedback, Vector3 reference) {

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
