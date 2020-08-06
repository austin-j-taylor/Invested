using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/CloudsHDRP")]
public sealed class CloudsHDRP : CustomPostProcessVolumeComponent, IPostProcessComponent {

    const string headerDecoration = " --- ";

    //[Header(headerDecoration + "Main" + headerDecoration)]
    //public LightParameter sunLight = new LightParameter(null);
    //public TransformParameter container = new TransformParameter(null);

    [Header("March settings" + headerDecoration)]
    public MinIntParameter numStepsLight = new MinIntParameter(8, 1);
    public FloatParameter rayOffsetStrength = new FloatParameter(0);
    public BoolParameter especiallyNoisyRayOffsets = new BoolParameter(false);
    public TextureParameter blueNoise = new TextureParameter(null);

    [Header(headerDecoration + "Base Shape" + headerDecoration)]
    public FloatParameter cloudScale = new FloatParameter(1);
    public FloatParameter densityMultiplier = new FloatParameter(1);
    public FloatParameter densityOffset = new FloatParameter(0);
    public Vector3Parameter shapeOffset = new Vector3Parameter(new Vector3());
    public Vector2Parameter heightOffset = new Vector2Parameter(new Vector2());
    public Vector4Parameter shapeNoiseWeights = new Vector4Parameter(new Vector4());

    [Header(headerDecoration + "Detail" + headerDecoration)]
    public FloatParameter detailNoiseScale = new FloatParameter(10);
    public FloatParameter detailNoiseWeight = new FloatParameter(.1f);
    public Vector3Parameter detailNoiseWeights = new Vector3Parameter(new Vector3());
    public Vector3Parameter detailOffset = new Vector3Parameter(new Vector3());

    [Header(headerDecoration + "Lighting" + headerDecoration)]
    public FloatParameter lightAbsorptionThroughCloud = new FloatParameter(1);
    public FloatParameter lightAbsorptionTowardSun = new FloatParameter(1);
    public ClampedFloatParameter darknessThreshold = new ClampedFloatParameter(.2f, 0, 1);
    public ClampedFloatParameter forwardScattering = new ClampedFloatParameter(.83f, 0, 1);
    public ClampedFloatParameter backScattering = new ClampedFloatParameter(.3f, 0, 1);
    public ClampedFloatParameter baseBrightness = new ClampedFloatParameter(.8f, 0, 1);
    public ClampedFloatParameter phaseFactor = new ClampedFloatParameter(.15f, 0, 1);

    [Header(headerDecoration + "Animation" + headerDecoration)]
    public FloatParameter timeScale = new FloatParameter(1);
    public FloatParameter baseSpeed = new FloatParameter(1);
    public FloatParameter detailSpeed = new FloatParameter(3);

    [Header(headerDecoration + "Sky and Fog" + headerDecoration)]
    public ColorParameter colFog = new ColorParameter(Color.black);
    public ColorParameter colClouds = new ColorParameter(Color.black);
    public ColorParameter colSun = new ColorParameter(Color.black);
    public BoolParameter haveSunInSky = new BoolParameter(true);
    public ClampedFloatParameter fogDensity = new ClampedFloatParameter(0, 0, .1f);

    [Header(headerDecoration + "Other" + headerDecoration)]
    public BoolParameter cloudsFollowPlayerXZ = new BoolParameter(false);
    public BoolParameter cloudsFollowPlayerXYZ = new BoolParameter(true);

    // Internal
    private Material m_Material;
    private WeatherMap weatherMapGen;
    private NoiseGenerator noise;

    bool paramsSet = false;

    public bool IsActive() => m_Material != null;
    //public bool IsActive() => m_Material != null && container.value != null && sunLight.value != null;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/Shader/CloudsHDRP";

    public override void Setup() {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume CloudsHDRP is unable to load.");

        

        //if (weatherMapGen == null && container.value != null)
        //    weatherMapGen = container.value.gameObject.GetComponentInChildren<WeatherMap>();
        //if (noise == null && container.value != null)
        //    noise = container.value.gameObject.GetComponentInChildren<NoiseGenerator>();
        //if (Application.isPlaying && weatherMapGen) {
        //    weatherMapGen.container = container.value;
        //    weatherMapGen.UpdateMap();
        //}
        paramsSet = false;
    }

    //[ImageEffectOpaque]
    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination) {
        if (m_Material == null)
            return;

        //// Keep the cloud container.value centered on the player to provide the illusion that the clouds are infinite
        //if (container.value != null && Player.PlayerInstance && (cloudsFollowPlayerXZ.value || cloudsFollowPlayerXYZ.value)) {
        //    Vector3 position = Player.PlayerInstance.transform.position;

        //    if (!cloudsFollowPlayerXYZ.value) {
        //        position.y = container.value.position.y;
        //    }
        //    container.value.position = position;
        //}

        RenderTexture src = source.rt, dest = destination.rt;
        if (!paramsSet) {
            SetCloudParams(src, dest);
        }

        // Noise
        //noise.UpdateNoise();

        //m_Material.SetTexture("NoiseTex", noise.shapeTexture);
        //m_Material.SetTexture("DetailNoiseTex", noise.detailTexture);
        //m_Material.SetTexture("BlueNoise", blueNoise.value);

        //// Weathermap
        //if (!Application.isPlaying && weatherMapGen.gameObject != null) {
        //    weatherMapGen.UpdateMap();
        //}
        //m_Material.SetTexture("WeatherMap", weatherMapGen.weatherMap);

        //// Bit does the following:
        //// - sets _MainTex property on m_Material to the source texture
        //// - sets the render target to the destination texture
        //// - draws a full-screen quad
        //// This copies the src texture to the dest texture, with whatever modifications the shader makes
        //Graphics.Blit(src, dest, m_Material);

        // Set shader properties
        m_Material.SetTexture("_InputTexture", source);
        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }
    private void SetCloudParams(RenderTexture src, RenderTexture dest) {
        paramsSet = false; // false for debugging

        //Vector3 size = container.value.localScale;
        //int height = Mathf.CeilToInt(size.y);
        //int width = Mathf.CeilToInt(size.x);
        //int depth = Mathf.CeilToInt(size.z);

        m_Material.SetFloat("scale", cloudScale.value);
        m_Material.SetFloat("densityMultiplier", densityMultiplier.value);
        m_Material.SetFloat("densityOffset", densityOffset.value);
        m_Material.SetFloat("lightAbsorptionThroughCloud", lightAbsorptionThroughCloud.value);
        m_Material.SetFloat("lightAbsorptionTowardSun", lightAbsorptionTowardSun.value);
        m_Material.SetFloat("darknessThreshold", darknessThreshold.value);
        //m_Material.SetVector("sunLightDirection", sunLight.value.transform.rotation * Vector3.back);
        m_Material.SetFloat("rayOffsetStrength", rayOffsetStrength.value);
        m_Material.SetFloat("especiallyNoisyRayOffsets", especiallyNoisyRayOffsets.value ? 1 : 0);

        m_Material.SetFloat("detailNoiseScale", detailNoiseScale.value);
        m_Material.SetFloat("detailNoiseWeight", detailNoiseWeight.value);
        m_Material.SetVector("shapeOffset", shapeOffset.value);
        m_Material.SetVector("detailOffset", detailOffset.value);
        m_Material.SetVector("detailWeights", detailNoiseWeights.value);
        m_Material.SetVector("shapeNoiseWeights", shapeNoiseWeights.value);
        m_Material.SetVector("phaseParams", new Vector4(forwardScattering.value, backScattering.value, baseBrightness.value, phaseFactor.value));

        //m_Material.SetVector("boundsMin", container.value.position - container.value.localScale / 2);
        //m_Material.SetVector("boundsMax", container.value.position + container.value.localScale / 2);

        m_Material.SetInt("numStepsLight", numStepsLight.value);

        //m_Material.SetVector("mapSize", new Vector4(width, height, depth, 0));

        m_Material.SetFloat("timeScale", (Application.isPlaying) ? timeScale.value : 0);
        m_Material.SetFloat("baseSpeed", baseSpeed.value);
        m_Material.SetFloat("detailSpeed", detailSpeed.value);

        m_Material.SetColor("colFog", colFog.value);
        m_Material.SetColor("colClouds", colClouds.value);
        m_Material.SetColor("colSun", colSun.value);
        m_Material.SetFloat("haveSunInSky", haveSunInSky.value ? 1 : 0); // blame unity for not having a setBool
        m_Material.SetFloat("fogDensity", fogDensity.value);
    }

    public override void Cleanup() {
        CoreUtils.Destroy(m_Material);
    }

    [Serializable]
    public sealed class LightParameter : VolumeParameter<Light> {
        public LightParameter(Light value, bool overrideState = false)
            : base(value, overrideState) { }
    }
    [Serializable]
    public sealed class TransformParameter : VolumeParameter<Transform> {
        public TransformParameter(Transform value, bool overrideState = false)
            : base(value, overrideState) { }
    }

}
