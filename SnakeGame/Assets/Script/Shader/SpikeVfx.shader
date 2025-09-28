Shader "Custom/SpikeVfx"
{
    Properties
    {
        _Matcap("Matcap", 2D) = "white" {}
        _MaxDistance("Distance max to spike before it expands", float) = 20.
        _SpikeHeight("Spike Height", float) = 5.
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
            #pragma geometry geometry
            #pragma fragment frag

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma require geometry
           
            #include "UnityCG.cginc"

            // Global Var
            float3 _CamPos;
            float4 _PlayerPos[2];

            // Property
            float _MaxDistance;
            float _SpikeHeight;

            struct appdata
            {
                float4 vertex : POSITION;   
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex : POSITION;   
                float2 uv : TEXCOORD0;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float depth : DEPTH;
            };

            float when_lt(float x, float y) {
                return max(sign(y - x), 0.0);
            }

            float when_ge(float x, float y) {
                return 1.0 - when_lt(x, y);
            }

            // Credits: this geometry shader code is based on this video
            // https://www.youtube.com/watch?v=7C-mA08mp8o
            float3 GetNormalFromTriangle(float3 a, float3 b, float3 c) {
                return normalize(cross(b - a, c - a));
            }

            g2f SetupVertex(float3 positionWS, float3 normalWS, float2 uv) {
                // TODO: Might want to use lightning for this
                // so keep the normal calculated for now
                g2f output = (g2f)0;
                output.vertex = UnityObjectToClipPos(positionWS);
                output.uv = uv;
                output.depth = abs(_CamPos.z - positionWS.z) / 12.;
                return output;
            }

            void SetupAndOutputTriangle(inout TriangleStream<g2f> outputStream, v2g a, v2g b, v2g c) {
                outputStream.RestartStrip();
                float3 normalWS = GetNormalFromTriangle(a.vertex, b.vertex, c.vertex);

                // Should be clockwise
                outputStream.Append(SetupVertex(a.vertex, normalWS, a.uv));
                outputStream.Append(SetupVertex(c.vertex, normalWS, c.uv));
                outputStream.Append(SetupVertex(b.vertex, normalWS, b.uv));
            }

            float4 GetSquareCenterFromTriangle(float4 a, float4 b, float4 c) {
                return when_ge(distance(a,c), distance(a,b)) * ((a + c) / 2) + when_lt(distance(a,c), distance(a,b)) * ((a + b) / 2);
            }

            float2 GetSquareCenterFromTriangle(float2 a, float2 b, float2 c) {
                return when_ge(distance(a,c), distance(a,b)) * ((a + c) / 2) + when_lt(distance(a,c), distance(a,b)) * ((a + b) / 2);
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
           
            v2g vert (appdata v)
            {
                v2g o;               
                o.vertex = v.vertex;   
                o.uv = v.uv;
                return o;
            }

            [maxvertexcount(6)]
            void geometry (triangle v2g inputs[3], inout TriangleStream<g2f> outputStream)
            {
                v2g center = (v2g)0;
                // Assuming Spike is always a cube the triangles Will always 
                // have same normal within 1 face

                float3 triNormal = GetNormalFromTriangle(inputs[0].vertex, inputs[1].vertex, inputs[2].vertex);
                float3 centerPos = GetSquareCenterFromTriangle(inputs[0].vertex, inputs[1].vertex, inputs[2].vertex);
                float3 heightIncrease = triNormal * _SpikeHeight * max(0., 1.0 - (getClosestPlayerDist(centerPos) / _MaxDistance));

                center.vertex = float4(float3(centerPos + heightIncrease), 0.);
                center.uv = GetSquareCenterFromTriangle(inputs[0].vertex, inputs[1].vertex, inputs[2].vertex);

                SetupAndOutputTriangle(outputStream, inputs[0], inputs[1], center);
                SetupAndOutputTriangle(outputStream, inputs[1], inputs[2], center);
            }
           
            float4 frag (g2f i) : SV_Target
            {
                return float4(i.depth,i.depth,i.depth, 1.);
            }
            ENDCG
        }

        // Pass
        // {
        //     Blend One One

        //     ZTest Always
        //     ZWrite Off

        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag
           
        //     #include "UnityCG.cginc"

        //     struct appdata
        //     {
        //         float4 vertex : POSITION;   
        //         float4 uv : TEXCOORD0;
        //     };

        //     struct v2f
        //     {
        //         float4 vertex : SV_POSITION;
        //         float2 uv : TEXCOORD0;
        //         float4 screenPos : TEXCOORD1;
        //         float depth : DEPTH;
        //         float size : TEXCOORD2;
        //     };
           
        //     v2f vert (appdata v)
        //     {
        //         v2f o;               
        //         o.vertex = UnityObjectToClipPos(v.vertex);   
        //         o.screenPos = ComputeScreenPos(o.vertex);
        //         COMPUTE_EYEDEPTH(o.depth);
        //         o.uv = v.uv;
        //         o.size = v.uv.z;
        //         return o;
        //     }

        //     uniform sampler2D _ParticleDepthTexture;

        //     float4 frag (v2f i) : SV_Target
        //     {
        //         float2 toCenter = (i.uv - 0.5) * 2;
        //         float radius = i.size * 0.5;
        //         float z = sqrt(1.0 - toCenter.x * toCenter.x - toCenter.y * toCenter.y) * radius;

        //         float underlyingDepth = tex2D(_ParticleDepthTexture, i.screenPos.xy / i.screenPos.w).r;
        //         float decodedDepth = underlyingDepth / _ProjectionParams.w;
        //         float dz = saturate(decodedDepth - i.depth + z);

        //         toCenter *= dz;

        //         return float4(toCenter, dz, dz / z) * 100;
        //     }
        //     ENDCG
        // }

        // Pass
        // {
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag
           
        //     #include "UnityCG.cginc"

        //     struct appdata
        //     {
        //         float4 vertex : POSITION;   
        //         float2 uv : TEXCOORD0;
        //     };

        //     struct v2f
        //     {
        //         float4 vertex : SV_POSITION;
        //         float2 uv : TEXCOORD0;
        //         float4 screenPos : TEXCOORD1;
        //     };
           
        //     v2f vert (appdata v)
        //     {
        //         v2f o;               
        //         o.vertex = UnityObjectToClipPos(v.vertex);   
        //         o.uv = v.uv;
        //         o.screenPos = ComputeScreenPos(o.vertex);
        //         return o;
        //     }

        //     sampler2D _AdditiveTexture;
        //     sampler2D _Matcap;

        //     float4 frag (v2f i) : SV_Target
        //     {
        //         clip(length(i.uv - 0.5) > 0.5 ? -1 : 1);

        //         float4 merged = tex2D(_AdditiveTexture, i.screenPos.xy / i.screenPos.w);
        //         merged.xy /= merged.z;
        //         merged.z = sqrt(1.0 - merged.x * merged.x - merged.y * merged.y);

        //         float2 normal = merged.xy * 0.5 + 0.5;

        //         float4 mc = tex2D(_Matcap, normal);
               

        //         return mc;
        //     }
        //     ENDCG
        // }
    }
}