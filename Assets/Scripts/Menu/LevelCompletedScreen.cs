using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedScreen : MonoBehaviour {

    private Button resetButton;

    private static GameObject levelCompletedScreen;
    
    // Use this for initialization
    void Start() {
        resetButton = GetComponentInChildren<Button>();
        resetButton.onClick.AddListener(ClickReset);

        levelCompletedScreen = gameObject;
        gameObject.SetActive(false);
    }

    private void ClickReset() {
        SceneSelectMenu.ReloadScene();
        gameObject.SetActive(false);
    }

    public static void OpenScreen() {
        levelCompletedScreen.SetActive(true);
        CameraController.UnlockCamera();
    }
}
