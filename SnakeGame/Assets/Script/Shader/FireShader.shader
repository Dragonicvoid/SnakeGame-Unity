Shader "Transparent/Fire"
{
    Properties
    {
        _Color ("Color", Color) = (1.,1.,1.,1.)
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

            #define PI 3.14159265359

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

            struct polarData
            {
                float alpha;
                float distance;
            };

            float4 _Color;

            // Must use -N to N uv coord
            polarData getPolarData(float2 uv) {
                polarData res;
                
                res.alpha = (atan2(uv.y, uv.x) + 2.0 * PI) % (2.0 * PI);
                res.distance = distance(float2(0, 0), uv);

                return res;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float segment = 5.;
                float2 lightDir = float2(-1, -0.75);
                float2 uv0 = (i.uv - 0.5) * 2;
                polarData polar = getPolarData(uv0);

                float time = floor(_Time.z / 0.15);

                float alpha = (polar.alpha + time) % (2 * PI);
                float radius = PI * 2. / segment;

                float triangl = cos(floor(.3+alpha/radius)*radius-alpha)*length(uv0);
                triangl = 1.0 - step(.5, triangl);

                float normalAlpha = radius * floor((alpha + radius / 2) / radius);
                float2 normal = float2(polar.distance * cos((normalAlpha - time) % (2 * PI)), polar.distance * sin((normalAlpha - time) % (2 * PI)));

                float2 inverseLight = lightDir * -1;
                float reflectAngle = acos(
                    (normal.x * inverseLight.x + normal.y * inverseLight.y) / (sqrt(normal.x * normal.x + normal.y * normal.y) * sqrt(inverseLight.x * inverseLight.x + inverseLight.y * inverseLight.y)));
                reflectAngle = PI - reflectAngle;
                reflectAngle /= PI;

                float4 col = _Color;
                col.rgb *= float3(reflectAngle,reflectAngle,reflectAngle);
                col.a *= triangl;
                clip(col.a - 0.1);
                return col;
            }
            ENDCG
        }
    }
}
