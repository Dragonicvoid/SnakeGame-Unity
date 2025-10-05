Shader "Snake/RainbowBubble"
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

            // Credit: this Part of code is taken from Shader Toy
            // https://www.shadertoy.com/view/lsfBWs
            float3 rainbow(float level)
            {
                float r = float(level <= 2.0) + float(level > 4.0) * 0.5;
                float g = max(1.0 - abs(level - 2.0) * 0.5, 0.0);
                float b = (1.0 - (level - 4.0) * 0.5) * float(level >= 4.0);
                return float3(r, g, b);
            }

            float3 smoothRainbow (float x)
            {
                float level1 = floor(x*6.0);
                float level2 = min(6.0, floor(x*6.0) + 1.0);
                
                float3 a = rainbow(level1);
                float3 b = rainbow(level2);
                
                return lerp(a, b, frac(x*6.0));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvTex = i.uv;
                uvTex.y = frac(uvTex.y + _Time.x);

                float2 uvRainbow = i.uv;
                uvRainbow.x = frac(uvRainbow.x + _Time.y);
                float3 rainbow = smoothRainbow(frac(uvRainbow.x + i.uv.y));

                float4 tex = tex2D(_MainTex, uvTex);
                float4 col = float4(rainbow.r, rainbow.g, rainbow.b, 1.0);

                return lerp(col, tex, tex.a);
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

            // Credit: this Part of code is taken from Shader Toy
            // https://www.shadertoy.com/view/lsfBWs
            float3 rainbow(float level)
            {
                float r = float(level <= 2.0) + float(level > 4.0) * 0.5;
                float g = max(1.0 - abs(level - 2.0) * 0.5, 0.0);
                float b = (1.0 - (level - 4.0) * 0.5) * float(level >= 4.0);
                return float3(r, g, b);
            }

            float3 smoothRainbow (float x)
            {
                float level1 = floor(x*6.0);
                float level2 = min(6.0, floor(x*6.0) + 1.0);
                
                float3 a = rainbow(level1);
                float3 b = rainbow(level2);
                
                return lerp(a, b, frac(x*6.0));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvTex = i.uv;
                uvTex.y = frac(uvTex.y + _Time.x);

                float2 uvRainbow = i.uv;
                uvRainbow.x = frac(uvRainbow.x + _Time.y);
                float3 rainbow = smoothRainbow(frac(uvRainbow.x + i.uv.y));

                float4 col = tex2D(_MainTex, uvTex);
                col.rgb *= rainbow;
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

            // Credit: this Part of code is taken from Shader Toy
            // https://www.shadertoy.com/view/lsfBWs
            float3 rainbow(float level)
            {
                float r = float(level <= 2.0) + float(level > 4.0) * 0.5;
                float g = max(1.0 - abs(level - 2.0) * 0.5, 0.0);
                float b = (1.0 - (level - 4.0) * 0.5) * float(level >= 4.0);
                return float3(r, g, b);
            }

            float3 smoothRainbow (float x)
            {
                float level1 = floor(x*6.0);
                float level2 = min(6.0, floor(x*6.0) + 1.0);
                
                float3 a = rainbow(level1);
                float3 b = rainbow(level2);
                
                return lerp(a, b, frac(x*6.0));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvTex = i.uv;
                uvTex.y = frac(uvTex.y + _Time.x);

                float2 uvRainbow = i.uv;
                uvRainbow.x = frac(uvRainbow.x + _Time.y);
                float3 rainbow = smoothRainbow(frac(uvRainbow.x + i.uv.y));

                float4 col = tex2D(_MainTex, uvTex);
                col.rgb *= rainbow;
                return col;
            }
            ENDCG
        }
    }
}
