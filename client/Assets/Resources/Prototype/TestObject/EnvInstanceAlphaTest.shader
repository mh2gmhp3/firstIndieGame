// This shader fills the mesh shape with a color predefined in the code.
Shader "Protptype/EnvInstanceAlphaTest"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _Cutoff("Alpha Cutoff Threshold", Range(0.0, 1.0)) = 0.5
        _WindStrength("WindStrength", float) = 0.5
        _WindSpeed("WindSpeed", float) = 0.5
        _WindDirection("WindDirection", Vector) = (1, 0, 0, 0)
        _PixelsPerUnit("PixelsPerUnit", float) = 1
    }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "RenderQueue" = "AlphaTest" }

        Cull Off

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_IN_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_instancing

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Cutoff;
                float _WindStrength;
                float _WindSpeed;
                float4 _WindDirection;
                float _PixelsPerUnit;
            CBUFFER_END

            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);

                float bendMask = IN.uv.y;
                float snapSize = 1 / _PixelsPerUnit;
                float oscillation = sin(_Time.y * _WindSpeed) * 0.5 + 0.5;
                float smoothDisplacementValue = oscillation * _WindStrength * bendMask;
                float3 smoothDisplacement = smoothDisplacementValue * _WindDirection.xyz;
                IN.positionOS.xyz += smoothDisplacement;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                positionWS.xyz = floor(positionWS.xyz / snapSize) * snapSize;

                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                //OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = TransformWorldToShadowCoord(TransformObjectToWorld(IN.positionOS.xyz));
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                return OUT;
            }

            // The fragment shader definition.
            half4 frag(Varyings IN, float face : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                float3 normalWS = normalize(IN.normalWS);
                normalWS *= sign(face);
                Light mainLight = GetMainLight(IN.shadowCoord);
                real shadowAttenuation = mainLight.shadowAttenuation;
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                half3 lighting = diffuse;// * shadowAttenuation;
                half3 ambient = SampleSH(normalWS);

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                clip(color.a - _Cutoff);
                half3 finalColor = color.rgb * (lighting.r + ambient.r);
                return half4(finalColor.rgb, color.a);
            }
            ENDHLSL
        }
    }
}