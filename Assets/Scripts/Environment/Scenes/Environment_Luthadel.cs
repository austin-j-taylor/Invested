using UnityEngine;
using System.Collections;

public class Environment_Luthadel : Environment {

    void Start() {
        GetComponent<AudioSource>().Play();

        Player.FeelingScale = .625f;
    }
}
