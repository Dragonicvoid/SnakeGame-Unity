Shader "Debug/AiRenderer"
{
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

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
                float drawLine: TANGENT;
                float4 color : COLOR;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float drawLine: TANGENT;
                float4 color : COLOR0;
                half2 uv : TEXCOORD0;
            };

            float when_lt(float x, float y) {
                return max(sign(y - x), 0.0);
            }

            float when_ge(float x, float y) {
                return 1.0 - when_lt(x, y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.drawLine = v.drawLine;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half dist = distance(half2(0.5,0.5), i.uv);
                i.color.a = (1.0 - step(0.45, dist)) * step(0.4, dist);
                fixed4 col = lerp(i.color, float4(i.color.xyz, 1.), when_ge(i.drawLine, 0.5));

                return col;
            }
            ENDCG
        }
    }
}
