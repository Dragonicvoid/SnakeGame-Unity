Shader "Transparent/VortexShader"
{
    Properties
    {
        _VortexSize("How many circle are formed from center to outside", Int) = 5
        _VortexWidth("How thick is the vortex", float) = 0.01
        _Show("Size Percentage", Range(0., 1.)) = 1.

        _MainColor ("Vortex Main Color", Color) = (1,1,1,1)
        _SecondColor ("Vortex Secondary Color", Color) = (1,1,1,1)
        _ScreenColor ("Vortex Border Lighten Color", Color) = (0.5, 0.1,0.,1)
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

            int _VortexSize;
            float _VortexWidth;
            float _Show;
            float4 _MainColor;
            float4 _SecondColor;
            float4 _ScreenColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
                float4 vertex : SV_POSITION;
            };

            struct polarData
            {
                float alpha;
                float distance;
            };

            float between(float min, float max, float x) {
                return (1.0 - step(max, x)) * step(min, x);
            }

            // Must use -N to N uv coord
            polarData getPolarData(float2 uv) {
                polarData res;
                
                res.alpha = (atan2(uv.y, uv.x) + 2.0 * PI) % (2.0 * PI);
                res.distance = distance(float2(0, 0), uv) + (1.0 - _Show) * (float)_VortexSize;

                return res;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = floor(_Time.z / 0.15) * 0.15;
                float4 screen = _ScreenColor;
                float4 secondScreenVal = _SecondColor * (1.0 - screen) + screen;
                float shouldScreen = (sin(time) + 1.) / 2.;

                float uvDist = distance(i.uv, float2(0.5, 0.5));
                float2 uv0 = (i.uv - 0.5) * 2;
                uv0 *= _VortexSize;

                float borderMinDist = 0.45;
                float borderMaxDist = 0.5;

                polarData polar = getPolarData(uv0); 

                float alphaNorm = polar.alpha / (2 * PI);
                alphaNorm = frac(alphaNorm + time);
                float fDist = frac(polar.distance);
                float iDist = floor(polar.distance);
                float vortex = 
                    min(
                        between(iDist + alphaNorm - _VortexWidth, iDist + alphaNorm + _VortexWidth, polar.distance) + 
                        between(iDist + 1.0 + alphaNorm - _VortexWidth, iDist + 1.0 + alphaNorm + _VortexWidth, polar.distance) +
                        between(iDist - 1.0 + alphaNorm - _VortexWidth, iDist - 1.0 + alphaNorm + _VortexWidth, polar.distance) 
                        , 1.0);
                vortex *= 1.0 - uvDist / 0.5;

                float border = between(borderMinDist, borderMaxDist, uvDist) * (1.0 - smoothstep(borderMinDist, borderMaxDist, uvDist));
                float4 col = lerp(_MainColor, lerp(_SecondColor, secondScreenVal, border * shouldScreen), min(vortex + border, 1.0));

                col.a *= polar.distance <= (float)_VortexSize;

                return col;
            }
            ENDCG
        }
    }
}
