using UnityEngine;
using System.Collections;

public class Environment_Luthadel : Environment {

    [SerializeField]
    private Material Material_smokeMaterial = null;
    [SerializeField]
    private bool dayMode = false;

    // Objects that are enabled or disabled, etc., depending on the time of day
    [SerializeField]
    public CloudMaster dayClouds = null;
    [SerializeField]
    public CloudMaster nightClouds = null;
    [SerializeField]
    private Material daySkyBox = null;
    [SerializeField]
    private Material nightSkyBox = null;
    [SerializeField]
    private Light dayDirectionalLight = null;
    [SerializeField]
    private Light nightDirectionalLight = null;
    [SerializeField]
    private GameObject[] dayObjects = null;
    [SerializeField]
    private GameObject[] nightObjects = null;

    void Start() {
        GetComponent<AudioSource>().Play();

        Player.FeelingScale = .75f;
        Player.PlayerInstance.SetSmokeMaterial(Material_smokeMaterial);

        // Enable/disable objects and set other style properties depending on the time of day
        for (int i = 0; i < dayObjects.Length; i++) {
            dayObjects[i].SetActive(dayMode);
        }
        for (int i = 0; i < nightObjects.Length; i++) {
            nightObjects[i].SetActive(!dayMode);
        }

        if (dayMode) {
            RenderSettings.skybox = daySkyBox;
            RenderSettings.sun = dayDirectionalLight;
            CameraController.SetCloudData(dayClouds);
        } else {
            RenderSettings.skybox = nightSkyBox;
            RenderSettings.sun = nightDirectionalLight;
            CameraController.SetCloudData(nightClouds);
        }
    }
}
