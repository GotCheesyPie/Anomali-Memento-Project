// Simple Box Blur Shader for URP
Shader "MyShaders/ScreenBlurShader"
{
    Properties
    {
        _BlurSize ("Blur Size", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off
        Cull Off

        Pass
        {
            Name "BlurPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = 0;
                float2 offsets[9] = {
                    float2(-1, 1), float2(0, 1), float2(1, 1),
                    float2(-1, 0), float2(0, 0), float2(1, 0),
                    float2(-1,-1), float2(0,-1), float2(1,-1)
                };

                for(int i = 0; i < 9; i++)
                {
                    col += tex2D(_MainTex, IN.uv + offsets[i] * _MainTex_TexelSize.xy * _BlurSize);
                }
                return col / 9;
            }
            ENDHLSL
        }
    }
}