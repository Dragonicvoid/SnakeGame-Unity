Shader "Transparent/Background"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmptyColor ("Default Color", Color) = (1., 1., 1., 1.)
        _FillColor ("Background Color when player have Energy, it is filled with _Dist property", Color) = (1., 1., 1., 1.)
        _Dist ("Percentage of Fill color filling up the background", float) = 0
        _EatRatio ("Visual Effect Because main player Eating", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"  }
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _EmptyColor;
            float4 _FillColor;
            float _Dist;
            float _EatRatio;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float gradOffset = 0.05;
                float newRange = 1.0 + gradOffset * 2.;
                float xDist = _Dist * newRange - gradOffset;

                float4 screen = float4(0., 0., 0.25, 1.);
                float4 bgCol = lerp(_FillColor, _EmptyColor, smoothstep(xDist, xDist + gradOffset, i.uv.x));
                bgCol = lerp(bgCol, bgCol * (1.0 - screen) + screen, _EatRatio);
                float4 block = tex2D(_MainTex, i.uv);
                block = lerp(bgCol, block, block.a);

                return block;
            }
            ENDCG
        }
    }
}
