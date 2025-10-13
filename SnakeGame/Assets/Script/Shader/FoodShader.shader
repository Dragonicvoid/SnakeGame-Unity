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
                float4 color = float4(1., 1., 1., 1.);
                float2 normUV = (i.uv - 0.5) * 2.0;
                float reduce = lerp(1.0 - _Height, _Height, 1.0 - i.uv.y);
                float2 dist = step(float2(_Width - reduce + _Fade, _Height + _Fade), float2(abs(normUV.x), abs(normUV.y)));
                color.a *= 1.0 - max(dist.x, dist.y);
                color *= i.color;

                clip(color.a - 0.1);
            
                fixed4 o = color;
                return o;
            }
            ENDCG
        }
    }
}
