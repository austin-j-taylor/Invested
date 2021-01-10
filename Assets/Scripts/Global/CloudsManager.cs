using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the cloudsVolumetric/mist effects in the scene.
/// Sets parameters for the CloudsMaster script attached to the player's camera.
/// </summary>
public class CloudsManager : MonoBehaviour {

    private CloudMaster cloudsVolumetric; // perceives cloudsVolumetric on certain levels
    private ParticleSystem cloudsSimple; // the simple clouds built from particles
    private float volumetricMaxDensity; // used for fading between densities
    private float currentDensity = 1;
    private float simpleMaxDensity = 1;
    private int simpleMaxParticlesCount = 0;
    private float simpleMaxParticlesMultiplier = 0;
    private bool sceneUsesClouds, simpleUsesFog; // simpleUsesFog is true if the simple clouds use fog. It's ignored by volumetric clouds, which have their own fog options.

    private void Awake() {
        cloudsVolumetric = CameraController.ActiveCamera.GetComponent<CloudMaster>();
        cloudsSimple = null; // loaded from each scene
        SceneManager.sceneLoaded += LoadCloudDataFromScene;
    }

    #region cloudParameterSetting
    // On scene startup
    // Copy parameters from this scene's cloud controller to the camera
    private void LoadCloudDataFromScene(Scene scene, LoadSceneMode mode) {
        // mode is Single when it's loading scenes on startup, so skip those
        if (mode == LoadSceneMode.Single) {
            GameObject clouds = GameObject.Find("Clouds");
            if (clouds) {
                Transform otherVolumetric = clouds.transform.Find("Volumetric");
                ParticleSystem otherSimple = clouds.transform.Find("Simple").GetComponent<ParticleSystem>();
                //ActiveCamera.clearFlags = CameraClearFlags.SolidColor;
                SetCloudData(otherVolumetric.GetComponent<CloudMaster>(), otherSimple, RenderSettings.fog);
            } else {
                // No cloudsVolumetric for this scene
                sceneUsesClouds = false;
                simpleUsesFog = RenderSettings.fog;
                cloudsVolumetric.enabled = false;
                //ActiveCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }

    public void SetClouds(bool enable) {
        if (sceneUsesClouds) {
            if (enable) { // volumetric
                cloudsVolumetric.enabled = true;
                cloudsSimple.gameObject.SetActive(false);
                RenderSettings.fog = false;
            } else { // simple
                cloudsVolumetric.enabled = false;
                cloudsSimple.gameObject.SetActive(true);
                RenderSettings.fog = simpleUsesFog;
            }
        }
    }
    // Loads cloud settings from the passed CloudMaster.
    public void SetCloudData(CloudMaster otherVolumetric, ParticleSystem otherSimple, bool usingFog) {
        // Set params for volumetric clouds on player camera
        cloudsVolumetric.shader = otherVolumetric.shader;
        cloudsVolumetric.container = otherVolumetric.container;
        cloudsVolumetric.weatherMapGen = otherVolumetric.weatherMapGen;
        cloudsVolumetric.noise = otherVolumetric.noise;
        cloudsVolumetric.sunLight = otherVolumetric.sunLight;
        cloudsVolumetric.cloudTestParams = otherVolumetric.cloudTestParams;

        cloudsVolumetric.numStepsLight = otherVolumetric.numStepsLight;
        cloudsVolumetric.rayOffsetStrength = otherVolumetric.rayOffsetStrength;
        cloudsVolumetric.especiallyNoisyRayOffsets = otherVolumetric.especiallyNoisyRayOffsets;
        cloudsVolumetric.blueNoise = otherVolumetric.blueNoise;

        cloudsVolumetric.cloudScale = otherVolumetric.cloudScale;
        cloudsVolumetric.densityMultiplier = otherVolumetric.densityMultiplier;
        cloudsVolumetric.densityOffset = otherVolumetric.densityOffset;
        cloudsVolumetric.shapeOffset = otherVolumetric.shapeOffset;
        cloudsVolumetric.heightOffset = otherVolumetric.heightOffset;
        cloudsVolumetric.shapeNoiseWeights = otherVolumetric.shapeNoiseWeights;

        cloudsVolumetric.detailNoiseScale = otherVolumetric.detailNoiseScale;
        cloudsVolumetric.detailNoiseWeight = otherVolumetric.detailNoiseWeight;
        cloudsVolumetric.detailNoiseWeights = otherVolumetric.detailNoiseWeights;
        cloudsVolumetric.detailOffset = otherVolumetric.detailOffset;

        cloudsVolumetric.lightAbsorptionThroughCloud = otherVolumetric.lightAbsorptionThroughCloud;
        cloudsVolumetric.lightAbsorptionTowardSun = otherVolumetric.lightAbsorptionTowardSun;
        cloudsVolumetric.darknessThreshold = otherVolumetric.darknessThreshold;
        cloudsVolumetric.forwardScattering = otherVolumetric.forwardScattering;
        cloudsVolumetric.backScattering = otherVolumetric.backScattering;
        cloudsVolumetric.baseBrightness = otherVolumetric.baseBrightness;
        cloudsVolumetric.phaseFactor = otherVolumetric.phaseFactor;

        cloudsVolumetric.timeScale = otherVolumetric.timeScale;
        cloudsVolumetric.baseSpeed = otherVolumetric.baseSpeed;
        cloudsVolumetric.detailSpeed = otherVolumetric.detailSpeed;

        cloudsVolumetric.colFog = otherVolumetric.colFog;
        cloudsVolumetric.colClouds = otherVolumetric.colClouds;
        cloudsVolumetric.colSun = otherVolumetric.colSun;
        cloudsVolumetric.haveSunInSky = otherVolumetric.haveSunInSky;
        cloudsVolumetric.fogDensity = otherVolumetric.fogDensity;

        cloudsVolumetric.cloudsFollowPlayerXZ = otherVolumetric.cloudsFollowPlayerXZ;
        cloudsVolumetric.cloudsFollowPlayerXYZ = otherVolumetric.cloudsFollowPlayerXYZ;

        cloudsVolumetric.material = otherVolumetric.material;

        sceneUsesClouds = true;
        simpleUsesFog = usingFog;
        RenderSettings.fog = !GraphicsController.CloudsEnabled && usingFog;
        cloudsVolumetric.enabled = GraphicsController.CloudsEnabled;
        cloudsVolumetric.Awake();

        otherVolumetric.gameObject.SetActive(false);

        volumetricMaxDensity = cloudsVolumetric.densityMultiplier;

        // Set params for simple clouds in scene
        cloudsSimple = otherSimple;
        ParticleSystemRenderer rend = cloudsSimple.GetComponent<ParticleSystemRenderer>();
        simpleMaxDensity = rend.material.color.a;
        ParticleSystem.MainModule main = cloudsSimple.main;
        simpleMaxParticlesCount = main.maxParticles;
        main.maxParticles = (int)(simpleMaxParticlesMultiplier * simpleMaxParticlesCount);
        cloudsSimple.gameObject.SetActive(!GraphicsController.CloudsEnabled);
    }

    /// <summary>
    /// Sets the multiplier for the count of particles that can appear during simple clouds.
    /// </summary>
    public void SetParticleCount(float multiplier) {
        simpleMaxParticlesMultiplier = multiplier;
        if(cloudsSimple) {
            ParticleSystem.MainModule main = cloudsSimple.main;
            main.maxParticles = (int)(simpleMaxParticlesMultiplier * simpleMaxParticlesCount);
            cloudsSimple.Stop();
            cloudsSimple.Play();
        }
    }
    #endregion

    #region cloudFading
    public void FadeCloudsIn(float time) {
        StopAllCoroutines();
        StartCoroutine(CloudsIn(time));
    }
    public void FadeCloudsOut(float time) {
        StopAllCoroutines();
        StartCoroutine(CloudsOut(time));
    }
    public void FadeCloudsOutImmediate() {
        StopAllCoroutines();
        cloudsVolumetric.densityMultiplier = 0;
        ParticleSystemRenderer rend = cloudsSimple.GetComponent<ParticleSystemRenderer>();
        Color col = rend.material.color;
        col.a = 0;
        rend.material.color = col;
    }
    private IEnumerator CloudsIn(float fadeTime) {
        ParticleSystemRenderer rend = cloudsSimple.GetComponent<ParticleSystemRenderer>();
        Color col = rend.material.color;
        while (currentDensity < 1) {
            currentDensity += Time.deltaTime / fadeTime;
            cloudsVolumetric.densityMultiplier = currentDensity * volumetricMaxDensity;
            col.a = currentDensity * simpleMaxDensity;
            rend.material.color = col;
            yield return null;
        }
        cloudsVolumetric.densityMultiplier = volumetricMaxDensity;
        col.a = simpleMaxDensity;
        rend.material.color = col;
    }
    private IEnumerator CloudsOut(float fadeTime) {
        ParticleSystemRenderer rend = cloudsSimple.GetComponent<ParticleSystemRenderer>();
        Color col = rend.material.color;
        while (currentDensity > 0) {
            currentDensity -= Time.deltaTime / fadeTime;
            cloudsVolumetric.densityMultiplier = currentDensity * volumetricMaxDensity;
            col.a = currentDensity * simpleMaxDensity;
            rend.material.color = col;
            yield return null;
        }
        cloudsVolumetric.densityMultiplier = 0;
        col.a = 0;
        rend.material.color = col;
    }
    #endregion
}
