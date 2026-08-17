// 지면에 빛이 비친 것처럼 보이게 하는 데칼.
// 메시의 UV가 아니라 화면 깊이에서 복원한 실제 지면 좌표로 세기를 계산한다.
// 따라서 지면이 없는 디오라마 바깥에는 아무것도 그려지지 않으며, 절벽이나 경사에도 투영된다
Shader "ProjectAM/LightDecalShader"
{
    Properties
    {
        _DecalColor("Decal Color", Color) = (1.0, 0.72, 0.36, 1)
        _DecalIntensity("Decal Intensity", Range(0, 8)) = 1.6

        [PerRendererData] _GlowStartAmount("Glow Start Amount", Float) = 0
        [PerRendererData] _GlowTargetAmount("Glow Target Amount", Float) = 0
        [PerRendererData] _GlowStartTime("Glow Start Time", Float) = 0
        _GlowDuration("Glow Duration", Range(0, 10)) = 1.5
        
        _DecalFalloff("Decal Falloff", Range(1, 8)) = 2.5
        _DecalHeightRange("Decal Height Range", Range(0.1, 5)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "LightDecal"
            Tags { "LightMode" = "UniversalForward" }

            // 빛은 더해지는 것이므로 가산 합성
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DecalColor;
                half _DecalIntensity;
                float _GlowStartAmount;
                float _GlowTargetAmount;
                float _GlowStartTime;
                float _GlowDuration;
                half _DecalFalloff;
                half _DecalHeightRange;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // 깊이 버퍼로 현재 표면의 월드/로컬 좌표 복원
                float rawDepth = SampleSceneDepth(screenUV);
                float3 groundWS = ComputeWorldSpacePosition(screenUV, rawDepth, UNITY_MATRIX_I_VP);
                float3 groundOS = TransformWorldToObject(groundWS);

                // 중심에서 멀어질수록 감쇠한다
                half dist = saturate(length(groundOS.xy) * 2.0);
                half falloff = pow(1.0 - dist, _DecalFalloff);

                // 높이에 따라 멀어질수록 감쇠시킨다.
                half heightFade = saturate(1.0 - abs(groundOS.z) / _DecalHeightRange);
                
                // 연속적인 변화를 위해 시작 밝기로부터 경과한 시간을 계산
                float actualDuration = _GlowDuration * abs(_GlowTargetAmount - _GlowStartAmount);
                float elapsed = _Time.y - _GlowStartTime;
                float glowProgress = actualDuration > 0.0 ? saturate(elapsed / actualDuration) : 1.0;
                half glowAmount = lerp(_GlowStartAmount, _GlowTargetAmount, glowProgress);

                // 가산 합성은 알파에도 적용되므로 0을 반환해 알파 버퍼를 건드리지 않는다
                return half4(_DecalColor.rgb * _DecalIntensity * glowAmount * falloff * heightFade, 0.0);
            }
            ENDHLSL
        }
    }
}
