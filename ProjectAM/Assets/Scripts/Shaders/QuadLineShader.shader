Shader "Line/QuadLineShader"
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
                float3 start : TEXCOORD0;
                float3 end : TEXCOORD1;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 start : TEXCOORD0;
                float3 end : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.start = v.start;
                o.end = v.end;
                o.localPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 sp = i.localPos - i.start;
                float3 se = i.end - i.start;

                float t = dot(sp, se) / dot(se, se);
                t = clamp(t, 0.0, 1.0);

                float3 proj = i.start + (t * se);
                float dist = length(proj - i.localPos);

                if(dist > _Thickness) discard;

                return _Color;
            }
            ENDCG
        }
    }
}
