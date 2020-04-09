using UnityEngine;
using System.Collections;

public class Environment_Luthadel : Environment {

    [SerializeField]
    private Material Material_smokeMaterial = null;

    void Start() {
        GetComponent<AudioSource>().Play();

        Player.FeelingScale = .75f;
        Player.PlayerInstance.SetSmokeMaterial(Material_smokeMaterial);
    }
}
