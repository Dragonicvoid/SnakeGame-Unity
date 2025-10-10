Shader "Custom/Test3DShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Texture For Normal Mapping", 2D) = "white" {}
        _WorldPos("Object Position", Vector) = (0., 0., 0.)
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
          #include "Assets/Script/Shader/Volume.cginc"
          #include "Assets/Script/Shader/Snake/SnakeLib.cginc"

          #define PI 3.14159265358979323846

          struct appdata
          {
              float4 vertex : POSITION;
              float2 uv : TEXCOORD0;
          };

          struct v2f
          {
              float4 vertex : SV_POSITION;
              float2 uv : TEXCOORD0;
              float3 world : TEXCOORD1;
              float3 local : TEXCOORD2;
          };

          sampler3D _MainTex;
          float4 _MainTex_ST;
          float4 _Color;

          float3 _WorldPos;

          float sample_volume(float3 uv, float3 p)
          {
            // Assuming it is RenderTexture 3D using Red Color
            // Like Texture3D from mesh to SDF
            float v = 1.0 - tex3D(_MainTex, uv).r;
            return v;
          }

          v2f vert (appdata v)
          {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;

            o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.local = v.vertex.xyz;
            return o;
          }

          fixed4 frag (v2f i) : SV_Target
          {
            Ray ray;
            ray.origin = i.local;
            float3 dir = (i.world - _WorldSpaceCameraPos);
            ray.dir = normalize(mul(unity_WorldToObject, dir));

            AABB aabb;
            aabb.min = float3(-0.5, -0.5, -0.5);
            aabb.max = float3(0.5, 0.5, 0.5);

            float tnear;
            float tfar;
            intersect(ray, aabb, tnear, tfar);

            tnear = max(0.0, tnear);

            float3 start = ray.origin;
            float3 end = ray.origin + ray.dir * tfar;
            float dist = abs(tfar - tnear);
            float step_size = dist / float(ITERATIONS);
            float3 ds = normalize(end - start) * step_size;

            float4 dst = float4(0, 0, 0, 0);
            float3 p = start;

            float offset = 0;
            float distToCenterSphere = 0;
            float3 uv = get_uv(p);
            [unroll]
            for (int iter = 0; iter < ITERATIONS; iter++)
            {
              uv = get_uv(p);
              SphericalCoord sCoord = GetSphereCoord(uv);
              float size = 10;
              float distToCamera = distance(mul(unity_ObjectToWorld, ((uv - 0.5) * 2.)), _WorldSpaceCameraPos); 
              float2 i = floor(float2((sCoord.omega / PI) * size, (sCoord.tetha / (2 * PI)) * size));
              offset = sin(rand(float2(i.x, i.y)));
              offset = offset / 2 + 0.5;
              offset *= 0.2;
              offset += distToCamera / 700;
              distToCenterSphere = distance(uv, float3(0.0,0.0,0.0));
              float src = (distToCenterSphere < (0.025 + offset)) * (distToCenterSphere >= (offset));

              // blend
              dst.a = src;
              p += ds;

              if (dst.a > 0.95) {
                break;
              };
            }

            return float4((offset / 0.025),(offset / 0.025),(offset / 0.025),dst.a);
          }
            ENDCG
        }
    }
}
