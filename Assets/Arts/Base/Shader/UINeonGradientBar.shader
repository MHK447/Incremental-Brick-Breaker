Shader "UI/NeonGradientBar_FilledCompatible"
{
    Properties
    {
        _ColorA ("Start Color (0%)", Color) = (0,1,1,1)
        _ColorB ("Middle Color (50%)", Color) = (0.4,1,1,1)
        _ColorC ("End Color (100%)", Color) = (1,1,1,1)
        
        [PerRendererData]_MainTex("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _ColorA, _ColorB, _ColorC;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 baseTex = tex2D(_MainTex, i.uv) * i.color;

                // ✅ 핵심: Image.fillAmount 적용된 UV.x 영역만 살아있음
                float t = saturate(i.uv.x);

                // A → B → C 단계 그라데이션
                fixed4 g = (t < 0.5)
                    ? lerp(_ColorA, _ColorB, t * 2)
                    : lerp(_ColorB, _ColorC, (t - 0.5) * 2);

                g.a = baseTex.a;
                return g;
            }
            ENDCG
        }
    }
}
