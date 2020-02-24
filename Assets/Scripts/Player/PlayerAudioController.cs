using UnityEngine;
using System.Collections;

public class PlayerAudioController : MonoBehaviour {

    public AudioListener Listener { get; private set; }

    void Start() {
        Listener = GetComponent<AudioListener>();
    }

    public void Clear() {
    }
}
