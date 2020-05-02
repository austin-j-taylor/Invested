using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Environment_TitleScreen : Environment {

    [SerializeField]
    private float speed = .05f;
    [SerializeField]
    private float speedVert = .1f;
    [SerializeField]
    private float speedAmp = 2;

    private Transform cameraPositionTarget;

    void Start() {

        HUD.DisableHUD();
        TimeController.CurrentTimeScale = 0;
        cameraPositionTarget = transform.Find("cameraTarget");
        CameraController.SetExternalSource(cameraPositionTarget, Player.PlayerInstance.transform, false);

        Magnetic[] pulls = transform.Find("Magnetics").GetComponentsInChildren<Magnetic>();
        Magnetic[] pushes = transform.Find("MagneticsPush").GetComponentsInChildren<Magnetic>();
        Player.PlayerIronSteel.SizeOfTargetArrays = 5;
        foreach (Magnetic m in pulls) {
            Player.PlayerIronSteel.AddPullTarget(m);
        }
        foreach (Magnetic m in pushes) {
            Player.PlayerIronSteel.AddPullTarget(m);
        }

        TimeController.CurrentTimeScale = 0;
    }

    void Update() {
        cameraPositionTarget.RotateAround(Player.PlayerInstance.transform.position, Vector3.up, speed);
        Vector3 pos = cameraPositionTarget.position;
        pos.y = Mathf.Sin(Time.unscaledTime * speedVert) * speedAmp;
        cameraPositionTarget.position = pos;
    }
}
