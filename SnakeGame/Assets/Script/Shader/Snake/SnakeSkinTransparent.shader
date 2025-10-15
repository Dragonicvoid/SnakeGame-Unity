Shader "Snake/TransparentWavy"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Texture For Normal Mapping", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Script/Shader/Snake/SnakeLib.cginc"

            #define PI 3.1415

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv0 = i.uv;
                uv0.y += _Time.y * 2.;
                float2 uvI = floor(uv0);
                float2 f = frac(uv0);
                float offset = rand(uvI.y) * 0.5;
                float dist = f.x - 0.25;

                float4 bgCol = float4(0.9686, 0.8549, 0.6235, 0.95);
                float4 waveCol = float4(1., 1., 1., 1.);

                float waveY = (sin((0.5 - dist) / 0.5 * -PI) * 0.2 + 1.0);
                float wave = saturate(1.0 - (when_lt(dist, 0.) + when_gt(dist, 0.5)));
                wave *= when_le(distance(f, float2(f.x, waveY)), 0.025); 

                fixed4 col = lerp(bgCol, waveCol, wave);
                return col;
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Script/Shader/Snake/SnakeLib.cginc"

            #define PI 3.1415

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv0 = i.uv;
                uv0.y += _Time.y * 2.;
                float2 uvI = floor(uv0);
                float2 f = frac(uv0);
                float offset = rand(uvI.y) * 0.5;
                float dist = f.x - 0.25;

                float4 bgCol = float4(0.9686, 0.8549, 0.6235, 1.);
                float4 waveCol = float4(1., 1., 1., 1.);

                float waveY = (sin((0.5 - dist) / 0.5 * -PI) * 0.2 + 1.0);
                float wave = saturate(1.0 - (when_lt(dist, 0.) + when_gt(dist, 0.5)));
                wave *= when_le(distance(f, float2(f.x, waveY)), 0.025); 

                fixed4 col = lerp(bgCol, waveCol, wave);
                return col;
            }
            ENDCG
        }

         Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Script/Shader/Snake/SnakeLib.cginc"

            #define PI 3.1415

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv0 = i.uv;
                uv0.y += _Time.y * 2.;
                float2 uvI = floor(uv0);
                float2 f = frac(uv0);
                float offset = rand(uvI.y) * 0.5;
                float dist = f.x - 0.25;

                float4 bgCol = float4(0.9686, 0.8549, 0.6235, 0.95);
                float4 waveCol = float4(1., 1., 1., 1.);

                float waveY = (sin((0.5 - dist) / 0.5 * -PI) * 0.2 + 1.0);
                float wave = saturate(1.0 - (when_lt(dist, 0.) + when_gt(dist, 0.5)));
                wave *= when_le(distance(f, float2(f.x, waveY)), 0.025); 

                fixed4 col = lerp(bgCol, waveCol, wave);
                return col;
            }
            ENDCG
        }
    }
}
