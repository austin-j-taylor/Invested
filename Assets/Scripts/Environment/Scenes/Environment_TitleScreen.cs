using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Environment_TitleScreen : EnvironmentCinematic {

    public const float speed = .05f;
    public const float speedVert = .1f;
    public const float speedAmp = 2;


    void Start() {
        InitializeCinemachine();
        vcam.LookAt = Player.PlayerInstance.transform;

        GameManager.SetCameraState(GameManager.GameCameraState.Cutscene);
        HUD.DisableHUD();
        Magnetic[] pulls = transform.Find("Magnetics").GetComponentsInChildren<Magnetic>();
        Magnetic[] pushes = transform.Find("MagneticsPush").GetComponentsInChildren<Magnetic>();
        Prima.PrimaInstance.ActorIronSteel.StartBurning();

        TimeController.CurrentTimeScale = 0;
        Clear();

        // Handle music
        StartCoroutine(Play_title_screen_music());
    }

    // Called by DataManagement when flags are reset and player data is erased.
    public static void Clear() {
        //Prima.PlayerGlower.SetOverrideGlows(FlagsController.GetData("completeTutorial1"), FlagsController.GetData("pwr_steel"), FlagsController.GetData("pwr_pewter"), FlagsController.GetData("pwr_zinc"));
        Prima.PlayerGlower.Clear();
        if (FlagsController.CompletedAllLevels)
            Prima.PrimaInstance.SetFrameMaterial(GameManager.Material_MARLmetal_lit);
        else
            Prima.PrimaInstance.ResetFrameMaterial();
    }

    void Update() {
        cameraPositionTarget.position = new Vector3(Mathf.Cos(Time.unscaledTime * speedVert) * speedAmp, (1 + Mathf.Sin(Time.unscaledTime * speedVert)) * speedAmp, Mathf.Sin(Time.unscaledTime * speedVert) * speedAmp);
    }

    private IEnumerator Play_title_screen_music() {
        AudioSource intro = GetComponent<AudioSource>();
        while (intro.isPlaying) {
            yield return null;
        }
        // Tell AudioManager to start the loop, which may carry into the next scene.
        GameManager.AudioManager.Play_title_screen_loop();
    }
}
