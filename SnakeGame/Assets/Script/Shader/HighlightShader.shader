Shader "Transparent/HighlightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurTex ("BlurTexture", 2D) = "white" {}
        _OverlayColor ("Color if its not main or blur", Color) = (0.,0.,0.,0.95)
        _BlurColor ("Color for highlight", Color) = (1.,0.,0.,1.)
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

            sampler2D _BlurTex;

            float4 _OverlayColor;
            float4 _BlurColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float4 main = tex2D(_MainTex, i.uv);
                float4 blur = tex2D(_BlurTex, i.uv);
                blur.rgb = _BlurColor.rgb;
                float4 highlight = lerp(lerp(_OverlayColor, blur, sqrt(blur.a)), main, step(0.2, main.a));

                return highlight;
            }
            ENDCG
        }
    }
}
