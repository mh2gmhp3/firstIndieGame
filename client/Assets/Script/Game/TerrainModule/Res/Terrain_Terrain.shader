// This shader fills the mesh shape with a color predefined in the code.
Shader "Terrain/Terrain"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _TileMap("Tile Map", 2D) = "white"
        _Tiling("Tiling", Vector) = (4, 4, 0, 0)
    }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

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
            #pragma multi_compile_fog

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
                float3 normalOS : NORMAL;     // 物体空间法线
                float2 uv           : TEXCOORD0;
                float2 uv2          : TEXCOORD1;
                float2 uv3          : TEXCOORD2;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float2 uv           : TEXCOORD2;
                float2 uv2_Tiling   : TEXCOORD3;
                float2 uv3_Rotation : TEXCOORD4;

                float4 shadowCoord : TEXCOORD5;
                float fogCoord : TEXCOORD6;
            };

            TEXTURE2D(_TileMap);
            SAMPLER(sampler_TileMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _TileMap_ST;
                float2 _Tiling;
            CBUFFER_END

            float2 RotateUV(float2 uv, float2 uvCenter, float progress)
            {
                // to radian
                float rotation = progress * (2.0f * PI);

                // set to center
                float2 rotatedUV = uv - uvCenter;

                // rotate
                float s = sin(rotation);
                float c = cos(rotation);
                float x = rotatedUV.x * c - rotatedUV.y * s;
                float y = rotatedUV.x * s + rotatedUV.y * c;
                rotatedUV = float2(x, y);

                // revert to uv
                return rotatedUV + uvCenter;
            }

            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv; //TRANSFORM_TEX(IN.uv, _TileMap);
                OUT.uv2_Tiling = IN.uv2;
                OUT.uv3_Rotation = IN.uv3;

                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);
                OUT.fogCoord = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            // The fragment shader definition.
            half4 frag(Varyings IN) : SV_Target
            {
                float2 uvTiling = 1 / _Tiling;
                float2 uv = frac(IN.uv) * uvTiling + IN.uv2_Tiling;
                uv = RotateUV(uv, IN.uv2_Tiling + uvTiling / 2, IN.uv3_Rotation.x);
                half4 color = SAMPLE_TEXTURE2D(_TileMap, sampler_TileMap, uv);

                float3 normalWS = normalize(IN.normalWS);

                // 1. 获取主光源信息（使用阴影坐标作为参数）
                // GetMainLight 是 URP 推荐获取光照信息的函数
                Light mainLight = GetMainLight(IN.shadowCoord);
                // 阴影衰减值 (0.0 = 全阴影, 1.0 = 无阴影)
                real shadowAttenuation = mainLight.shadowAttenuation;
                // 2. 基础光照计算 (简单的 Lambert 模型)
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                // 3. 应用阴影衰减和环境光
                half3 lighting = diffuse;// * shadowAttenuation;
                half3 ambient = SampleSH(normalWS); // 环境光 / 间接光

                half3 finalColor = color.rgb * (lighting.r + ambient.r);
                finalColor = MixFog(finalColor, IN.fogCoord);
                //half3 finalColor = color.rgb * lighting.r;
                return half4(finalColor, color.a);
            }
            ENDHLSL
        }

        // ----------------------------------------------------
        // 2. 投射阴影 Pass (ShadowCaster)
        // ----------------------------------------------------
        // 此 Pass 依赖 URP 内部宏，通常不需要修改，是通用的。
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // 引用 URP 内部用于投射阴影的实现
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }
}