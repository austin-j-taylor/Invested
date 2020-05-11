﻿using System.Collections;
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
    public Material material;

    public WeatherMap weatherMapGen;
    public NoiseGenerator noise;

    bool paramsSet;

    public void Awake() {
        if (weatherMapGen == null)
            weatherMapGen = gameObject.GetComponentInChildren<WeatherMap>();
        if (noise == null)
            noise = gameObject.GetComponentInChildren<NoiseGenerator>();
        if (Application.isPlaying && weatherMapGen) {
            weatherMapGen.container = container;
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

        material.SetTexture("NoiseTex", noise.shapeTexture);
        material.SetTexture("DetailNoiseTex", noise.detailTexture);
        material.SetTexture("BlueNoise", blueNoise);

        // Weathermap
        if (!Application.isPlaying && weatherMapGen.gameObject != null) {
            weatherMapGen.UpdateMap();
        }
        material.SetTexture("WeatherMap", weatherMapGen.weatherMap);

        // Bit does the following:
        // - sets _MainTex property on material to the source texture
        // - sets the render target to the destination texture
        // - draws a full-screen quad
        // This copies the src texture to the dest texture, with whatever modifications the shader makes
        Graphics.Blit(src, dest, material);
    }

    private void SetCloudParams(RenderTexture src, RenderTexture dest) {
        paramsSet = false; // false for debugging

        // Validate inputs
        if (material == null || material.shader != shader) {
            material = new Material(shader);
        }
        numStepsLight = Mathf.Max(1, numStepsLight);


        Vector3 size = container.localScale;
        int width = Mathf.CeilToInt(size.x);
        int height = Mathf.CeilToInt(size.y);
        int depth = Mathf.CeilToInt(size.z);

        material.SetFloat("scale", cloudScale);
        material.SetFloat("densityMultiplier", densityMultiplier);
        material.SetFloat("densityOffset", densityOffset);
        material.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
        material.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun);
        material.SetFloat("darknessThreshold", darknessThreshold);
        material.SetVector("sunLightDirection", sunLight.transform.rotation * Vector3.back);
        material.SetVector("params", cloudTestParams);
        material.SetFloat("rayOffsetStrength", rayOffsetStrength);
        material.SetFloat("especiallyNoisyRayOffsets", especiallyNoisyRayOffsets ? 1 : 0);

        material.SetFloat("detailNoiseScale", detailNoiseScale);
        material.SetFloat("detailNoiseWeight", detailNoiseWeight);
        material.SetVector("shapeOffset", shapeOffset);
        material.SetVector("detailOffset", detailOffset);
        material.SetVector("detailWeights", detailNoiseWeights);
        material.SetVector("shapeNoiseWeights", shapeNoiseWeights);
        material.SetVector("phaseParams", new Vector4(forwardScattering, backScattering, baseBrightness, phaseFactor));

        material.SetVector("boundsMin", container.position - container.localScale / 2);
        material.SetVector("boundsMax", container.position + container.localScale / 2);

        material.SetInt("numStepsLight", numStepsLight);

        material.SetVector("mapSize", new Vector4(width, height, depth, 0));

        material.SetFloat("timeScale", (Application.isPlaying) ? timeScale : 0);
        material.SetFloat("baseSpeed", baseSpeed);
        material.SetFloat("detailSpeed", detailSpeed);

        // Set debug params
        SetDebugParams();

        material.SetColor("colFog", colFog);
        material.SetColor("colClouds", colClouds);
        material.SetColor("colSun", colSun);
        material.SetFloat("haveSunInSky", haveSunInSky ? 1 : 0); // blame unity for not having a setBool
        material.SetFloat("fogDensity", fogDensity);

    }

    void SetDebugParams() {
        int debugModeIndex = 0;
        if (noise.viewerEnabled) {
            debugModeIndex = (noise.activeTextureType == NoiseGenerator.CloudNoiseType.Shape) ? 1 : 2;
        }
        if (weatherMapGen.viewerEnabled) {
            debugModeIndex = 3;
        }

        material.SetInt("debugViewMode", debugModeIndex);
        material.SetFloat("debugNoiseSliceDepth", noise.viewerSliceDepth);
        material.SetFloat("debugTileAmount", noise.viewerTileAmount);
        material.SetFloat("viewerSize", noise.viewerSize);
        material.SetVector("debugChannelWeight", noise.ChannelMask);
        material.SetInt("debugGreyscale", (noise.viewerGreyscale) ? 1 : 0);
        material.SetInt("debugShowAllChannels", (noise.viewerShowAllChannels) ? 1 : 0);
    }

}