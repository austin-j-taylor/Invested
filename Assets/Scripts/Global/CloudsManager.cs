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
    private float cloudsMaxDensity; // used for fading between densities
    private float currentDensity = 1;
    private bool SceneUsesClouds;

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
            Transform clouds = GameObject.Find("Clouds").transform;
            Transform otherVolumetric = clouds.Find("Volumetric");
            ParticleSystem otherSimple = clouds.Find("Simple").GetComponent<ParticleSystem>();
            if (clouds) {
                //ActiveCamera.clearFlags = CameraClearFlags.SolidColor;
                SetCloudData(otherVolumetric.GetComponent<CloudMaster>(), otherSimple);
            } else {
                // No cloudsVolumetric for this scene
                SceneUsesClouds = false;
                cloudsVolumetric.enabled = false;
                //ActiveCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }

    public void SetClouds(bool enable) {
        if (SceneUsesClouds) {
            if (enable) {
                if (SceneUsesClouds) {
                    cloudsVolumetric.enabled = true;
                    cloudsSimple.gameObject.SetActive(false);
                }
            } else {
                cloudsVolumetric.enabled = false;
                cloudsSimple.gameObject.SetActive(true);
            }
        }
    }
    // Loads cloud settings from the passed CloudMaster.
    public void SetCloudData(CloudMaster otherVolumetric, ParticleSystem otherSimple) {
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

        SceneUsesClouds = true;
        cloudsVolumetric.enabled = GraphicsController.CloudsEnabled;
        cloudsVolumetric.Awake();

        otherVolumetric.enabled = false;

        cloudsMaxDensity = cloudsVolumetric.densityMultiplier;

        // Set params for simple clouds in scene
        cloudsSimple = otherSimple;
        cloudsSimple.gameObject.SetActive(!GraphicsController.CloudsEnabled);
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
            cloudsVolumetric.densityMultiplier = currentDensity * cloudsMaxDensity;
            col.a = currentDensity;
            rend.material.color = col;
            yield return null;
        }
        cloudsVolumetric.densityMultiplier = cloudsMaxDensity;
        col.a = 1;
        rend.material.color = col;
    }
    private IEnumerator CloudsOut(float fadeTime) {
        ParticleSystemRenderer rend = cloudsSimple.GetComponent<ParticleSystemRenderer>();
        Color col = rend.material.color;
        while (currentDensity > 0) {
            currentDensity -= Time.deltaTime / fadeTime;
            cloudsVolumetric.densityMultiplier = currentDensity * cloudsMaxDensity;
            col.a = currentDensity;
            rend.material.color = col;
            yield return null;
        }
        cloudsVolumetric.densityMultiplier = 0;
        col.a = 0;
        rend.material.color = col;
    }
    #endregion
}
