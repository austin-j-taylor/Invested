using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls the motion of the 3 flywheels inside the player sphere.
 * When the player tries to move, these wheels spin in a way that would
 *  produce the player's rotational rotation.
 *  
 *  Also, spin the wheels a bit while burning pewter.
 */
public class PlayerFlywheelController : MonoBehaviour {

    private const int speedFactor = 20;
    private const int pewterSpinFactor = 20;
    private const int passiveSpin = 10;

    private Animator anim;

    // X rotates in the +Y
    // Y rotates in the +Z
    // Z rotates in the +Y
    private Transform wheelX;
    private Transform wheelY;
    private Transform wheelZ;

    private Quaternion startX;
    private Quaternion startY;
    private Quaternion startZ;

    private bool extended;


    private void Start() {
        anim = GetComponentInParent<Animator>();

        Transform wheels = transform.Find("Wheels");
        wheelX = wheels.Find("Motor/WheelX");
        wheelY = wheels.Find("Motor_001/WheelY");
        wheelZ = wheels.Find("Motor_002/WheelZ");
        startX = wheelX.localRotation;
        startY = wheelY.localRotation;
        startZ = wheelZ.localRotation;
    }

    private void FixedUpdate() {
        if (!PauseMenu.IsPaused) {
            if (Player.PlayerPewter.IsBurning) {
                AddAngleX(pewterSpinFactor * -(float)Player.PlayerPewter.PewterReserve.Rate);
                AddAngleY(pewterSpinFactor * -(float)Player.PlayerPewter.PewterReserve.Rate);
                AddAngleZ(pewterSpinFactor * -(float)Player.PlayerPewter.PewterReserve.Rate);
            }
        }
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {
            AddAngleX(passiveSpin);
            AddAngleY(passiveSpin);
            AddAngleZ(passiveSpin);
        }
    }

    public void Clear() {
        Retract();
        // reset rotations
        wheelX.localRotation = startX;
        wheelY.localRotation = startY;
        wheelZ.localRotation = startZ;
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

        AddAngleX(angleX);
        AddAngleY(angleY);
        AddAngleZ(angleZ);
    }

    // Spins the wheels by the given angle
    private void AddAngleX(float angleX) {
        Vector3 eulers = wheelX.localEulerAngles;
        eulers.y += angleX * TimeController.CurrentTimeScale;
        wheelX.localEulerAngles = eulers;
    }
    private void AddAngleY(float angleY) {
        Vector3 eulers = wheelY.localEulerAngles;
        eulers.z += angleY * TimeController.CurrentTimeScale;
        wheelY.localEulerAngles = eulers;
    }
    private void AddAngleZ(float angleZ) {
        Vector3 eulers = wheelZ.localEulerAngles;
        eulers.y += angleZ * TimeController.CurrentTimeScale;
        wheelZ.localEulerAngles = eulers;
    }

    public void Extend() {
        if(extended) {
            anim.SetBool("Extended", true);
            extended = false;
        }
    }

    public void Retract() {
        if (!extended) {
            anim.SetBool("Extended", false);
            extended = true;
        }
    }
}
