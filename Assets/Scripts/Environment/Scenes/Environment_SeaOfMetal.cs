﻿using UnityEngine;
using System.Collections;

public class Environment_SeaOfMetal : Environment {

    void Start() {
        GetComponent<AudioSource>().Play();

        Player.FeelingScale = 2;
    }
}