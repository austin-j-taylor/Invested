using UnityEngine;
using System.Collections;

public class Environment_Luthadel : Environment {

    [SerializeField]
    private Material Material_smokeMaterial = null;

    // Objects that are enabled or disabled, etc., depending on the time of day
    public static bool DayMode { get; set; } = false;
    [SerializeField]
    public CloudMaster dayCloudsVolumetric = null;
    [SerializeField]
    public CloudMaster nightCloudsVolumetric = null;
    [SerializeField]
    public ParticleSystem dayCloudsSimple = null;
    [SerializeField]
    public ParticleSystem nightCloudsSimple = null;
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
            dayObjects[i].SetActive(DayMode);
        }
        for (int i = 0; i < nightObjects.Length; i++) {
            nightObjects[i].SetActive(!DayMode);
        }

        if (DayMode) {
            RenderSettings.skybox = daySkyBox;
            RenderSettings.sun = dayDirectionalLight;
            //RenderSettings.fog = false;
            //RenderSettings.fogColor = Color.black;
            //RenderSettings.fogDensity = 0.002f;
            GameManager.CloudsManager.SetCloudData(dayCloudsVolumetric, dayCloudsSimple, false);
            dayCloudsSimple.Stop();
            dayCloudsSimple.Play();
        } else {
            RenderSettings.skybox = nightSkyBox;
            RenderSettings.sun = nightDirectionalLight;
            //RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.6140263f, 0.7088344f, 0.8301887f);
            RenderSettings.fogDensity = 0.01f;
            GameManager.CloudsManager.SetCloudData(nightCloudsVolumetric, nightCloudsSimple, true);
            nightCloudsSimple.Stop();
            nightCloudsSimple.Play();
        }
    }
}
