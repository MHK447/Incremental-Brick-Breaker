Shader "Universal Render Pipeline/Sprites/SpriteOneColor"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _SelfIllum("Self Illumination", Range(0.0, 1.0)) = 0.0
        _FlashAmount("Flash Amount", Range(0.0, 1.0)) = 0.0
        _Color("Tint", Color) = (1, 1, 1, 1)
        
        // URP specific properties
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 10 // OneMinusSrcAlpha
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0 // Off
        [Toggle] _ZWrite("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Name "SpriteOneColor"
            Tags { "LightMode" = "Universal2D" }

            Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // URP doesn't have UnityPixelSnap, so we define it here
            float4 UnityPixelSnap(float4 pos)
            {
                float2 pixelPos = pos.xy / pos.w;
                pixelPos = floor(pixelPos * _ScreenParams.xy + 0.5) / _ScreenParams.xy;
                pos.xy = pixelPos * pos.w;
                return pos;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                half fogCoord : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _FlashAmount;
                half _SelfIllum;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                #if defined(PIXELSNAP_ON)
                output.positionCS = UnityPixelSnap(output.positionCS);
                #endif

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 c = texColor * input.color;

                // Apply flash effect (lerp to white)
                half3 flashedColor = lerp(c.rgb, half3(1.0, 1.0, 1.0), _FlashAmount);
                
                // Apply self illumination
                half3 emission = flashedColor * _SelfIllum;
                
                // Combine albedo and emission
                half3 finalColor = flashedColor + emission;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);

                return half4(finalColor, c.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Sprite Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // URP doesn't have UnityPixelSnap, so we define it here
            float4 UnityPixelSnap(float4 pos)
            {
                float2 pixelPos = pos.xy / pos.w;
                pixelPos = floor(pixelPos * _ScreenParams.xy + 0.5) / _ScreenParams.xy;
                pos.xy = pixelPos * pos.w;
                return pos;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                half fogCoord : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _FlashAmount;
                half _SelfIllum;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                
                #if defined(PIXELSNAP_ON)
                output.positionCS = UnityPixelSnap(output.positionCS);
                #endif

                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 c = texColor * input.color;

                // Apply flash effect (lerp to white)
                half3 flashedColor = lerp(c.rgb, half3(1.0, 1.0, 1.0), _FlashAmount);
                
                // Apply self illumination
                half3 emission = flashedColor * _SelfIllum;
                
                // Combine albedo and emission
                half3 finalColor = flashedColor + emission;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);

                return half4(finalColor, c.a);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/2D/Sprite-Lit-Default"
}

