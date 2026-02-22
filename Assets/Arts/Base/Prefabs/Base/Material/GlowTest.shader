Shader "Custom/GlowTest"
{
    Properties
    {
        _BaseMap     ("Gradient Texture", 2D) = "white" {}
        _Color       ("Glow Color", Color)    = (0.2, 0.6, 1.0, 1.0)
        _Intensity   ("Intensity", Range(0,5)) = 1
        _ScrollSpeed ("Scroll Speed (U)", Float) = 0.5
        _OneSideBias ("One-Side Bias", Range(0,2)) = 1   // 1이면 uv.y 그대로, 2면 더 가파르게 위쪽만 보임
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Name "GlowUnlit"
            Tags { "LightMode"="UniversalForward" }

            // Additive: 발광 느낌
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _Color;
                float  _Intensity;
                float  _ScrollSpeed;
                float  _OneSideBias;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // 텍스처 스케일/오프셋 적용 + U 방향 스크롤
                float2 uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                uv.x += _Time.y * _ScrollSpeed;
                OUT.uv = uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _Color;

                // 위쪽으로만 퍼지게: uv.y(0~1)를 바탕으로 알파/세기 편향
                half glowMask = saturate(pow(IN.uv.y, _OneSideBias)); // Bias↑ → 더 위쪽만 보임

                col.rgb *= _Intensity * glowMask;
                col.a   *= glowMask; // Additive지만 알파도 같이 조절하면 가장자리 정리됨
                return col;
            }
            ENDHLSL
        }
    }
}
