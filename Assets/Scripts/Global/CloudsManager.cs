using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the clouds/mist effects in the scene.
/// Sets parameters for the CloudsMaster script attached to the player's camera.
/// </summary>
public class CloudsManager : MonoBehaviour {

    private CloudMaster clouds; // perceives clouds on certain levels
    private float cloudsMaxDensity; // used for fading between densities
    private bool SceneUsesClouds;

    private void Awake() {
        clouds = CameraController.ActiveCamera.GetComponent<CloudMaster>();
        SceneManager.sceneLoaded += LoadCloudDataFromScene;
    }

    #region cloudParameterSetting
    // On scene startup
    // Copy parameters from this scene's cloud controller to the camera
    private void LoadCloudDataFromScene(Scene scene, LoadSceneMode mode) {
        // mode is Single when it's loading scenes on startup, so skip those
        if (mode == LoadSceneMode.Single) {
            GameObject otherObject = GameObject.Find("Clouds");
            if (otherObject) {
                //ActiveCamera.clearFlags = CameraClearFlags.SolidColor;
                CloudMaster other = otherObject.GetComponent<CloudMaster>();

                SetCloudData(other);
            } else {
                // No clouds for this scene
                SceneUsesClouds = false;
                clouds.enabled = false;
                //ActiveCamera.clearFlags = CameraClearFlags.Skybox;
            }
        }
    }

    public void SetClouds(bool enable) {
        if (enable) {
            if (SceneUsesClouds)
                clouds.enabled = true;
        } else {
            clouds.enabled = false;
        }
    }
    // Loads cloud settings from the passed CloudMaster.
    public void SetCloudData(CloudMaster other) {
        clouds.shader = other.shader;
        clouds.container = other.container;
        clouds.weatherMapGen = other.weatherMapGen;
        clouds.noise = other.noise;
        clouds.sunLight = other.sunLight;
        clouds.cloudTestParams = other.cloudTestParams;

        clouds.numStepsLight = other.numStepsLight;
        clouds.rayOffsetStrength = other.rayOffsetStrength;
        clouds.especiallyNoisyRayOffsets = other.especiallyNoisyRayOffsets;
        clouds.blueNoise = other.blueNoise;

        clouds.cloudScale = other.cloudScale;
        clouds.densityMultiplier = other.densityMultiplier;
        clouds.densityOffset = other.densityOffset;
        clouds.shapeOffset = other.shapeOffset;
        clouds.heightOffset = other.heightOffset;
        clouds.shapeNoiseWeights = other.shapeNoiseWeights;

        clouds.detailNoiseScale = other.detailNoiseScale;
        clouds.detailNoiseWeight = other.detailNoiseWeight;
        clouds.detailNoiseWeights = other.detailNoiseWeights;
        clouds.detailOffset = other.detailOffset;

        clouds.lightAbsorptionThroughCloud = other.lightAbsorptionThroughCloud;
        clouds.lightAbsorptionTowardSun = other.lightAbsorptionTowardSun;
        clouds.darknessThreshold = other.darknessThreshold;
        clouds.forwardScattering = other.forwardScattering;
        clouds.backScattering = other.backScattering;
        clouds.baseBrightness = other.baseBrightness;
        clouds.phaseFactor = other.phaseFactor;

        clouds.timeScale = other.timeScale;
        clouds.baseSpeed = other.baseSpeed;
        clouds.detailSpeed = other.detailSpeed;

        clouds.colFog = other.colFog;
        clouds.colClouds = other.colClouds;
        clouds.colSun = other.colSun;
        clouds.haveSunInSky = other.haveSunInSky;
        clouds.fogDensity = other.fogDensity;

        clouds.cloudsFollowPlayerXZ = other.cloudsFollowPlayerXZ;
        clouds.cloudsFollowPlayerXYZ = other.cloudsFollowPlayerXYZ;

        clouds.material = other.material;

        SceneUsesClouds = true;
        clouds.enabled = GraphicsController.CloudsEnabled;
        clouds.Awake();

        other.enabled = false;

        cloudsMaxDensity = clouds.densityMultiplier;
    }
    #endregion

    #region cloudFading
    public void FadeCloudsIn(float time) {
        StartCoroutine(CloudsIn(time));
    }
    public void FadeCloudsOut(float time) {
        StartCoroutine(CloudsOut(time));
    }
    public void FadeCloudsOutImmediate() {
        clouds.densityMultiplier = 0;
    }
    private IEnumerator CloudsIn(float fadeTime) {
        float density = 0;
        if (cloudsMaxDensity < 0) {
            while (density > cloudsMaxDensity) {
                density += Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        } else {
            while (density < cloudsMaxDensity) {
                density += Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        }
        clouds.densityMultiplier = cloudsMaxDensity;
    }
    private IEnumerator CloudsOut(float fadeTime) {
        float density = cloudsMaxDensity;
        if (cloudsMaxDensity < 0) {
            while (density < 0) {
                density -= Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        } else {
            while (density > 0) {
                density -= Time.deltaTime / fadeTime * cloudsMaxDensity;
                clouds.densityMultiplier = density;
                yield return null;
            }
        }
        clouds.densityMultiplier = 0;
    }
    #endregion
}
