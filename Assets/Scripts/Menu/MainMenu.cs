using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    public const int sceneMain = 0;
    public const int sceneLuthadel = 1;
    
    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    private SettingsMenu settingsMenu;
    private static HUD hud;
    private static GameObject player;

    // Use this for initialization
    private void Awake () {
        settingsMenu = transform.parent.GetComponentInChildren<SettingsMenu>();
        hud = transform.parent.GetComponentInChildren<HUD>();
        player = GameObject.FindGameObjectWithTag("Player");

        Button[] buttons = GetComponentsInChildren<Button>();
        playButton = buttons[0];
        settingsButton = buttons[1];
        quitButton = buttons[2];

        playButton.onClick.AddListener(OnClickedPlay);
        settingsButton.onClick.AddListener(OnClickedSettings);
        quitButton.onClick.AddListener(OnClickedQuit);

        DontDestroyOnLoad(transform.parent.gameObject);
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(GameObject.FindGameObjectWithTag("GameController"));
        
        player.SetActive(false);
        hud.DisableHUD();
    }

    private void OnClickedPlay() {
        hud.EnableHUD();
        player.SetActive(true);

        LoadScene(SceneManager.GetSceneByBuildIndex(sceneLuthadel));

        Camera.main.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnClickedSettings() {
        settingsMenu.OpenSettings();
    }

    private void OnClickedQuit() {
        Application.Quit();
    }

    public static void LoadScene(Scene scene) {
        player.GetComponent<Player>().ReloadPlayerIntoNewScene(sceneLuthadel);
        hud.ResetHUD();
    }
}
