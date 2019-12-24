Shader "CrosshairCircle"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (0,0,0,0)
		_Radius("Radius", Range(0, .7071)) = .5
		_Width("Width", Range(0, .5)) = .1
		_Fill("Fill", Range(0, 1)) = 1
	}
		SubShader
		{
			Blend SrcAlpha OneMinusSrcAlpha

			Tags
			{
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
			}
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				static const fixed low = .666, high = 1.5;

				struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f {
					float2 uv : TEXCOORD0;
					float4 screenPos : TEXCOORD2;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v) {
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.screenPos = ComputeScreenPos(o.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				fixed4 _Color;
				float _Radius;
				float _Width;
				float _Fill;

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = _Color;//tex2D(_MainTex, i.uv);

					float x = i.uv.x - .5;
					float y = i.uv.y - .5;

					float ratio = y / x;
					float2 loc = float2(x, y);
					float d = distance(loc, float2(0, 0));

					if (x > 0 && y < 0 || x < 0 && y > 0) {
						ratio = -ratio;
					}

					if (ratio < low || ratio > high) {
						return float4(0,0,0,0);
					} else {
						if (d > _Radius || d < _Radius - _Width) {
							return float4(0, 0, 0, 0);
						} else if (_Radius - d < _Width * (1 - _Fill)) {
							col.x = 1;
							col.y = 1;
							col.z = 1;
							col.w = .5;
						}
					}

					return col;
				}
				ENDCG
			}
		}
}
