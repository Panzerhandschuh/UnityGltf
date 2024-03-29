// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
	#define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
	#define _DETAIL 1
#endif

//---------------------------------------
half4	   _Color;
half		_Cutoff;

sampler2D   _MainTex;
float4	  _MainTex_ST;

sampler2D   _DetailAlbedoMap;
float4	  _DetailAlbedoMap_ST;

sampler2D   _BumpMap;
half		_BumpScale;

sampler2D   _DetailMask;
sampler2D   _DetailNormalMap;
half		_DetailNormalMapScale;

sampler2D   _SpecGlossMap;
sampler2D   _MetallicGlossMap;
half		_Metallic;
half		_Glossiness;

sampler2D   _OcclusionMap;
half		_OcclusionStrength;

sampler2D   _ParallaxMap;
half		_Parallax;
half		_UVSec;

half4	   _EmissionColor;
sampler2D   _EmissionMap;

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
	float4 vertex   : POSITION;
	half3 normal	: NORMAL;
	float2 uv0	  : TEXCOORD0;
	float2 uv1	  : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
	float2 uv2	  : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
	half4 tangent   : TANGENT;
#endif
#if defined(UNITY_VERTEX_INPUT_INSTANCE_ID)
	UNITY_VERTEX_INPUT_INSTANCE_ID
#else
	UNITY_VERTEX_INPUT_INSTANCE_ID
#endif
};

float4 TexCoords(VertexInput v)
{
	float4 texcoord;
	texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
	texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
	return texcoord;
}

half DetailMask(float2 uv)
{
	return tex2D (_DetailMask, uv).a;
}

#if defined(VERTEX_COLOR_ON)
half3 Albedo(float4 texcoords, half4 color)
#else
half3 Albedo(float4 texcoords)
#endif
{
	half3 albedo = _Color.rgb * tex2D(_MainTex, texcoords.xy).rgb;

#if _DETAIL
	#if (SHADER_TARGET < 30)
		// SM20: instruction count limitation
		// SM20: no detail mask
		half mask = 1;
	#else
		half mask = DetailMask(texcoords.xy);
	#endif
	half3 detailAlbedo = tex2D (_DetailAlbedoMap, texcoords.zw).rgb;
	#if _DETAIL_MULX2
		albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, mask);
	#elif _DETAIL_MUL
		albedo *= LerpWhiteTo (detailAlbedo, mask);
	#elif _DETAIL_ADD
		albedo += detailAlbedo * mask;
	#elif _DETAIL_LERP
		albedo = lerp (albedo, detailAlbedo, mask);
	#endif
#endif
	return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
	return _Color.a;
#else
	return tex2D(_MainTex, uv).a * _Color.a;
#endif
}

half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
	// SM20: instruction count limitation
	// SM20: simpler occlusion
	return tex2D(_OcclusionMap, uv).r;
#else
	half occ = tex2D(_OcclusionMap, uv).r;
	return LerpOneTo (occ, _OcclusionStrength);
#endif
}

half4 SpecularGloss(float2 uv)
{
	half4 sg;
#ifdef _SPECGLOSSMAP
	#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
		sg.rgb = tex2D(_SpecGlossMap, uv).rgb;
		sg.a = tex2D(_MainTex, uv).a;
	#else
		sg = tex2D(_SpecGlossMap, uv);
	#endif
	sg *= half4(_SpecColor.rgb, _Glossiness);
#else
	sg.rgb = _SpecColor.rgb;
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		sg.a = tex2D(_MainTex, uv).a * _Glossiness;
	#else
		sg.a = _Glossiness;
	#endif
#endif
	return sg;
}

half2 MetallicGloss(float2 uv)
{
	half2 mg;

#ifdef _METALLICGLOSSMAP
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		mg.r = tex2D(_MetallicGlossMap, uv).b;
		mg.g = tex2D(_MainTex, uv).a;
	#else
		mg = tex2D(_MetallicGlossMap, uv).bg;
	#endif
	mg *= half2(_Metallic, _Glossiness);
#else
	mg.r = _Metallic;
	#ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		mg.g = tex2D(_MainTex, uv).a * _Glossiness;
	#else
		mg.g = _Glossiness;
	#endif
#endif
	// it's roughness, not glossiness. invert it
	mg.g = 1.0f - mg.g;
	return mg;
}

half2 MetallicRough(float2 uv)
{
	half2 mg;

#if UNITY_SETUP_BRDF_INPUT == MetallicSetup
	#ifdef _METALLICGLOSSMAP
		mg = half2(_Metallic, _Glossiness) * tex2D(_MetallicGlossMap, uv).bg;
	#else
		mg = half2(_Metallic, _Glossiness);
	#endif
#else
	#ifdef _SPECGLOSSMAP
		mg.r = _Metallic;
		mg.g = 1.0f - _Glossiness * tex2D(_SpecGlossMap, uv).a;
	#else
		mg = half2(Metallic, 1.0f - _Glossiness);
	#endif
#endif
	return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
	return 0;
#else
	return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float4 texcoords)
{
	half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords.xy), _BumpScale);

#if _DETAIL && defined(UNITY_ENABLE_DETAIL_NORMALMAP)
	half mask = DetailMask(texcoords.xy);
	half3 detailNormalTangent = UnpackScaleNormal(tex2D (_DetailNormalMap, texcoords.zw), _DetailNormalMapScale);
	#if _DETAIL_LERP
		normalTangent = lerp(
			normalTangent,
			detailNormalTangent,
			mask);
	#else
		normalTangent = lerp(
			normalTangent,
			BlendNormals(normalTangent, detailNormalTangent),
			mask);
	#endif
#endif

	return normalTangent;
}
#endif

float4 Parallax (float4 texcoords, half3 viewDir)
{
// D3D9/SM30 supports up to 16 samplers, skip the parallax map in case we exceed the limit
#define EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT  (defined(LIGHTMAP_ON) && defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) && defined(_NORMALMAP) && \
											 defined(_EMISSION) && defined(_DETAIL) && (defined(_METALLICGLOSSMAP) || defined(_SPECGLOSSMAP)))

#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30) || (defined(SHADER_API_D3D9) && EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT)
	// SM20: instruction count limitation
	// SM20: no parallax
	return texcoords;
#else
	half h = tex2D (_ParallaxMap, texcoords.xy).g;
	float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
	return float4(texcoords.xy + offset, texcoords.zw + offset);
#endif

#undef EXCEEDS_D3D9_SM3_MAX_SAMPLER_COUNT
}

#endif // UNITY_STANDARD_INPUT_INCLUDED
