Shader "Transparent/SnakeRender"
{
    Properties
    {
        _MainTex ("Main Body Texture", 2D) = "white" {}
        _SecondTex ("Side Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Less

        Pass
        {
            Name "BodyCreation"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                uint body_count : TANGENT;
                half2 uv : TEXCOORD0;
                float2 center : TEXCOORD1;
                float3 next_pos_norm : TEXCOORD2;
                float3 prev_pos_norm : TEXCOORD3;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                uint body_count : TANGENT;
                float2 uv : TEXCOORD0;
                float2 center : TEXCOORD1;
                float2 next_pos_norm : TEXCOORD2;
                float2 prev_pos_norm : TEXCOORD3;
                float radius : PSIZE;
            };

            struct sdfData {
                float h;
                float hClamp;
                float d;
                float dClamp;
                float2 proj;
                float2 projClamp;
            };

            struct texdata
            {
                float2 uv;
                float2 actUv;
                sdfData nextData;
                sdfData prevData;
                float2 prevNorm;
                float2 nextNorm;
                uint body_count;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.radius = v.vertex.z;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.next_pos_norm = v.next_pos_norm;
                o.prev_pos_norm = v.prev_pos_norm;
                o.body_count = v.body_count;
                o.center = v.center;
                o.uv = v.uv;
                return o;
            }

            float rand(float x) {
                return frac(sin(x * 2525.) * 24. );
            }

            float when_eq(float x, float y) {
                return 1.0 - abs(sign(x - y));
            }

            float when_lt(float x, float y) {
                return max(sign(y - x), 0.0);
            }

            float when_ge(float x, float y) {
                return 1.0 - when_lt(x, y);
            }

            float between(float min, float max, float x) {
                return (1.0 - step(max, x)) * step(min, x);
            }

            float or(float a, float b) {
                return min(a + b, 1.0);
            }

            float and(float a, float b) {
                return a * b;
            }

            float normalize(float x, float min, float max) {
                return (x - min) / (max - min);
            }

            float4 snake_body(texdata tex) {
                float hasPrev = min(ceil(distance(tex.prevNorm, float2(0., 0.))), 1.);
                float hasNext = min(ceil(distance(tex.nextNorm, float2(0., 0.))), 1.);

                float2 proj = hasPrev * (1.0 - hasNext) * tex.prevNorm + hasNext * (1.0 - hasPrev) * tex.nextNorm;
                float isLeft = when_lt(tex.actUv.x, 0.5);
                float dist = distance(tex.uv.x, 0.5);
                float distUnclamped = tex.actUv.x - 0.5;

                float mainTexClamp = 0.125;
                float secondTexClamp = 0.15;

                float tex1UvX = (1.0 - step(mainTexClamp, abs(dist))) * ((distUnclamped + mainTexClamp) / (2 * mainTexClamp));
                float tex2UvX = isLeft * ((distUnclamped + secondTexClamp) / (2 * secondTexClamp - mainTexClamp)) +
                                (1.0 - isLeft) * ((distUnclamped + mainTexClamp) / (2 * secondTexClamp - mainTexClamp) + 0.5);

                float4 mainColor = tex2D(_MainTex, float2(tex1UvX, tex.actUv.y));
                float4 edgeColor = tex2D(_SecondTex, float2(tex2UvX, tex.actUv.y));
                
                float4 o = lerp(mainColor, edgeColor, between(mainTexClamp, secondTexClamp, dist));
                o.a *= (1.0 - step(secondTexClamp, abs(dist)));
                o.a *= when_ge(tex.nextData.dClamp, tex.prevData.dClamp) + when_eq(tex.body_count, 1.);

                clip(((1.0 - step(secondTexClamp, abs(dist))) * (when_ge(tex.nextData.dClamp, tex.prevData.dClamp) + when_eq(tex.body_count, 1.))) - 0.1); 

                return o;
            }

            sdfData getSdfData(float2 u, float2 v) {
                float isNull = when_eq(v.x, 0.) * when_eq(v.y, 0.);
                float h = (1.0 - isNull) * (u.x * v.x + u.y * v.y) / (v.x * v.x + v.y * v.y);
                float hClamp = min(1. , max(0., h));
                float2 proj = v * h;
                float2 projClamp = v * hClamp;
                float d = distance(u, proj);
                float dClamp = distance(u, projClamp);

                sdfData res;
                res.h = h;
                res.hClamp = hClamp;
                res.d = d;
                res.dClamp = dClamp;
                res.proj = proj;
                res.projClamp = projClamp;

                return res;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv0 = (i.uv * 2.) - 1.;
                float4 o = float4(1., 1., 1., 1.);

                float2 u = uv0;
                float2 v = i.prev_pos_norm;
                sdfData prevData = getSdfData(u,v);

                v = i.next_pos_norm;
                sdfData nextData = getSdfData(u,v);

                float boundary = 0.5;
                float inBetween2Vec = and(when_ge(prevData.h, 0.), when_ge(nextData.h, 0.));
                float minDistClamp = min(prevData.dClamp, nextData.dClamp);
                float minDist = when_ge(prevData.h, 0.) * when_ge(prevData.h, nextData.h) * prevData.d 
                        + when_ge(nextData.h, 0.) * when_lt(prevData.h, nextData.h) * nextData.d;
                minDist = inBetween2Vec * min(prevData.d, nextData.d) +
                        when_ge(prevData.h, 0.) * (1.0 - inBetween2Vec) * prevData.d +
                        when_ge(nextData.h, 0.) * (1.0 - inBetween2Vec) * nextData.d;

                float2 vectorUse = when_lt(prevData.dClamp, nextData.dClamp) * prevData.projClamp + when_ge(prevData.dClamp, nextData.dClamp) * nextData.projClamp;
                float isLeft = when_lt(cross(float3(uv0.x, uv0.y, 0.), float3(vectorUse.x, vectorUse.y, 0.)).z, 0.);
                float textureDistX = 1. - (boundary - minDistClamp);
                textureDistX = isLeft * (max(boundary - minDistClamp * boundary, 0.)) + (1.0 - isLeft) * min((minDistClamp * boundary) + boundary, 1.) ;

                vectorUse = when_lt(prevData.dClamp, nextData.dClamp) * prevData.proj + when_ge(prevData.dClamp, nextData.dClamp) * nextData.proj;
                isLeft = when_lt(cross(float3(uv0.x, uv0.y, 0.), float3(vectorUse.x, vectorUse.y, 0.)).z, 0.);
                float actTextureDistX = minDist;
                actTextureDistX = isLeft * (boundary - minDist * boundary) + (1.0 - isLeft) * min((minDist * boundary) + boundary, 1.);

                // Edge Case the tail Part
                float lastBackPart = when_eq(i.body_count, 1.) * when_ge(nextData.h, 0.);
                actTextureDistX = lastBackPart * (1.0 - actTextureDistX) + (1.0 - lastBackPart)* actTextureDistX;

                // Edge case outside both Hvalue;
                float outsiteHRange = when_eq(prevData.hClamp, 0.) * when_eq(nextData.hClamp, 0.);
                float outsideIsLeft = when_lt(cross(float3(i.next_pos_norm.x, i.next_pos_norm.y, 0.), float3(i.prev_pos_norm.x, i.prev_pos_norm.y, 0.)).z, 0.);
                float centerDist = distance(float2(0., 0.), uv0);
                float outsideX = outsideIsLeft * (1.0 - (boundary - centerDist * boundary)) + (1.0 - outsideIsLeft) * (boundary - centerDist * boundary);
                actTextureDistX = outsiteHRange * outsideX + (1.0 - outsiteHRange) * actTextureDistX;
    
                float textureDistY = frac(prevData.hClamp);
                float actY = prevData.h;

                texdata tex;
                tex.uv = float2(textureDistX, textureDistY);
                tex.actUv = float2(actTextureDistX, actY);
                tex.prevData = prevData;
                tex.nextData = nextData;
                tex.prevNorm = i.prev_pos_norm;
                tex.nextNorm = i.next_pos_norm;
                tex.body_count = i.body_count;

                o *= snake_body(tex);

                float4 col = o;
                return col;
            }
            ENDCG
        }
    }
}
