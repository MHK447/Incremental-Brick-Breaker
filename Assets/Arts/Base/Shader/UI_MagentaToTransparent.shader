Shader "Custom/UI_MagentaToTransparents"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _HueCenter ("Magenta Hue", Range(0,1)) = 0.80      // 마젠타~보라 중심 (288°)
        _HueRange ("Hue Range", Range(0,0.5)) = 0.12       // 넓은 범위로 보라~마젠타 모두 포함
        _SaturationMin ("Minimum Saturation", Range(0,1)) = 0.3  // 낮춰서 연한 보라도 포함
        _ValueMin ("Minimum Value", Range(0,1)) = 0.2      // 어두운 보라도 포함
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
            float _HueCenter;
            float _HueRange;
            float _SaturationMin;
            float _ValueMin;
            float _Threshold;
            float4 _Tint;

            // RGB → HSV 변환 (개선된 버전)
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                
                float hue = abs(q.z + (q.w - q.y) / (6.0 * d + e));
                float sat = d / (q.x + e);
                float val = q.x;
                
                return float3(hue, sat, val);
            }

            // Hue 거리 계산 (순환 특성 고려)
            float hueDist(float h1, float h2)
            {
                float diff = abs(h1 - h2);
                // 0과 1이 연결되어 있으므로 짧은 경로 선택
                return min(diff, 1.0 - diff);
            }

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

                float3 hsv = rgb2hsv(col.rgb);
                float hue = hsv.x;
                float sat = hsv.y;
                float val = hsv.z;

                // 마젠타 계열 판별 (순환 거리 사용)
                float dist = hueDist(hue, _HueCenter);
                
                // 조건: 
                // 1. Hue가 마젠타 범위 안에 있고
                // 2. 채도가 최소값 이상이고 (회색/흰색 제외)
                // 3. 밝기가 최소값 이상일 때 (완전 검은색 제외)
                if (dist < _HueRange && sat > _SaturationMin && val > _ValueMin)
                {
                    // discard 대신 알파만 0으로 설정 (Bloom 효과 유지)
                    col.a = 0.0;
                }

                return col;
            }
            ENDCG
        }
    }
}
