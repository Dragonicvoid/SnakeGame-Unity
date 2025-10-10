Shader "Transparent/TrailVfx"
{
    Properties
    {
        _MainTex ("Current Snake Draw", 2D) = "white" {}
        _PrevTex ("Last Snake Draw", 2D) = "white" {}
        _TrailCol ("Color of the trail", Color) = (1.,1.,1.,1.)
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _PrevTex;
            float4 _TrailCol;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 mainTex = tex2D(_MainTex, i.uv);
                float4 secondTex = tex2D(_PrevTex, i.uv);
                fixed4 col = float4(_TrailCol.r, _TrailCol.g, _TrailCol.b, max(mainTex.a, secondTex.a));
                clip(col.a - 0.3);
                return col;
            }
            ENDCG
        }
    }
}
