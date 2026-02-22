Shader "UI/NeonGlow"
{
    Properties
    {
        [PerRendererData]_MainTex("Sprite", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)

        // ✅ 이 값 키우면 더 밝게 퍼짐
        _GlowPower ("Glow Power", Float) = 1.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
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
            float _GlowPower;

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
                // ✅ 원래 Sprite + Color 적용
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                // ✅ 단순히 밝기 증폭 (HDR 가능)
                c.rgb *= _GlowPower;

                return c;
            }
            ENDCG
        }
    }
}
