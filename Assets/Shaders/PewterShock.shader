Shader "Unlit/PewterShock"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Strength("General strength", Float) = 1
		_Intensity("Intensity", Range(0.0,1.0)) = 1
		_HitTime("Time since collision", Range(0.0,1.0)) = 0
		_SourcePosition("Source Location", Vector) = (0, 0, 0, 0)
	}

	SubShader
	{
		Tags {
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		Blend OneMinusSrcAlpha One
		Cull Off
		Zwrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			static const fixed offset = .55, thinness = 2;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float3 objectPos : TEXCOORD3;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float _Intensity;
			float _Strength;
			float _HitTime;
			float4 _SourcePosition; // the source of the collision - local in position, global in rotation.

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.objectPos = v.vertex.xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				if (_HitTime < 0) {
					return fixed4(0, 0, 0, 0);
				} else {
					// sample the texture
					fixed4 col = tex2D(_MainTex, i.uv) * _Color;

					// as time moves on, the source of the shielding moves away from the source
					float3 normal = normalize(_SourcePosition.xyz);
					float3 timeOffset = _HitTime * normal * (1 + offset);
					float dotty = dot(i.objectPos+ timeOffset, normal) + length(i.objectPos) - (offset / 2);

					float glow = 1 - (.9 + .1 *_Intensity) * exp(-thinness * (dotty - 1) * (dotty - 1));

					col.a = saturate(glow / (_Intensity * _Strength));

					return col;
				}
			}
			ENDCG
		}
	}
}
