Shader "Transparent/BackgroundBlock"
{
    Properties 
    {
        _Color ("Block Color", Color) = (1., 1., 1., 1.)
    }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float depth01 : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _CameraPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float farPlane = _CameraPos.w;
                o.depth01 = distance(o.vertex, _CameraPos) / farPlane;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = _Color;
                color.a = max(1.0 - i.depth01, 0.15);
                return color;
            }
            ENDCG
        }
    }
}
