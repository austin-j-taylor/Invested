using UnityEngine;
using System.Collections;

public class Environment_StormRocks : MonoBehaviour {


    Rigidbody[] rbs;
    // Use this for initialization
    void Start() {
        //randomly spawn boulders
        rbs = GetComponentsInChildren<Rigidbody>();

        // randomly set velocities, spins
        rbs[0].velocity = Vector3.forward * 100;
        rbs[1].velocity = Vector3.forward * 70 + Vector3.down * 70;

    }

}
