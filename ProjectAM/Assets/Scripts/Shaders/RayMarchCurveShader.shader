Shader "Custom/RayMarchCurveShader"
{
    Properties
    {
        _Color     ("Color", Color)     = (1, 0.5, 0, 1)
        _Thickness ("Thickness", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            fixed4 _Color;
            float  _Thickness;

            // 커브당 다른 제어점을 인스턴스 버퍼로 관리
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _P1)
                UNITY_DEFINE_INSTANCED_PROP(float4, _P2)
                UNITY_DEFINE_INSTANCED_PROP(float4, _P3)
                UNITY_DEFINE_INSTANCED_PROP(float4, _P4)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct fragOut
            {
                fixed4 color : SV_Target;
                float  depth : SV_Depth;
            };

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.pos      = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float3 bezierPoint(float3 p1, float3 p2, float3 p3, float3 p4, float t)
            {
                float3 a = lerp(p1, p2, t);
                float3 b = lerp(p2, p3, t);
                float3 c = lerp(p3, p4, t);
                return lerp(lerp(a, b, t), lerp(b, c, t), t);
            }

            float segmentSDF(float3 position, float3 start, float3 end)
            {
                float3 sp = position - start;
                float3 se = end - start;
                float  t  = saturate(dot(sp, se) / dot(se, se));
                return length(position - (start + se * t));
            }

            float curveSDF(float3 position, float3 p1, float3 p2, float3 p3, float3 p4)
            {
                float  minDist      = 1e6;
                float3 segmentStart = p1;

                [unroll]
                for (int i = 1; i <= 16; i++)
                {
                    float3 segmentEnd = bezierPoint(p1, p2, p3, p4, i * rcp(16.0));
                    minDist      = min(minDist, segmentSDF(position, segmentStart, segmentEnd));
                    segmentStart = segmentEnd;
                }

                return minDist;
            }

            bool intersectAABB(float3 ro, float3 rd, float3 boxMin, float3 boxMax, out float tNear, out float tFar)
            {
                float3 inv = rcp(rd);
                float3 tA  = (boxMin - ro) * inv;
                float3 tB  = (boxMax - ro) * inv;
                tNear = max(max(min(tA.x, tB.x), min(tA.y, tB.y)), min(tA.z, tB.z));
                tFar  = min(min(max(tA.x, tB.x), max(tA.y, tB.y)), max(tA.z, tB.z));
                return tFar >= max(tNear, 0.0);
            }

            fragOut frag(v2f i)
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 p1 = UNITY_ACCESS_INSTANCED_PROP(Props, _P1).xyz;
                float3 p2 = UNITY_ACCESS_INSTANCED_PROP(Props, _P2).xyz;
                float3 p3 = UNITY_ACCESS_INSTANCED_PROP(Props, _P3).xyz;
                float3 p4 = UNITY_ACCESS_INSTANCED_PROP(Props, _P4).xyz;

                // 단위 정육면체(0→1) 메시에 position/scale을 적용했으므로
                // 월드 변환 행렬에서 직접 박스 범위를 추출
                float3 boxMin = float3(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23);
                float3 boxMax = boxMin + float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m11, unity_ObjectToWorld._m22);

                float3 ro = _WorldSpaceCameraPos;
                float3 rd = normalize(i.worldPos - ro);

                float tNear, tFar;
                if (!intersectAABB(ro, rd, boxMin, boxMax, tNear, tFar)) discard;

                float  t   = max(tNear, 0.001);
                float3 p   = ro + rd * t;
                bool   hit = false;

                [loop]
                for (int step = 0; step < 128; step++)
                {
                    if (t > tFar) break;

                    float d = curveSDF(p, p1, p2, p3, p4);
                    if (d < _Thickness)
                    {
                        hit = true;
                        break;
                    }

                    t += max(d * 0.5, 0.005);
                    p += rd * max(d * 0.5, 0.005);
                }

                if (!hit) discard;

                float4 clip = mul(UNITY_MATRIX_VP, float4(p, 1.0));

                fragOut o;
                o.color = _Color;
                o.depth = clip.z / clip.w;
                return o;
            }
            ENDCG
        }
    }
}
