Shader "Allomancy/RefillingTube"
{
	Properties
	{
		[PerRendererData]_Fill("Fill", Range(0.0, 1.0)) = 0
		_Color("Color", Color) = (1,1,1,1)
		_Color("ColorInvested", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

		float _Fill;
		half _Glossiness;
		half _Metallic;
		fixed4 _Color, _ColorInvested;
		fixed4 _EmissionColor;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			if (_Fill - IN.uv_MainTex.x >= 0) {
				o.Albedo = _ColorInvested;
				o.Emission = _EmissionColor;
			} else {
				o.Albedo = _Color;
				o.Emission = 0;
			}

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
		Fallback "Diffuse"
}
