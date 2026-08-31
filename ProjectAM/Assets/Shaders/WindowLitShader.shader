Shader "ProjectAM/WindowLitShader"
{
    Properties
    {
        [PerRendererData] _BaseColor("Base Color", Color) = (0.12, 0.13, 0.18, 1)
        _EmissionColor("Emission Color", Color) = (1.0, 0.72, 0.36, 1)
        _EmissionIntensity("Emission Intensity", Range(0, 8)) = 3
        
        [PerRendererData] _GlowStartAmount("Glow Start Amount", Float) = 0
        [PerRendererData] _GlowTargetAmount("Glow Target Amount", Float) = 0
        [PerRendererData] _GlowStartTime("Glow Start Time", Float) = 0
        _GlowDuration("Glow Duration", Range(0, 10)) = 3
        
        _FlickerAmount("Flicker Amount", Range(0, 0.5)) = 0.06
        _FlickerSpeed("Flicker Speed", Range(0, 20)) = 6
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EmissionColor;
                half _EmissionIntensity;
                float _GlowStartAmount;
                float _GlowTargetAmount;
                float _GlowStartTime;
                float _GlowDuration;
                half _FlickerAmount;
                half _FlickerSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);

                // 꺼져 있을 때의 유리는 평범한 표면이므로 주광원 + 앰비언트만 받는다
                Light mainLight = GetMainLight();
                half diffuse = saturate(dot(normalWS, mainLight.direction));
                half3 lighting = mainLight.color * diffuse + SampleSH(normalWS);

                // 촛불처럼 미세하게 흔들리게 한다. 주기가 다른 sin 세가지를 합성해 반복이 눈에 띄지 않게 한다.
                float phase = _Time.y * _FlickerSpeed;
                float noise = sin(phase) * 0.5 + sin(phase * 1.7 + 1.3) * 0.3 + sin(phase * 2.9 + 2.7) * 0.2;
                half flicker = 1.0 + noise * _FlickerAmount;

                // 연속적인 변화를 위해 시작 밝기로부터 경과한 시간을 계산
                float actualDuration = _GlowDuration * abs(_GlowTargetAmount - _GlowStartAmount);
                float elapsed = _Time.y - _GlowStartTime;
                float glowProgress = actualDuration > 0.0 ? saturate(elapsed / actualDuration) : 1.0;
                half glowAmount = lerp(_GlowStartAmount, _GlowTargetAmount, glowProgress);

                // 발광에는 조명을 곱하지 않는다. 빛을 받지 않아도 스스로 내는 색이다
                half3 baseColor = _BaseColor.rgb * lighting;
                half3 emission = _EmissionColor.rgb * _EmissionIntensity * glowAmount * flicker;

                return half4(baseColor + emission, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
