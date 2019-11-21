using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls the motion of the 3 flywheels inside the player sphere.
 * When the player tries to move, these wheels spin in a way that would
 *  produce the player's rotational rotation.
 */
public class PlayerFlywheelController : MonoBehaviour {
    private const int speedFactor = 20;

    // X rotates in the +Y
    // Y rotates in the +Z
    // Z rotates in the +Y
    private Transform wheelX;
    private Transform wheelY;
    private Transform wheelZ;


    private void Start() {
        Transform wheels = transform.Find("Wheels");
        wheelX = wheels.Find("Motor/WheelX");
        wheelY = wheels.Find("Motor_001/WheelY");
        wheelZ = wheels.Find("Motor_002/WheelZ");
    }

    public void Clear() {
        // reset rotations
    }

    // Spins the flywheels to produce the given torque.
    // The torque is in real-world units (per SECOND, not per frame)
    public void SpinToTorque(Vector3 torque) {
        torque = -torque;

        // get the relative torques

        float angleX = Time.deltaTime * speedFactor * Vector3.Dot(torque, Player.PlayerInstance.transform.right);
        float angleY = Time.deltaTime * speedFactor * Vector3.Dot(torque, Player.PlayerInstance.transform.up);
        float angleZ = Time.deltaTime * speedFactor * Vector3.Dot(torque, Player.PlayerInstance.transform.forward);

        //Debug.Log(angleX);
        //Debug.Log(angleY);
        //Debug.Log(angleZ);
        //Debug.DrawRay(Player.PlayerInstance.transform.position + new Vector3(0, .5f, 0), Player.PlayerInstance.transform.right * angleX / torque.magnitude, Color.red);
        //Debug.DrawRay(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.up * angleY / torque.magnitude, Color.green);
        //Debug.DrawRay(Player.PlayerInstance.transform.position + new Vector3(0, 1.5f, 0), Player.PlayerInstance.transform.forward * angleZ / torque.magnitude, Color.blue);
        //Debug.DrawRay(Player.PlayerInstance.transform.position, torque, Color.white);

        // apply the proportional rotations
        // i.e. "apply a torque" to each wheel

        Vector3 eulers = wheelX.localEulerAngles;
        eulers.y += angleX;
        wheelX.localEulerAngles = eulers;

        eulers = wheelY.localEulerAngles;
        eulers.z += angleY;
        wheelY.localEulerAngles = eulers;

        eulers = wheelZ.localEulerAngles;
        eulers.y += angleZ * TimeController.CurrentTimeScale;
        wheelZ.localEulerAngles = eulers;

    }

    private float TorqueToAngle(Vector3 v, Vector3 torque) {
        float dot = Vector3.Dot(torque, v);
        bool negative = dot < 0;
        float angle = dot;
        return angle;
    }

}
