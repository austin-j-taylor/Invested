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

        HUD.DisableHUD();
        Magnetic[] pulls = transform.Find("Magnetics").GetComponentsInChildren<Magnetic>();
        Magnetic[] pushes = transform.Find("MagneticsPush").GetComponentsInChildren<Magnetic>();
        Player.PlayerIronSteel.StartBurning();
        //Player.PlayerIronSteel.SizeOfTargetArrays = 5;
        //foreach (Magnetic m in pulls) {
        //    Player.PlayerIronSteel.AddPullTarget(m);
        //}
        //foreach (Magnetic m in pushes) {
        //    Player.PlayerIronSteel.AddPullTarget(m);
        //}

        TimeController.CurrentTimeScale = 0;

        // Handle music
        StartCoroutine(Play_title_screen_music());
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
