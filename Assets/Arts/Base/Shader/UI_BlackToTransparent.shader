Shader "Custom/UI_BlackToTransparent"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _BlackThreshold ("Black Threshold", Range(0,1)) = 0.1  // 검정색으로 인식할 밝기 임계값
        _Threshold ("Alpha Threshold", Range(0,1)) = 0.01   // 거의 투명한 픽셀만
        _Tint ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }

        Cull Off
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
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlackThreshold;
            float _Threshold;
            float4 _Tint;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Tint;
                
                // 이미 거의 투명한 픽셀은 그냥 버림
                if (col.a < _Threshold)
                {
                    discard;
                }

                // RGB의 밝기 계산 (Luminance)
                float brightness = dot(col.rgb, float3(0.299, 0.587, 0.114));
                
                // 검정색 판별: 밝기가 임계값보다 낮으면 투명하게
                if (brightness < _BlackThreshold)
                {
                    col.a = 0.0;
                }

                return col;
            }
            ENDCG
        }
    }
}
