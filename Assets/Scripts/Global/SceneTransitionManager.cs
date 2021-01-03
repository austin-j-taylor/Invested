using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour {

    private const float wipeTime = 0.5f;

    public void LoadScene(int scene, bool withMistyTransition = true) {
        Player.PlayerInstance.transform.parent = EventSystem.current.transform; // make sure the player is in the DontDestroyOnLoad scene

        if (withMistyTransition) {
            if (Player.PlayerInstance.playerState == Player.PlayerState.Normal) {
                Player.PlayerInstance.playerState = Player.PlayerState.Respawning;
                HUD.LoadingFadeController.Enshroud();
                HUD.DisableHUD();
                Player.CanPause = false;
                Player.PlayerInstance.transform.parent = EventSystem.current.transform;

                StartCoroutine(LoadSceneTransition(scene));
            }
        } else {
            SceneManager.LoadScene(scene);
        }
    }
    private IEnumerator LoadSceneTransition(int scene) {
        yield return new WaitForSecondsRealtime(wipeTime);
        SceneManager.LoadScene(scene);
        Player.PlayerInstance.playerState = Player.PlayerState.Normal;
    }
    public void ReloadScene() {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
