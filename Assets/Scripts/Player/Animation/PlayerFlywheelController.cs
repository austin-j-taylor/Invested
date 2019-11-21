using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls the motion of the 3 flywheels inside the player sphere.
 * When the player tries to move, these wheels spin in a way that would
 *  produce the player's rotational rotation.
 */
public class PlayerFlywheelController : MonoBehaviour
{
    // X rotates in the +Y
    // Y rotates in the +Z
    // Z rotates in the +Y
    private Transform motorX;
    private Transform motorY;
    private Transform motorZ;


    private void Start() {
        Transform wheels = transform.Find("Wheels");
        motorX = wheels.Find("Motor/WheelX");
        motorY = wheels.Find("Motor_001/WheelY");
        motorZ = wheels.Find("Motor_002/WheelZ");
    }

    public void Clear() {

    }

    // Spins the flywheels to produce the given torque.
    // The torque is in real-world units (per SECOND, not per frame)
    public void SpinToTorque(Vector3 torque) {

    }
}
