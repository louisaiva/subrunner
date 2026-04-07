Shader "Shader Graphs/colored_wired_object"
{
    Properties
    {
        [NoScaleOffset]_MainTex("_MainTex", 2D) = "white" {}
        _color_threshold("color threshold", Range(0, 2)) = 0.1
        _outline_color("outline color", Color) = (0, 0, 0, 1)
        [Toggle(_ENABLED)]_ENABLED("enabled", Float) = 0
        _outline_size("outline size", Range(0, 2)) = 0.2
        [HideInInspector]White("Color", Color) = (1, 1, 1, 1)
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalSpriteLitSubTarget"
        }
        Pass
        {
            Name "Sprite Lit"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        Stencil
        {
        ReadMask 127
        WriteMask 128
        Ref 128
        CompFront Always
        PassFront Replace
        CompBack Always
        PassBack Replace
        }
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
        #pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_vertex _ SKINNED_SPRITE
        #pragma shader_feature_local _ _ENABLED
        
        #if defined(_ENABLED)
            #define KEYWORD_PERMUTATION_0
        #else
            #define KEYWORD_PERMUTATION_1
        #endif
        
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define VARYINGS_NEED_SCREENPOSITION
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITELIT
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
             float4 screenPosition;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float4 screenPosition : INTERP2;
             float3 positionWS : INTERP3;
             float3 normalWS : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.screenPosition.xyzw = input.screenPosition;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.screenPosition = input.screenPosition.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float _color_threshold;
        float4 _outline_color;
        float _outline_size;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }
        
        void Unity_Length_float4(float4 In, out float Out)
        {
            Out = length(In);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        struct Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float
        {
        half4 uv0;
        };
        
        void SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(UnityTexture2D _base_texture, float _outline_thickness, float2 _main_pixel_displacement, float2 _target_pixel_displacement, Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float IN, out float4 OutVector4_1)
        {
        UnityTexture2D _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D = _base_texture;
        float2 _Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2 = _main_pixel_displacement;
        UnityTexture2D _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D = _base_texture;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Width_0_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.z;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Height_2_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.w;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.x;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.y;
        float2 _Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2 = float2(_TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float, _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float);
        float _Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float = _outline_thickness;
        float2 _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2, (_Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float.xx), _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2);
        float2 _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2, _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2);
        float4 _UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4 = IN.uv0;
        float2 _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2;
        Unity_Add_float2(_Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2, (_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2);
        float4 _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.tex, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.samplerstate, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.GetTransformedUV(_Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2) );
        if (_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_R_4_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.r;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_G_5_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.g;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_B_6_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.b;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.a;
        UnityTexture2D _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D = _base_texture;
        float2 _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2 = _target_pixel_displacement;
        float2 _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2, _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2);
        float2 _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2;
        Unity_Add_float2((_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2, _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2);
        float4 _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.tex, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.samplerstate, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.GetTransformedUV(_Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2) );
        if (_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_R_4_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.r;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_G_5_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.g;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_B_6_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.b;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.a;
        float _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float;
        Unity_Add_float(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float, _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float);
        float _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float;
        Unity_Saturate_float(_Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float, _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float);
        float4 _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4;
        Unity_Subtract_float4(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4);
        float _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float;
        Unity_Length_float4(_Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4, _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float);
        float _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float;
        Unity_Branch_float(((bool) _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float), _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float, float(0), _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float);
        OutVector4_1 = (_Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float.xxxx);
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Step_float4(float4 Edge, float4 In, out float4 Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float4 SpriteMask;
            float3 NormalTS;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float = _color_threshold;
            UnityTexture2D _Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_93b4916a36c248019358724eb00a44aa;
            _Compare2Colors_93b4916a36c248019358724eb00a44aa.uv0 = IN.uv0;
            float4 _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float, float2 (0, 1), float2 (0, -1), _Compare2Colors_93b4916a36c248019358724eb00a44aa, _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4);
            float _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_a71a0875648446779bbea5977759fb55;
            _Compare2Colors_a71a0875648446779bbea5977759fb55.uv0 = IN.uv0;
            float4 _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float, float2 (1, 0), float2 (-1, 0), _Compare2Colors_a71a0875648446779bbea5977759fb55, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4);
            float4 _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4;
            Unity_Add_float4(_Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4, _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4);
            float4 _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4;
            Unity_Saturate_float4(_Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4, _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4);
            float4 _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4;
            Unity_Step_float4((_Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float.xxxx), _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4, _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4);
            float4 _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4 = _outline_color;
            float4 _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4, _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4);
            float _Split_a7787229b3a740c283ae554775467752_R_1_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[0];
            float _Split_a7787229b3a740c283ae554775467752_G_2_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[1];
            float _Split_a7787229b3a740c283ae554775467752_B_3_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[2];
            float _Split_a7787229b3a740c283ae554775467752_A_4_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[3];
            float _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Split_a7787229b3a740c283ae554775467752_A_4_Float, float(0), _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean);
            UnityTexture2D _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float4 _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.tex, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.samplerstate, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            if (_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode.x > 0)
                _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode);
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_R_4_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.r;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_G_5_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.g;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_B_6_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.b;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.a;
            float _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean;
            Unity_Comparison_Greater_float(_SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float, float(0), _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean);
            float4 _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4 = float4(float(0), float(0), float(0), float(1));
            float4 _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean, _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4, float4(0, 0, 0, 0), _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4);
            float4 _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4, _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4, _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4);
            #if defined(_ENABLED)
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            #else
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4;
            #endif
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_R_1_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[0];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_G_2_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[1];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_B_3_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[2];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[3];
            surface.BaseColor = (_enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4.xyz);
            surface.SpriteMask = IsGammaSpace() ? float4(1, 1, 1, 1) : float4 (SRGBToLinear(float3(1, 1, 1)), 1);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/Mesh2DLitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Sprite Normal"
            Tags
            {
                "LightMode" = "NormalsRendering"
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        Stencil
        {
        ReadMask 127
        WriteMask 128
        Ref 128
        CompFront Always
        PassFront Replace
        CompBack Always
        PassBack Replace
        }
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_vertex _ SKINNED_SPRITE
        #pragma shader_feature_local _ _ENABLED
        
        #if defined(_ENABLED)
            #define KEYWORD_PERMUTATION_0
        #else
            #define KEYWORD_PERMUTATION_1
        #endif
        
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITENORMAL
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float4 texCoord0 : INTERP1;
             float4 color : INTERP2;
             float3 normalWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float _color_threshold;
        float4 _outline_color;
        float _outline_size;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }
        
        void Unity_Length_float4(float4 In, out float Out)
        {
            Out = length(In);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        struct Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float
        {
        half4 uv0;
        };
        
        void SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(UnityTexture2D _base_texture, float _outline_thickness, float2 _main_pixel_displacement, float2 _target_pixel_displacement, Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float IN, out float4 OutVector4_1)
        {
        UnityTexture2D _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D = _base_texture;
        float2 _Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2 = _main_pixel_displacement;
        UnityTexture2D _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D = _base_texture;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Width_0_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.z;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Height_2_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.w;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.x;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.y;
        float2 _Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2 = float2(_TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float, _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float);
        float _Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float = _outline_thickness;
        float2 _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2, (_Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float.xx), _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2);
        float2 _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2, _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2);
        float4 _UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4 = IN.uv0;
        float2 _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2;
        Unity_Add_float2(_Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2, (_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2);
        float4 _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.tex, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.samplerstate, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.GetTransformedUV(_Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2) );
        if (_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_R_4_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.r;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_G_5_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.g;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_B_6_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.b;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.a;
        UnityTexture2D _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D = _base_texture;
        float2 _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2 = _target_pixel_displacement;
        float2 _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2, _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2);
        float2 _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2;
        Unity_Add_float2((_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2, _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2);
        float4 _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.tex, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.samplerstate, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.GetTransformedUV(_Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2) );
        if (_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_R_4_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.r;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_G_5_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.g;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_B_6_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.b;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.a;
        float _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float;
        Unity_Add_float(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float, _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float);
        float _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float;
        Unity_Saturate_float(_Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float, _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float);
        float4 _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4;
        Unity_Subtract_float4(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4);
        float _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float;
        Unity_Length_float4(_Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4, _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float);
        float _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float;
        Unity_Branch_float(((bool) _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float), _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float, float(0), _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float);
        OutVector4_1 = (_Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float.xxxx);
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Step_float4(float4 Edge, float4 In, out float4 Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float = _color_threshold;
            UnityTexture2D _Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_93b4916a36c248019358724eb00a44aa;
            _Compare2Colors_93b4916a36c248019358724eb00a44aa.uv0 = IN.uv0;
            float4 _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float, float2 (0, 1), float2 (0, -1), _Compare2Colors_93b4916a36c248019358724eb00a44aa, _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4);
            float _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_a71a0875648446779bbea5977759fb55;
            _Compare2Colors_a71a0875648446779bbea5977759fb55.uv0 = IN.uv0;
            float4 _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float, float2 (1, 0), float2 (-1, 0), _Compare2Colors_a71a0875648446779bbea5977759fb55, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4);
            float4 _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4;
            Unity_Add_float4(_Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4, _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4);
            float4 _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4;
            Unity_Saturate_float4(_Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4, _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4);
            float4 _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4;
            Unity_Step_float4((_Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float.xxxx), _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4, _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4);
            float4 _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4 = _outline_color;
            float4 _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4, _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4);
            float _Split_a7787229b3a740c283ae554775467752_R_1_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[0];
            float _Split_a7787229b3a740c283ae554775467752_G_2_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[1];
            float _Split_a7787229b3a740c283ae554775467752_B_3_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[2];
            float _Split_a7787229b3a740c283ae554775467752_A_4_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[3];
            float _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Split_a7787229b3a740c283ae554775467752_A_4_Float, float(0), _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean);
            UnityTexture2D _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float4 _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.tex, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.samplerstate, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            if (_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode.x > 0)
                _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode);
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_R_4_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.r;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_G_5_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.g;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_B_6_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.b;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.a;
            float _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean;
            Unity_Comparison_Greater_float(_SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float, float(0), _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean);
            float4 _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4 = float4(float(0), float(0), float(0), float(1));
            float4 _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean, _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4, float4(0, 0, 0, 0), _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4);
            float4 _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4, _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4, _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4);
            #if defined(_ENABLED)
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            #else
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4;
            #endif
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_R_1_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[0];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_G_2_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[1];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_B_3_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[2];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[3];
            surface.BaseColor = (_enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/MeshNormalPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        #pragma shader_feature_local _ _ENABLED
        
        #if defined(_ENABLED)
            #define KEYWORD_PERMUTATION_0
        #else
            #define KEYWORD_PERMUTATION_1
        #endif
        
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float _color_threshold;
        float4 _outline_color;
        float _outline_size;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }
        
        void Unity_Length_float4(float4 In, out float Out)
        {
            Out = length(In);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        struct Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float
        {
        half4 uv0;
        };
        
        void SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(UnityTexture2D _base_texture, float _outline_thickness, float2 _main_pixel_displacement, float2 _target_pixel_displacement, Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float IN, out float4 OutVector4_1)
        {
        UnityTexture2D _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D = _base_texture;
        float2 _Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2 = _main_pixel_displacement;
        UnityTexture2D _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D = _base_texture;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Width_0_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.z;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Height_2_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.w;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.x;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.y;
        float2 _Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2 = float2(_TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float, _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float);
        float _Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float = _outline_thickness;
        float2 _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2, (_Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float.xx), _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2);
        float2 _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2, _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2);
        float4 _UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4 = IN.uv0;
        float2 _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2;
        Unity_Add_float2(_Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2, (_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2);
        float4 _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.tex, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.samplerstate, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.GetTransformedUV(_Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2) );
        if (_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_R_4_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.r;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_G_5_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.g;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_B_6_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.b;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.a;
        UnityTexture2D _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D = _base_texture;
        float2 _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2 = _target_pixel_displacement;
        float2 _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2, _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2);
        float2 _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2;
        Unity_Add_float2((_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2, _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2);
        float4 _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.tex, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.samplerstate, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.GetTransformedUV(_Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2) );
        if (_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_R_4_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.r;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_G_5_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.g;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_B_6_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.b;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.a;
        float _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float;
        Unity_Add_float(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float, _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float);
        float _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float;
        Unity_Saturate_float(_Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float, _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float);
        float4 _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4;
        Unity_Subtract_float4(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4);
        float _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float;
        Unity_Length_float4(_Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4, _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float);
        float _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float;
        Unity_Branch_float(((bool) _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float), _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float, float(0), _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float);
        OutVector4_1 = (_Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float.xxxx);
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Step_float4(float4 Edge, float4 In, out float4 Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float = _color_threshold;
            UnityTexture2D _Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_93b4916a36c248019358724eb00a44aa;
            _Compare2Colors_93b4916a36c248019358724eb00a44aa.uv0 = IN.uv0;
            float4 _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float, float2 (0, 1), float2 (0, -1), _Compare2Colors_93b4916a36c248019358724eb00a44aa, _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4);
            float _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_a71a0875648446779bbea5977759fb55;
            _Compare2Colors_a71a0875648446779bbea5977759fb55.uv0 = IN.uv0;
            float4 _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float, float2 (1, 0), float2 (-1, 0), _Compare2Colors_a71a0875648446779bbea5977759fb55, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4);
            float4 _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4;
            Unity_Add_float4(_Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4, _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4);
            float4 _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4;
            Unity_Saturate_float4(_Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4, _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4);
            float4 _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4;
            Unity_Step_float4((_Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float.xxxx), _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4, _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4);
            float4 _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4 = _outline_color;
            float4 _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4, _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4);
            float _Split_a7787229b3a740c283ae554775467752_R_1_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[0];
            float _Split_a7787229b3a740c283ae554775467752_G_2_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[1];
            float _Split_a7787229b3a740c283ae554775467752_B_3_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[2];
            float _Split_a7787229b3a740c283ae554775467752_A_4_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[3];
            float _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Split_a7787229b3a740c283ae554775467752_A_4_Float, float(0), _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean);
            UnityTexture2D _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float4 _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.tex, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.samplerstate, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            if (_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode.x > 0)
                _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode);
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_R_4_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.r;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_G_5_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.g;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_B_6_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.b;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.a;
            float _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean;
            Unity_Comparison_Greater_float(_SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float, float(0), _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean);
            float4 _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4 = float4(float(0), float(0), float(0), float(1));
            float4 _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean, _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4, float4(0, 0, 0, 0), _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4);
            float4 _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4, _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4, _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4);
            #if defined(_ENABLED)
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            #else
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4;
            #endif
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_R_1_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[0];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_G_2_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[1];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_B_3_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[2];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[3];
            surface.Alpha = _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        #pragma shader_feature_local _ _ENABLED
        
        #if defined(_ENABLED)
            #define KEYWORD_PERMUTATION_0
        #else
            #define KEYWORD_PERMUTATION_1
        #endif
        
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float _color_threshold;
        float4 _outline_color;
        float _outline_size;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }
        
        void Unity_Length_float4(float4 In, out float Out)
        {
            Out = length(In);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        struct Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float
        {
        half4 uv0;
        };
        
        void SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(UnityTexture2D _base_texture, float _outline_thickness, float2 _main_pixel_displacement, float2 _target_pixel_displacement, Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float IN, out float4 OutVector4_1)
        {
        UnityTexture2D _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D = _base_texture;
        float2 _Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2 = _main_pixel_displacement;
        UnityTexture2D _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D = _base_texture;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Width_0_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.z;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Height_2_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.w;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.x;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.y;
        float2 _Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2 = float2(_TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float, _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float);
        float _Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float = _outline_thickness;
        float2 _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2, (_Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float.xx), _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2);
        float2 _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2, _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2);
        float4 _UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4 = IN.uv0;
        float2 _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2;
        Unity_Add_float2(_Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2, (_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2);
        float4 _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.tex, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.samplerstate, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.GetTransformedUV(_Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2) );
        if (_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_R_4_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.r;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_G_5_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.g;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_B_6_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.b;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.a;
        UnityTexture2D _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D = _base_texture;
        float2 _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2 = _target_pixel_displacement;
        float2 _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2, _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2);
        float2 _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2;
        Unity_Add_float2((_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2, _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2);
        float4 _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.tex, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.samplerstate, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.GetTransformedUV(_Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2) );
        if (_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_R_4_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.r;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_G_5_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.g;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_B_6_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.b;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.a;
        float _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float;
        Unity_Add_float(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float, _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float);
        float _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float;
        Unity_Saturate_float(_Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float, _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float);
        float4 _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4;
        Unity_Subtract_float4(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4);
        float _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float;
        Unity_Length_float4(_Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4, _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float);
        float _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float;
        Unity_Branch_float(((bool) _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float), _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float, float(0), _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float);
        OutVector4_1 = (_Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float.xxxx);
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Step_float4(float4 Edge, float4 In, out float4 Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float = _color_threshold;
            UnityTexture2D _Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_93b4916a36c248019358724eb00a44aa;
            _Compare2Colors_93b4916a36c248019358724eb00a44aa.uv0 = IN.uv0;
            float4 _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float, float2 (0, 1), float2 (0, -1), _Compare2Colors_93b4916a36c248019358724eb00a44aa, _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4);
            float _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_a71a0875648446779bbea5977759fb55;
            _Compare2Colors_a71a0875648446779bbea5977759fb55.uv0 = IN.uv0;
            float4 _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float, float2 (1, 0), float2 (-1, 0), _Compare2Colors_a71a0875648446779bbea5977759fb55, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4);
            float4 _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4;
            Unity_Add_float4(_Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4, _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4);
            float4 _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4;
            Unity_Saturate_float4(_Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4, _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4);
            float4 _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4;
            Unity_Step_float4((_Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float.xxxx), _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4, _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4);
            float4 _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4 = _outline_color;
            float4 _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4, _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4);
            float _Split_a7787229b3a740c283ae554775467752_R_1_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[0];
            float _Split_a7787229b3a740c283ae554775467752_G_2_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[1];
            float _Split_a7787229b3a740c283ae554775467752_B_3_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[2];
            float _Split_a7787229b3a740c283ae554775467752_A_4_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[3];
            float _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Split_a7787229b3a740c283ae554775467752_A_4_Float, float(0), _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean);
            UnityTexture2D _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float4 _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.tex, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.samplerstate, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            if (_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode.x > 0)
                _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode);
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_R_4_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.r;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_G_5_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.g;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_B_6_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.b;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.a;
            float _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean;
            Unity_Comparison_Greater_float(_SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float, float(0), _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean);
            float4 _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4 = float4(float(0), float(0), float(0), float(1));
            float4 _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean, _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4, float4(0, 0, 0, 0), _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4);
            float4 _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4, _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4, _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4);
            #if defined(_ENABLED)
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            #else
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4;
            #endif
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_R_1_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[0];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_G_2_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[1];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_B_3_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[2];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[3];
            surface.Alpha = _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Sprite Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma exclude_renderers d3d11_9x
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_vertex _ SKINNED_SPRITE
        #pragma shader_feature_local _ _ENABLED
        
        #if defined(_ENABLED)
            #define KEYWORD_PERMUTATION_0
        #else
            #define KEYWORD_PERMUTATION_1
        #endif
        
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX_NORMAL_OUTPUT
        #define FEATURES_GRAPH_VERTEX_TANGENT_OUTPUT
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_COLOR
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SPRITEFORWARD
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(ATTRIBUTES_NEED_INSTANCEID)
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 color : INTERP1;
             float3 positionWS : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.color.xyzw = input.color;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.color = input.color.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED || defined(VARYINGS_NEED_INSTANCEID)
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_TexelSize;
        float _color_threshold;
        float4 _outline_color;
        float _outline_size;
        UNITY_TEXTURE_STREAMING_DEBUG_VARS;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
        Out = A * B;
        }
        
        void Unity_Add_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A + B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float(float In, out float Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Subtract_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A - B;
        }
        
        void Unity_Length_float4(float4 In, out float Out)
        {
            Out = length(In);
        }
        
        void Unity_Branch_float(float Predicate, float True, float False, out float Out)
        {
            Out = Predicate ? True : False;
        }
        
        struct Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float
        {
        half4 uv0;
        };
        
        void SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(UnityTexture2D _base_texture, float _outline_thickness, float2 _main_pixel_displacement, float2 _target_pixel_displacement, Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float IN, out float4 OutVector4_1)
        {
        UnityTexture2D _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D = _base_texture;
        float2 _Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2 = _main_pixel_displacement;
        UnityTexture2D _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D = _base_texture;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Width_0_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.z;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_Height_2_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.w;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.x;
        float _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float = _Property_8fe8be9e0ca9456ca3a0e0e19706b4e4_Out_0_Texture2D.texelSize.y;
        float2 _Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2 = float2(_TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelWidth_3_Float, _TextureSize_e87fe496dcab403c87ea570d7c0c9fd1_TexelHeight_4_Float);
        float _Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float = _outline_thickness;
        float2 _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Vector2_5c2b05cb6c474a0784426e7449cfa0e0_Out_0_Vector2, (_Property_fb7f54ae13f24fd39e75477e8ecb6070_Out_0_Float.xx), _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2);
        float2 _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Property_d1423e9abe304a61a177c95895718653_Out_0_Vector2, _Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2);
        float4 _UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4 = IN.uv0;
        float2 _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2;
        Unity_Add_float2(_Multiply_9cfae9919fbe43218a62ecb836f496dd_Out_2_Vector2, (_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2);
        float4 _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.tex, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.samplerstate, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.GetTransformedUV(_Add_96ca55c1a39b4fd18800901c557c18b1_Out_2_Vector2) );
        if (_Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _Property_14fd5018d68f47e5b983932d269776ff_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_R_4_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.r;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_G_5_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.g;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_B_6_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.b;
        float _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float = _SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4.a;
        UnityTexture2D _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D = _base_texture;
        float2 _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2 = _target_pixel_displacement;
        float2 _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2;
        Unity_Multiply_float2_float2(_Multiply_1c9f48e8b4ba4cf08157539796731bb4_Out_2_Vector2, _Property_658d337d08a945d2aba58c34755d6a88_Out_0_Vector2, _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2);
        float2 _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2;
        Unity_Add_float2((_UV_87bdf2e615f6404d93b2ef198045e2dd_Out_0_Vector4.xy), _Multiply_35170f7f204848c987d8785788806aff_Out_2_Vector2, _Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2);
        float4 _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.tex, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.samplerstate, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.GetTransformedUV(_Add_32e01f90272e418185e666307fd0d4ab_Out_2_Vector2) );
        if (_Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode.x > 0)
        _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Property_419710fbfba44431a7fa7e5a9b405820_Out_0_Texture2D.hdrDecode);
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_R_4_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.r;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_G_5_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.g;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_B_6_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.b;
        float _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float = _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4.a;
        float _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float;
        Unity_Add_float(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_A_7_Float, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_A_7_Float, _Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float);
        float _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float;
        Unity_Saturate_float(_Add_7f135769e2684ae29ec08f301d2c15ae_Out_2_Float, _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float);
        float4 _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4;
        Unity_Subtract_float4(_SampleTexture2D_9cff7fc662d4439c9394e5d47c607118_RGBA_0_Vector4, _SampleTexture2D_91f149bf6271401996c99a1a2e3cf580_RGBA_0_Vector4, _Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4);
        float _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float;
        Unity_Length_float4(_Subtract_aa0152331e7f4df89e180e01498cae04_Out_2_Vector4, _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float);
        float _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float;
        Unity_Branch_float(((bool) _Saturate_e382fc5d1b4b43aea81cb419dbfc8947_Out_1_Float), _Length_adbcb79f79b94e769cd3055dc17ac1aa_Out_1_Float, float(0), _Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float);
        OutVector4_1 = (_Branch_45f9b83b1ac84067a1485a73fee8d53e_Out_3_Float.xxxx);
        }
        
        void Unity_Add_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A + B;
        }
        
        void Unity_Saturate_float4(float4 In, out float4 Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Step_float4(float4 Edge, float4 In, out float4 Out)
        {
            Out = step(Edge, In);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Comparison_Greater_float(float A, float B, out float Out)
        {
            Out = A > B ? 1 : 0;
        }
        
        void Unity_Branch_float4(float Predicate, float4 True, float4 False, out float4 Out)
        {
            Out = Predicate ? True : False;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float = _color_threshold;
            UnityTexture2D _Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_93b4916a36c248019358724eb00a44aa;
            _Compare2Colors_93b4916a36c248019358724eb00a44aa.uv0 = IN.uv0;
            float4 _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_e42dbdc71b474366b59c5fa2b1345b0e_Out_0_Float, float2 (0, 1), float2 (0, -1), _Compare2Colors_93b4916a36c248019358724eb00a44aa, _Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4);
            float _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float = _outline_size;
            Bindings_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float _Compare2Colors_a71a0875648446779bbea5977759fb55;
            _Compare2Colors_a71a0875648446779bbea5977759fb55.uv0 = IN.uv0;
            float4 _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4;
            SG_Compare2Colors_6edf98ee02866d4489b3228a2d360c11_float(_Property_a1b34728b61c4a60b2cfdd0eac2c9e64_Out_0_Texture2D, _Property_0e2dfcc9f24f429c8eace28cff124da3_Out_0_Float, float2 (1, 0), float2 (-1, 0), _Compare2Colors_a71a0875648446779bbea5977759fb55, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4);
            float4 _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4;
            Unity_Add_float4(_Compare2Colors_93b4916a36c248019358724eb00a44aa_OutVector4_1_Vector4, _Compare2Colors_a71a0875648446779bbea5977759fb55_OutVector4_1_Vector4, _Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4);
            float4 _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4;
            Unity_Saturate_float4(_Add_4cc997f07003496eab3c0d51b10d0a44_Out_2_Vector4, _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4);
            float4 _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4;
            Unity_Step_float4((_Property_7e4cd87c68be4a609e2284e77858ddb3_Out_0_Float.xxxx), _Saturate_3ea88d487e594e2b98d1836821709810_Out_1_Vector4, _Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4);
            float4 _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4 = _outline_color;
            float4 _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Step_e4aafca1cf524d849f54a531406d8777_Out_2_Vector4, _Property_cf54d181ae25432da9e84608ff4dfc45_Out_0_Vector4, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4);
            float _Split_a7787229b3a740c283ae554775467752_R_1_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[0];
            float _Split_a7787229b3a740c283ae554775467752_G_2_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[1];
            float _Split_a7787229b3a740c283ae554775467752_B_3_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[2];
            float _Split_a7787229b3a740c283ae554775467752_A_4_Float = _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4[3];
            float _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean;
            Unity_Comparison_Greater_float(_Split_a7787229b3a740c283ae554775467752_A_4_Float, float(0), _Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean);
            UnityTexture2D _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D = UnityBuildTexture2DStructInternal(_MainTex, sampler_MainTex, _MainTex_TexelSize, float4(1, 1, 0, 0), float4(0, 0, 0, 0));
            float4 _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.tex, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.samplerstate, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            if (_Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode.x > 0)
                _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4 = DecodeHDRSample(_SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4, _Property_f68c1055ba2944e08acbe71ebaf0d26e_Out_0_Texture2D.hdrDecode);
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_R_4_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.r;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_G_5_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.g;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_B_6_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.b;
            float _SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4.a;
            float _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean;
            Unity_Comparison_Greater_float(_SampleTexture2D_affb4a6116c041a682932552e43e46da_A_7_Float, float(0), _Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean);
            float4 _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4 = float4(float(0), float(0), float(0), float(1));
            float4 _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_8cdbcc4deab94f58a73fe5cf4240babc_Out_2_Boolean, _Vector4_5c3feeb7a6884dfe88aa2e3dc5145fcf_Out_0_Vector4, float4(0, 0, 0, 0), _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4);
            float4 _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            Unity_Branch_float4(_Comparison_497e37d243064c7c84dedbbc5c93e8f3_Out_2_Boolean, _Multiply_cdfc580ccd864f2c8ccd1fa4adcd8cbb_Out_2_Vector4, _Branch_986d552a138d48339c74af703fdea613_Out_3_Vector4, _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4);
            #if defined(_ENABLED)
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _Branch_819d08c64259422598b4b93c218f928e_Out_3_Vector4;
            #else
            float4 _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4 = _SampleTexture2D_affb4a6116c041a682932552e43e46da_RGBA_0_Vector4;
            #endif
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_R_1_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[0];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_G_2_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[1];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_B_3_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[2];
            float _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float = _enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4[3];
            surface.BaseColor = (_enabled_72d9bf40b9b247609d018f05ff6d0e29_Out_0_Vector4.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _Split_0ab53f3bdc6e4fa58753b7971b35b6c6_A_4_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if UNITY_ANY_INSTANCING_ENABLED
        #else // TODO: XR support for procedural instancing because in this case UNITY_ANY_INSTANCING_ENABLED is not defined and instanceID is incorrect.
        #endif
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphSpriteGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}