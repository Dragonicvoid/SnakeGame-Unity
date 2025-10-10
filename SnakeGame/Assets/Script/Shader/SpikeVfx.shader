Shader "Custom/SpikeVfx"
{
    Properties
    {
        _MaxDistance("Distance max to spike before it expands", float) = 20.
        _SpikeHeight("Spike Height", float) = 5.
        _Show("Range 0 - 1 Where 0 is not drawn and 1 is drawn, filling from left to right", Range(0., 1.)) = 1.0

        _NormalColor("Color when it is idle", Color) = (1.0, 1.0, 1.0, 1.0)
        _ActivateColor("Color when player are near", Color) = (1.0, 1.0, 1.0, 1.0)

        _CamPos("Pos of cam for depth coloring", Vector) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        LOD 100

        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Global Var
            float3 _CamPos;
            float4 _PlayerPos[2];

            // Property
            float _MaxDistance;
            float _SpikeHeight;
            float _Show;
            float4 _NormalColor;
            float4 _ActivateColor;

            struct appdata
            {
                float4 vertex : POSITION;   
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 vertexWS : POSITION1;
                half2 uv : TEXCOORD0;
                float depth : DEPTH;
            };

            float when_lt(float x, float y) {
                return max(sign(y - x), 0.0);
            }

            float when_ge(float x, float y) {
                return 1.0 - when_lt(x, y);
            }

            float getClosestPlayerDist(float3 currPos) {
                float closest = _MaxDistance;

                [unroll]
                for (int i = 0; i < 2; i++) {
                    float dist = distance(_PlayerPos[i], currPos);
                    closest = when_lt(dist, closest) * dist + when_ge(dist, closest) * closest;
                }

                return closest;
            }
           
            v2f vert (appdata v)
            {
                v2f o;               
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertexWS = v.vertex;
                o.uv = v.uv;
                return o;
            }
           
            float4 frag (v2f i) : SV_Target
            {
                float heightDist = max(0., 1.0 - (getClosestPlayerDist(i.vertexWS) / _MaxDistance));

                float4 viewPos = UnityObjectToClipPos(i.vertexWS);
                float4 clipPos = mul(UNITY_MATRIX_VP, i.vertexWS);
                float2 screenUV = clipPos.xy / clipPos.w;
                screenUV = screenUV * 0.5 + 0.5;

                float show = min(-screenUV.y + _Show * 2, 1.0);
                float4 color = lerp(_NormalColor, _ActivateColor, heightDist);
                color.a *= show;

                return color;
            }
            ENDCG
        }
    }
}