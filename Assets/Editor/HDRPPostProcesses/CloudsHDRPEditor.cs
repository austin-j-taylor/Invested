using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor;

[VolumeComponentEditor(typeof(CloudsHDRP))]

sealed class GrayScaleEditor : VolumeComponentEditor {

    //SerializedDataParameter m_sunLight;
    //SerializedDataParameter m_container;

    SerializedDataParameter m_numStepsLight;
    SerializedDataParameter m_rayOffsetStrength;
    SerializedDataParameter m_especiallyNoisyRayOffsets;
    SerializedDataParameter m_blueNoise;

    SerializedDataParameter m_cloudScale;
    SerializedDataParameter m_densityMultiplier;
    SerializedDataParameter m_densityOffset;
    SerializedDataParameter m_shapeOffset;
    SerializedDataParameter m_heightOffset;
    SerializedDataParameter m_shapeNoiseWeights;

    SerializedDataParameter m_detailNoiseScale;
    SerializedDataParameter m_detailNoiseWeight;
    SerializedDataParameter m_detailNoiseWeights;
    SerializedDataParameter m_detailOffset;

    SerializedDataParameter m_lightAbsorptionThroughCloud;
    SerializedDataParameter m_lightAbsorptionTowardSun;
    SerializedDataParameter m_darknessThreshold;
    SerializedDataParameter m_forwardScattering;
    SerializedDataParameter m_backScattering;
    SerializedDataParameter m_baseBrightness;
    SerializedDataParameter m_phaseFactor;

    SerializedDataParameter m_timeScale;
    SerializedDataParameter m_baseSpeed;
    SerializedDataParameter m_detailSpeed;

    SerializedDataParameter m_colFog;
    SerializedDataParameter m_colClouds;
    SerializedDataParameter m_colSun;
    SerializedDataParameter m_haveSunInSky;
    SerializedDataParameter m_fogDensity;

    SerializedDataParameter m_cloudsFollowPlayerXZ;
    SerializedDataParameter m_cloudsFollowPlayerXYZ;

    public override bool hasAdvancedMode => false;

    public override void OnEnable() {
        base.OnEnable();
        PropertyFetcher<CloudsHDRP> o = new PropertyFetcher<CloudsHDRP>(serializedObject);

        CloudsHDRP clouds = target as CloudsHDRP;
        //m_sunLight = Unpack(o.Find(x => x.sunLight));
        //m_container = Unpack(o.Find(x => x.container));

        m_numStepsLight = Unpack(o.Find(x => x.numStepsLight));
        m_rayOffsetStrength = Unpack(o.Find(x => x.rayOffsetStrength));
        m_especiallyNoisyRayOffsets = Unpack(o.Find(x => x.especiallyNoisyRayOffsets));
        m_blueNoise = Unpack(o.Find(x => x.blueNoise));

        m_cloudScale = Unpack(o.Find(x => x.cloudScale));
        m_densityMultiplier = Unpack(o.Find(x => x.densityMultiplier));
        m_densityOffset = Unpack(o.Find(x => x.densityOffset));
        m_shapeOffset = Unpack(o.Find(x => x.shapeOffset));
        m_heightOffset = Unpack(o.Find(x => x.heightOffset));
        m_shapeNoiseWeights = Unpack(o.Find(x => x.shapeNoiseWeights));

        m_detailNoiseScale = Unpack(o.Find(x => x.detailNoiseScale));
        m_detailNoiseWeight = Unpack(o.Find(x => x.detailNoiseWeight));
        m_detailNoiseWeights = Unpack(o.Find(x => x.detailNoiseWeights));
        m_detailOffset = Unpack(o.Find(x => x.detailOffset));

        m_lightAbsorptionThroughCloud = Unpack(o.Find(x => x.lightAbsorptionThroughCloud));
        m_lightAbsorptionTowardSun = Unpack(o.Find(x => x.lightAbsorptionTowardSun));
        m_darknessThreshold = Unpack(o.Find(x => x.darknessThreshold));
        m_forwardScattering = Unpack(o.Find(x => x.forwardScattering));
        m_backScattering = Unpack(o.Find(x => x.backScattering));
        m_baseBrightness = Unpack(o.Find(x => x.baseBrightness));
        m_phaseFactor = Unpack(o.Find(x => x.phaseFactor));

        m_timeScale = Unpack(o.Find(x => x.timeScale));
        m_baseSpeed = Unpack(o.Find(x => x.baseSpeed));
        m_detailSpeed = Unpack(o.Find(x => x.detailSpeed));

        m_colFog = Unpack(o.Find(x => x.colFog));
        m_colClouds = Unpack(o.Find(x => x.colClouds));
        m_colSun = Unpack(o.Find(x => x.colSun));
        m_haveSunInSky = Unpack(o.Find(x => x.haveSunInSky));
        m_fogDensity = Unpack(o.Find(x => x.fogDensity));

        m_cloudsFollowPlayerXZ = Unpack(o.Find(x => x.cloudsFollowPlayerXZ));
        m_cloudsFollowPlayerXYZ = Unpack(o.Find(x => x.cloudsFollowPlayerXYZ));
    }

    public override void OnInspectorGUI() {
        GUILayout.Label("Cloud Parameters");
        CloudsHDRP clouds = target as CloudsHDRP;
        //PropertyField(m_sunLight);
        //PropertyField(m_container);

        PropertyField(m_numStepsLight);
        PropertyField(m_rayOffsetStrength);
        PropertyField(m_especiallyNoisyRayOffsets);
        PropertyField(m_blueNoise);

        PropertyField(m_cloudScale);
        PropertyField(m_densityMultiplier);
        PropertyField(m_densityOffset);
        PropertyField(m_shapeOffset);
        PropertyField(m_heightOffset);
        PropertyField(m_shapeNoiseWeights);

        PropertyField(m_detailNoiseScale);
        PropertyField(m_detailNoiseWeight);
        PropertyField(m_detailNoiseWeights);
        PropertyField(m_detailOffset);

        PropertyField(m_lightAbsorptionThroughCloud);
        PropertyField(m_lightAbsorptionTowardSun);
        PropertyField(m_darknessThreshold);
        PropertyField(m_forwardScattering);
        PropertyField(m_backScattering);
        PropertyField(m_baseBrightness);
        PropertyField(m_phaseFactor);

        PropertyField(m_timeScale);
        PropertyField(m_baseSpeed);
        PropertyField(m_detailSpeed);

        PropertyField(m_colFog);
        PropertyField(m_colClouds);
        PropertyField(m_colSun);
        PropertyField(m_haveSunInSky);
        PropertyField(m_fogDensity);

        PropertyField(m_cloudsFollowPlayerXZ);
        PropertyField(m_cloudsFollowPlayerXYZ);
    }
}