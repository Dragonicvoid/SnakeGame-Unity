Shader "Snake/Wither"
{
    //https://www.filterforge.com/filters/12721-normal.html
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

            sampler2D _NormalMap;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // For Main Body Effect
                float3 lightDir = float3(1, 0, 1);
                float2 uv0Main = i.uv;
                uv0Main.y = frac(uv0Main.y + _Time.y);

                // For ink effect
                float inkSize = 6;
                float2 uv0Second = ((float2(i.uv.x, frac(i.uv.y + _Time.y * 0.5)) - 0.5) * 2) * inkSize;
                float2 fUVSecond = frac(uv0Second);
                float2 iUVSecond = floor(uv0Second);
                float randOffset = rand(float2(when_ge(iUVSecond.x, 0), iUVSecond.y));
                float randOffsetNext = rand(
                    float2(when_ge(iUVSecond.x, 0), 
                    when_ge(iUVSecond.y + 1, inkSize) * -inkSize + when_lt(iUVSecond.y + 1, inkSize) * (iUVSecond.y + 1))
                );
                float bodyDistance = abs(uv0Main.x - 0.5);
                float sinBody = sin(uv0Main.y * 2 * PI) * 0.05;
                float offset = lerp(randOffset, randOffsetNext, smoothstep(0., 1., fUVSecond.y)) * 0.1;
                float maxDist = 0.3 + sinBody + offset;

                // For Wither Effect
                float witherSize = 3;
                float slowModif = 4;
                float uvThirdX = when_ge(i.uv.x, 0.5) * (frac((i.uv.x - 0.5) - _Time.y / slowModif) + when_lt(frac((i.uv.x - 0.5) - _Time.y / slowModif), 0.5) * 0.5) + 
                        when_lt(i.uv.x, 0.5) * frac(frac((i.uv.x + 0.5) + _Time.y / slowModif) - 0.5);
                float2 uv0Third = ((float2(uvThirdX, i.uv.y) - 0.5) * 2) * witherSize;
                float2 fUVThird = frac(uv0Third);
                float2 iUVThird = floor(uv0Third);
                float randWither = rand(iUVThird) > 0.85;
                float sizeRand = rand(iUVThird + _Time.z);
                float distFromMid = distance(i.uv.x, 0.5);
                float wither = randWither * when_lt(distance(fUVThird, float2(0.5, 0.5)), 0.05 - sizeRand * 0.05) * (1.0 - pow(distFromMid / 0.5, 5));
                
                // Make osmosis like effect
                float mainBody = when_le(bodyDistance, maxDist);
                float rot = (atan2(fUVThird.y - 0.5, fUVThird.x - 0.5) / UNITY_TWO_PI + 0.5);
                float rotValue = rot * 21;
                float rotV = frac(rotValue);
                float rotI = floor(rotValue);
                float nextRotI = (rotI + 1) % 21;
                float osmosisOffset = lerp(rand(float3(rotI, iUVThird.x, iUVThird.y)), rand(float3(nextRotI, iUVThird.x, iUVThird.y)), smoothstep(0., 1., rotV)) * 0.2 - 0.1;
                osmosisOffset += frac((sin(rot * PI * 3 + _Time.y * 3) + 1.0) / 2) * 0.1;

                mainBody *= 
                    randWither * (saturate(1.0 - smoothstep(maxDist - 0.1, maxDist, distance(i.uv.x, 0.5)) + 
                                    (distance(fUVThird, float2(0.5, 0.5)) > 0.3 + osmosisOffset))) + 
                    (1.0 - randWither) * mainBody;
                mainBody = saturate(mainBody + wither);
               
                // Process Normal Map
                float4 col = float4(1., 1., 1., 1.);

                half3 normalMap = UnpackNormal(tex2D(_NormalMap, uv0Main));
                float3 reflection = reflect(-lightDir, normalMap);
                col.a *= mainBody;
                col.rgb = ((reflection.r +reflection.g + reflection.b) / 3) * smoothstep(maxDist - 0.21, maxDist, distance(i.uv.x, 0.5)) * 1.;

                return col;
            }
            ENDCG
        }
    }
}
