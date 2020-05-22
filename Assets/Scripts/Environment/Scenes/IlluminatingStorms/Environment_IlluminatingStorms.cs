using UnityEngine;
using System.Collections;

public class Environment_IlluminatingStorms : Environment {

    // Use this for initialization
    void Start() {
        GetComponent<AudioSource>().Play();

        Player.FeelingScale = 2;
        Player.VoidHeight = -1000;
    }

    // Update is called once per frame
    void Update() {

    }
}
