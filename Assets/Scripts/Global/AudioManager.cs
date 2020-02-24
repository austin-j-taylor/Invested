using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    private const int index_menu_select = 0;

    public AudioSource[] sources;

    private void Start() {
        sources = GetComponents<AudioSource>();
    }

    public void Play_menu_select() {
        sources[index_menu_select].Play();
    }

}
