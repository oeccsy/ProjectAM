Shader "Spline/QuadSplineShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color;
            float _Thickness;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 p1 : TEXCOORD0;
                float3 p2 : TEXCOORD1;
                float3 p3 : TEXCOORD2;
                float3 p4 : TEXCOORD3;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 p1 : TEXCOORD0;
                float3 p2 : TEXCOORD1;
                float3 p3 : TEXCOORD2;
                float3 p4 : TEXCOORD3;
                float3 localPos : TEXCOORD4;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.p1 = v.p1;
                o.p2 = v.p2;
                o.p3 = v.p3;
                o.p4 = v.p4;
                o.localPos = v.vertex.xyz;

                return o;
            }

            float3 bezierPoint(float3 p1, float3 p2, float3 p3, float3 p4, float t)
            {
                float3 m1 = lerp(p1, p2, t);
                float3 m2 = lerp(p2, p3, t);
                float3 m3 = lerp(p3, p4, t);

                float3 m4 = lerp(m1, m2, t);
                float3 m5 = lerp(m2, m3, t);

                float3 m6 = lerp(m4, m5, t);

                return m6;
            }

            float segmentSDF(float3 position, float3 start, float3 end)
            {
                float3 sp = position - start;
                float3 se = end - start;

                float t = dot(sp, se) / dot(se, se);
                t = clamp(t, 0.0, 1.0);

                float3 proj = start + (t * se);
                float dist = length(proj - position);

                return dist;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float minDist = 1e6;
                float3 tempStart = i.p1;

                for(float t = 0.02; t < 1.0; t += 0.02)
                {
                    float3 tempEnd = bezierPoint(i.p1, i.p2, i.p3, i.p4, t);
                    float tempDist = segmentSDF(i.localPos, tempStart, tempEnd);

                    minDist = min(minDist, tempDist);
                    tempStart = tempEnd;
                }

                if(minDist > _Thickness) discard;

                return _Color;
            }
            ENDCG
        }
    }
}
