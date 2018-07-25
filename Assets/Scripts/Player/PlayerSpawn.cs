using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour {

    // Use this for initialization
    void Start() {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().ResetPosition(this);
    }
}
