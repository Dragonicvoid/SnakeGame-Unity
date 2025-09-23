Shader "Transparent/Blur"
{
    Properties 
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Blur Offset", float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Less

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
            float4 _MainTex_TexelSize;
            float _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 getNeighborVal(float2 uv) {
                float4 total = float4(0., 0., 0., 0.);
                for(int i = -1; i <= 1; i++) {
                    for(int j = -1; j <= 1; j++) {
                        fixed4 col = tex2D(_MainTex, float2(uv.x + _MainTex_TexelSize.x * i * _Offset, uv.y + _MainTex_TexelSize.y * j * _Offset));
                        total += col;
                    }
                }

                float4 finalCol = pow(total / 9., float4(3., 3., 3., 3.));
                return pow(finalCol, float4(1./3.,1./3.,1./3.,1./3.));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = getNeighborVal(i.uv);
                return col;
            }
            ENDCG
        }
    }
}
