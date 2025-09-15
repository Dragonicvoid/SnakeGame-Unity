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
                float4 color : COLOR;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
                half2 uv : TEXCOORD0;
            };

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

            fixed4 frag (v2f i) : SV_Target
            {
                half dist = distance(half2(0.5,0.5), i.uv);
                i.color.a = (1.0 - step(0.45, dist)) * step(0.4, dist);
                fixed4 col = i.color;

                return col;
            }
            ENDCG
        }
    }
}
