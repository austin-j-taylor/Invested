Shader "Unlit/Hologram"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Transparency("Transparency", Range(0.0,0.5)) = 0.25
		_CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0.2
			_Distance("distance", Float) = 1
			_Amplitude("amplit", Float) = 1
			_Amplitudetrans("amplitTrans", Float) = 1
			_Speed("speed", Float) = 1
			_Speedtrans("speedtrans", Float) = 1
			_Amount("amount", Range(0.0,1.0)) = 1
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _TintColor;
			float _Transparency;
			float _CutoutThresh;
			float _Amplitude;
			float _Amplitudetrans;
			float _Distance;
			float _Amount;
			float _Speed;
			float _Speedtrans;

            v2f vert (appdata v)
            {
                v2f o;
				v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Distance * _Amount;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _TintColor;
				col.a = _Transparency * sin(_Time.y * _Speedtrans + i.vertex.y * _Amplitudetrans) * _Distance;
				clip(col.r - _CutoutThresh);
                return col;
            }
            ENDCG
        }
    }
}
