Shader "Custom/GlowEffect"
{
    Properties
    {
        _BaseColor   ("Base Color", Color) = (1,1,1,1)
        _GlowColor   ("Glow Color", Color) = (0.2,1,1,1)
        _GlowStrength("Glow Strength", Range(0,5)) = 1
        _PulseSpeed  ("Pulse Speed", Range(0,10)) = 3
        [Toggle]_Additive ("Use Additive Blend", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 100

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZWrite Off
            // _Additive 토글에 따라 블렌드 분기 (키워드 불필요, 간단히 두 줄 중 하나만 남겨도 됨)
            Blend One One, One One   // Additive (기본)
            // Blend SrcAlpha OneMinusSrcAlpha // 알파 블렌드 쓰려면 이 줄로 교체

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            float4 _BaseColor;
            float4 _GlowColor;
            float  _GlowStrength;
            float  _PulseSpeed;
            float  _Additive;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 기본 색
                float3 baseCol = _BaseColor.rgb;

                // 시간 기반 펄스
                float glowPulse = 0.5 + 0.5 * sin(_Time.y * _PulseSpeed);

                // 글로우 색
                float3 glow = _GlowColor.rgb * (_GlowStrength * glowPulse);

                // Additive면 알파는 의미 적음. 알파 블렌드 쓰려면 위 Blend 줄 교체.
                float alpha = _BaseColor.a;
                return float4(baseCol + glow, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
