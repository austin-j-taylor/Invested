using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
 
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CloudMaster : MonoBehaviour {
    const string headerDecoration = " --- ";
    [Header(headerDecoration + "Main" + headerDecoration)]
    public Shader shader;
    public Transform container;
    public Light sunLight;
    public Vector3 cloudTestParams;

    [Header("March settings" + headerDecoration)]
    public int numStepsLight = 8;
    public float rayOffsetStrength;
    public bool especiallyNoisyRayOffsets = false;
    public Texture2D blueNoise;

    [Header(headerDecoration + "Base Shape" + headerDecoration)]
    public float cloudScale = 1;
    public float densityMultiplier = 1;
    public float densityOffset;
    public Vector3 shapeOffset;
    public Vector2 heightOffset;
    public Vector4 shapeNoiseWeights;

    [Header(headerDecoration + "Detail" + headerDecoration)]
    public float detailNoiseScale = 10;
    public float detailNoiseWeight = .1f;
    public Vector3 detailNoiseWeights;
    public Vector3 detailOffset;


    [Header(headerDecoration + "Lighting" + headerDecoration)]
    public float lightAbsorptionThroughCloud = 1;
    public float lightAbsorptionTowardSun = 1;
    [Range(0, 1)]
    public float darknessThreshold = .2f;
    [Range(0, 1)]
    public float forwardScattering = .83f;
    [Range(0, 1)]
    public float backScattering = .3f;
    [Range(0, 1)]
    public float baseBrightness = .8f;
    [Range(0, 1)]
    public float phaseFactor = .15f;

    [Header(headerDecoration + "Animation" + headerDecoration)]
    public float timeScale = 1;
    public float baseSpeed = 1;
    public float detailSpeed = 2;

    [Header(headerDecoration + "Sky and Fog" + headerDecoration)]
    public Color colFog;
    public Color colClouds;
    public Color colSun;
    public bool haveSunInSky = true;
    [Range(0, .1f)]
    public float fogDensity = 0;
    [Header(headerDecoration + "Other" + headerDecoration)]
    public bool cloudsFollowPlayerXZ = true;
    public bool cloudsFollowPlayerXYZ = false;

    // Internal
    [HideInInspector]
    public Material m_Material;

    public WeatherMap weatherMapGen;
    public NoiseGenerator noise;

    bool paramsSet;

    public void Awake() {
        if (weatherMapGen == null)
            weatherMapGen = gameObject.GetComponentInChildren<WeatherMap>();
        if (noise == null)
            noise = gameObject.GetComponentInChildren<NoiseGenerator>();
        if (Application.isPlaying && weatherMapGen) {
            weatherMapGen.containerPosition = container.position;
            weatherMapGen.UpdateMap();
        }
        paramsSet = false;
    }
    private void Update() {
        // Keep the cloud container centered on the player to provide the illusion that the clouds are infinite
        if (container && Player.PlayerInstance && (cloudsFollowPlayerXZ || cloudsFollowPlayerXYZ)) {
            Vector3 position = Player.PlayerInstance.transform.position;

            if(!cloudsFollowPlayerXYZ) {
                position.y = container.position.y;
            }
            container.position = position;
        }
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture src, RenderTexture dest) {

        if (!paramsSet) {
            SetCloudParams(src, dest);
        }

        // Noise
        noise.UpdateNoise();

        m_Material.SetTexture("NoiseTex", noise.shapeTexture);
        m_Material.SetTexture("DetailNoiseTex", noise.detailTexture);
        m_Material.SetTexture("BlueNoise", blueNoise);

        // Weathermap
        if (!Application.isPlaying && weatherMapGen.gameObject != null) {
            weatherMapGen.UpdateMap();
        }
        m_Material.SetTexture("WeatherMap", weatherMapGen.weatherMap);

        // Bit does the following:
        // - sets _MainTex property on m_Material to the source texture
        // - sets the render target to the destination texture
        // - draws a full-screen quad
        // This copies the src texture to the dest texture, with whatever modifications the shader makes
        Graphics.Blit(src, dest, m_Material);
    }

    private void SetCloudParams(RenderTexture src, RenderTexture dest) {
        paramsSet = false; // false for debugging

        // Validate inputs
        if (m_Material == null || m_Material.shader != shader) {
            m_Material = new Material(shader);
        }
        numStepsLight = Mathf.Max(1, numStepsLight);


        Vector3 size = container.localScale;
        int width = Mathf.CeilToInt(size.x);
        int height = Mathf.CeilToInt(size.y);
        int depth = Mathf.CeilToInt(size.z);

        m_Material.SetFloat("scale", cloudScale);
        m_Material.SetFloat("densityMultiplier", densityMultiplier);
        m_Material.SetFloat("densityOffset", densityOffset);
        m_Material.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
        m_Material.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
        m_Material.SetFloat("darknessThreshold", darknessThreshold);
        m_Material.SetVector("sunLightDirection", sunLight.transform.rotation * Vector3.back);
        m_Material.SetVector("params", cloudTestParams);
        m_Material.SetFloat("rayOffsetStrength", rayOffsetStrength);
        m_Material.SetFloat("especiallyNoisyRayOffsets", especiallyNoisyRayOffsets ? 1 : 0);

        m_Material.SetFloat("detailNoiseScale", detailNoiseScale);
        m_Material.SetFloat("detailNoiseWeight", detailNoiseWeight);
        m_Material.SetVector("shapeOffset", shapeOffset);
        m_Material.SetVector("detailOffset", detailOffset);
        m_Material.SetVector("detailWeights", detailNoiseWeights);
        m_Material.SetVector("shapeNoiseWeights", shapeNoiseWeights);
        m_Material.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));

        m_Material.SetVector("boundsMin", container.position - container.localScale / 2);
        m_Material.SetVector("boundsMax", container.position + container.localScale / 2);

        m_Material.SetInt("numStepsLight", numStepsLight);

        m_Material.SetVector("mapSize", new Vector4(width, height, depth, 0));

        m_Material.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
        m_Material.SetFloat("baseSpeed", baseSpeed);
        m_Material.SetFloat("detailSpeed", detailSpeed);

        // Set debug params
        SetDebugParams();

        m_Material.SetColor("colFog", colFog);
        m_Material.SetColor("colClouds", colClouds);
        m_Material.SetColor("colSun", colSun);
        m_Material.SetFloat("haveSunInSky", haveSunInSky ? 1 : 0); // blame unity for not having a setBool
        m_Material.SetFloat("fogDensity", fogDensity);

    }

    void SetDebugParams() {
        int debugModeIndex = 0;
        if (noise.viewerEnabled) {
            debugModeIndex = (noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        }
        if (weatherMapGen.viewerEnabled) {
            debugModeIndex = 3;
        }

        m_Material.SetInt("debugViewMode", debugModeIndex);
        m_Material.SetFloat("debugNoiseSliceDepth", noise.viewerSliceDepth);
        m_Material.SetFloat("debugTileAmount", noise.viewerTileAmount);
        m_Material.SetFloat("viewerSize", noise.viewerSize);
        m_Material.SetVector("debugChannelWeight", noise.ChannelMask);
        m_Material.SetInt("debugGreyscale", (noise.viewerGreyscale) ? 1 : 0);
        m_Material.SetInt("debugShowAllChannels", (noise.viewerShowAllChannels) ? 1 : 0);
    }

}