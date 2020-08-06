Shader "Hidden/Shader/CloudsHDRP"
{
    HLSLINCLUDE

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

    struct Attributes
    {
        uint vertexID : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord   : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    Varyings Vert(Attributes input)
    {
        Varyings output;
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
        output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
        return output;
    }

    // List of properties to control post process effect
	// Textures
	Texture3D<float4> NoiseTex;
	Texture3D<float4> DetailNoiseTex;
	Texture2D<float4> WeatherMap;
	Texture2D<float4> BlueNoise;

	SamplerState samplerNoiseTex;
	SamplerState samplerDetailNoiseTex;
	SamplerState samplerWeatherMap;
	SamplerState samplerBlueNoise;

	sampler2D _MainTex;
	//sampler2D _CameraDepthTexture;

	// Shape settings
	float4 params;
	int3 mapSize;
	float densityMultiplier;
	float densityOffset;
	float scale;
	float detailNoiseScale;
	float detailNoiseWeight;
	float3 detailWeights;
	float4 shapeNoiseWeights;
	float4 phaseParams;

	// March settings
	int numStepsLight;
	float rayOffsetStrength;
	float especiallyNoisyRayOffsets; // blame unity for not having a setBool

	float3 boundsMin;
	float3 boundsMax;

	float3 shapeOffset;
	float3 detailOffset;

	// Light settings
	float lightAbsorptionTowardSun;
	float lightAbsorptionThroughCloud;
	float darknessThreshold;
	float4 sunLightDirection;
	float4 colFog; // color of fog in far-distant objects
	float4 colClouds; // color of the clouds in the sky
	float4 colSun; // color of the sun/directional light
	float haveSunInSky; // blame unity for not having a setBool
	float fogDensity;

	// Animation settings
	float timeScale;
	float baseSpeed;
	float detailSpeed;

	// Debug settings:
	int debugViewMode; // 0 = off; 1 = shape tex; 2 = detail tex; 3 = weathermap
	int debugGreyscale;
	int debugShowAllChannels;
	float debugNoiseSliceDepth;
	float4 debugChannelWeight;
	float debugTileAmount;
	float viewerSize;

    TEXTURE2D_X(_InputTexture);

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        uint2 positionSS = input.texcoord * _ScreenSize.xy;
        float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;

        //return float4(outColor, 1);
		return float4(lerp(outColor, Luminance(outColor).xxx, 1), 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "CloudsHDRP"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment CustomPostProcess
                #pragma vertex Vert
            ENDHLSL
        }
    }
    Fallback Off
}
