using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour {

    private const float wipeTime = 0.5f;

    public void LoadScene(int scene) {
        if (Player.PlayerInstance.playerState == Player.PlayerState.Normal) {
            Player.PlayerInstance.playerState = Player.PlayerState.Respawning;
            HUD.LoadingFadeController.Enshroud();
            HUD.DisableHUD();
            Player.CanPause = false;
            Player.PlayerInstance.transform.parent = EventSystem.current.transform;

            StartCoroutine(LoadSceneTransition(scene));
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
