Shader "Unlit/PewterShock"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Speed("speed", Float) = 1
		_Intensity("Intensity", Range(0.0,1.0)) = 1
		_HitTime("Time since collision", Float) = 0 
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
			float _Speed;
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
			// sample the texture
			fixed4 col = tex2D(_MainTex, i.uv) * _Color;

			float3 localpos = _SourcePosition.xyz - i.objectPos;
			col.a = saturate(length(localpos) / _Intensity);
			return col;
			}
			ENDCG
		}
	}
}
