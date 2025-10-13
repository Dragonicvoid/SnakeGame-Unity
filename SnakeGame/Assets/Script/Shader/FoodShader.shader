Shader "Transparent/FoodShader"
{
    Properties 
    {
        _Height("Height", float) = 0.2
        _Width("Width", float) = 0.6
        _Fade("Fading Effect on the edge", float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define PI 3.14159265359

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            struct polarData
            {
                float alpha;
                float distance;
            };

            // Must use -N to N uv coord
            polarData getPolarData(float2 uv) {
                polarData res;
                
                res.alpha = (atan2(uv.y, uv.x) + 2.0 * PI) % (2.0 * PI);
                res.distance = distance(float2(0, 0), uv);

                return res;
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float _Width;
            float _Height;
            float _Fade;

            fixed4 frag (v2f i) : SV_Target
            {
                float segment = 3.;
                float2 lightDir = float2(-1, -0.75);
                float time = _Time.y;
                float4 color = float4(1., 1., 1., 1.);
                float2 normUV = (i.uv - 0.5) * 2.0;

                polarData polar = getPolarData(normUV);
                float alpha = (polar.alpha + time) % (2 * PI);
                float radius = PI * 2. / segment;

                float2 rotatedUV = float2(polar.distance * cos(alpha), polar.distance * sin(alpha));

                float reduce = lerp(1.0 - _Height, _Height, 1.0 - i.uv.y);
                float2 dist = step(float2(_Width - reduce + _Fade, _Height + _Fade), float2(abs(rotatedUV.x), abs(rotatedUV.y)));
                color.a *= 1.0 - max(dist.x, dist.y);
                color *= i.color;

                float normalAlpha = radius * floor((alpha + radius / 2) / radius);
                float2 normal = float2(polar.distance * cos((normalAlpha - time) % (2 * PI)), polar.distance * sin((normalAlpha - time) % (2 * PI)));

                float2 inverseLight = lightDir * -1;
                float reflectAngle = acos(
                    (normal.x * inverseLight.x + normal.y * inverseLight.y) / (sqrt(normal.x * normal.x + normal.y * normal.y) * sqrt(inverseLight.x * inverseLight.x + inverseLight.y * inverseLight.y)));
                reflectAngle = PI - reflectAngle;
                reflectAngle /= PI;

                float4 col = color;
                col.rgb *= float3(reflectAngle,reflectAngle,reflectAngle);

                clip(col.a - 0.1);
                return col;
            }
            ENDCG
        }
    }
}
