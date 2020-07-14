using UnityEngine;
using System.Collections;

public class Environment_IlluminatingStorms : Environment {

    void Start() {
        GetComponent<AudioSource>().Play();

        Player.FeelingScale = 2;
        Player.VoidHeight = -1000;
    }
}
