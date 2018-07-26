using UnityEngine;
/*
 * At startup, loads the Title Screen scene
 */
public class MainLoader : MonoBehaviour {

    void Start() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneSelectMenu.sceneTitleScreen);
    }
}
