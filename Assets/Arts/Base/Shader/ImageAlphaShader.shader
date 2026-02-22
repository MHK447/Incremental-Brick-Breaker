Shader "Custom/ImageAlphaShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 3.0
        _GlowSize ("Glow Size", Range(0, 0.1)) = 0.02
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness Boost", Range(1, 5)) = 2.0
        _Tint ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }

        Cull Off
        ZWrite Off
        Blend One One  // Additive blending for strong glow

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
            float _GlowIntensity;
            float _GlowSize;
            float4 _GlowColor;
            float _Brightness;
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
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Glow 샘플링 (주변 픽셀들을 샘플링하여 블러 효과)
                float4 glowSum = float4(0, 0, 0, 0);
                float samples = 16.0;
                
                for(float angle = 0; angle < 6.28318; angle += 6.28318 / samples)
                {
                    float2 offset = float2(cos(angle), sin(angle)) * _GlowSize;
                    glowSum += tex2D(_MainTex, i.uv + offset);
                    glowSum += tex2D(_MainTex, i.uv + offset * 0.5);
                }
                
                glowSum /= (samples * 2.0);
                
                // 원본 이미지 밝기 부스트
                col.rgb *= _Brightness;
                
                // Glow 효과 추가
                float4 glow = glowSum * _GlowColor * _GlowIntensity;
                
                // 최종 색상 = 밝아진 원본 + 강력한 Glow
                col.rgb += glow.rgb;
                col.rgb *= _Tint.rgb;
                
                // 알파는 원본과 Glow의 합
                col.a = saturate(col.a + glow.a * 0.5);
                
                return col;
            }
            ENDCG
        }
    }
}
