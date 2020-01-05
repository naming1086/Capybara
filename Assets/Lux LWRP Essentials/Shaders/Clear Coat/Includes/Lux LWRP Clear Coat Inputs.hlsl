#ifndef INPUT_LUXLWRP_BASE_INCLUDED
#define INPUT_LUXLWRP_BASE_INCLUDED



    #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
//  defines a bunch of helper functions (like lerpwhiteto)
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"  
//  defines SurfaceData, textures and the functions Alpha, SampleAlbedoAlpha, SampleNormal, SampleEmission
    #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"
//  defines e.g. "DECLARE_LIGHTMAP_OR_SH"
    #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
 
    #include "../Includes/Lux LWRP Clear Coat Lighting.hlsl"

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

//  Material Inputs
    CBUFFER_START(UnityPerMaterial)

        half    _ClearCoatSmoothness;
        half    _ClearCoatThickness;
        half3   _ClearCoatSpecular;

        half3   _BaseColor;
        half3   _SecondaryColor;
        half    _Smoothness;
        half    _Metallic;

    //  Needed by LitMetaPass
        float4  _BaseMap_ST;

        float4  _BumpMap_ST;
        half    _BumpScale;

        float4  _CoatMask_ST;

        half    _Occlusion;

        half4   _RimColor;
        half    _RimPower;
        half    _RimMinPower;
        half    _RimFrequency;
        half    _RimPerPositionFrequency;
            
    CBUFFER_END

//  Additional textures
    #if defined(_MASKMAP)
        TEXTURE2D(_CoatMask); SAMPLER(sampler_CoatMask);
    #endif
    #if defined(_MASKMAPSECONDARY)
        TEXTURE2D(_SecondaryMask); SAMPLER(sampler_SecondaryMask);
    #endif


//  Global Inputs

//  Structs
    struct VertexInput
    {
        float3 positionOS                   : POSITION;
        float3 normalOS                     : NORMAL;
        float4 tangentOS                    : TANGENT;
        float2 texcoord                     : TEXCOORD0;
        float2 lightmapUV                   : TEXCOORD1;
   //   half4 color                         : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct VertexOutput
    {
        float4 positionCS                   : SV_POSITION;

        #if defined(_MASKMAP)
            float4 uv                       : TEXCOORD0;
        #else
            float2 uv                       : TEXCOORD0;
        #endif

        #if !defined(UNITY_PASS_SHADOWCASTER) && !defined(DEPTHONLYPASS)
            DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
            #ifdef _ADDITIONAL_LIGHTS
                float3 positionWS           : TEXCOORD2;
            #endif
            #if defined(_NORMALMAP)
                half4 normalWS              : TEXCOORD3;
                half4 tangentWS             : TEXCOORD4;
                half4 bitangentWS           : TEXCOORD5;
            #else
                half3 normalWS              : TEXCOORD3;
                half3 viewDirWS             : TEXCOORD4;
            #endif

            half4 fogFactorAndVertexLight   : TEXCOORD6;
            
            #ifdef _MAIN_LIGHT_SHADOWS
                float4 shadowCoord          : TEXCOORD7;
            #endif
        #endif

        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    struct SurfaceDescription
    {
        half3 albedo;
        half alpha;
        half3 normalTS;
        half3 emission;
        half metallic;
        half3 specular;
        half smoothness;
        half occlusion;

        half clearCoatSmoothness;
        half clearCoatThickness;
    };

#endif