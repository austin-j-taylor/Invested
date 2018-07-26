using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLoader : MonoBehaviour {

    void Start() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
