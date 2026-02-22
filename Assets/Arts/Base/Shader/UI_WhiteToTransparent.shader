Shader "Custom/UI_WhiteToTransparent"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Threshold ("White Threshold", Range(0,1)) = 0.9
        _Softness ("Softness", Range(0,1)) = 0.1
        _Tint ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Threshold;
            float _Softness;
            float4 _Tint;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float luminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Tint;

                float lum = luminance(col.rgb);

                float alpha = smoothstep(_Threshold + _Softness, _Threshold - _Softness, lum);

                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }
}
