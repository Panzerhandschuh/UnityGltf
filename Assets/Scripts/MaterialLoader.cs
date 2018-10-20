using System;
using glTFLoader;
using UnityDds;
using UnityEngine;
using UnityEngine.Rendering;
using UnityGltf.Utilities;

namespace UnityGltf
{
	public class MaterialLoader
	{
		private GltfLoaderData data;

		public MaterialLoader(GltfLoaderData data)
		{
			this.data = data;
		}

		public Material LoadMaterial(int materialIndex)
		{
			var unityMaterial = data.cache.materials[materialIndex];
			if (unityMaterial == null)
			{
				var material = data.gltf.Materials[materialIndex];

				unityMaterial = CreateUnityMaterial(material.Name);
				LoadAlphaMode(material, unityMaterial);
				LoadMetallicRoughnessMap(material, unityMaterial);
				LoadNormalMap(material, unityMaterial);
				LoadEmissiveMap(material, unityMaterial);
				LoadOcclusionMap(material, unityMaterial);

				data.cache.materials[materialIndex] = unityMaterial;
			}

			return unityMaterial;
		}

		public Material LoadDefaultMaterial()
		{
			return CreateUnityMaterial("Default");
		}

		private Material CreateUnityMaterial(string materialName)
		{
			var shader = Shader.Find("Standard");
			var unityMaterial = new Material(shader);
			unityMaterial.name = materialName;

			return unityMaterial;
		}

		private void LoadAlphaMode(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			switch (material.AlphaMode)
			{
				case glTFLoader.Schema.Material.AlphaModeEnum.MASK:
					unityMaterial.SetOverrideTag("RenderType", "TransparentCutout");
					unityMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					unityMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					unityMaterial.SetInt("_ZWrite", 1);
					unityMaterial.EnableKeyword("_ALPHATEST_ON");
					unityMaterial.DisableKeyword("_ALPHABLEND_ON");
					unityMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					unityMaterial.renderQueue = (int)RenderQueue.AlphaTest;
					if (unityMaterial.HasProperty("_Cutoff"))
						unityMaterial.SetFloat("_Cutoff", material.AlphaCutoff);
					break;
				case glTFLoader.Schema.Material.AlphaModeEnum.BLEND:
					unityMaterial.SetOverrideTag("RenderType", "Transparent");
					unityMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
					unityMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					unityMaterial.SetInt("_ZWrite", 0);
					unityMaterial.DisableKeyword("_ALPHATEST_ON");
					unityMaterial.EnableKeyword("_ALPHABLEND_ON");
					unityMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					unityMaterial.renderQueue = (int)RenderQueue.Transparent;
					break;
				case glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE:
				default:
					unityMaterial.SetOverrideTag("RenderType", "Opaque");
					unityMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					unityMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
					unityMaterial.SetInt("_ZWrite", 1);
					unityMaterial.DisableKeyword("_ALPHATEST_ON");
					unityMaterial.DisableKeyword("_ALPHABLEND_ON");
					unityMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					unityMaterial.renderQueue = (int)RenderQueue.Geometry;
					break;
			}
		}

		private void LoadMetallicRoughnessMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var pbrMat = material.PbrMetallicRoughness;
			if (pbrMat == null)
				return;

			// Base color factor
			var baseColorFactor = TypeConverter.ConvertColor(pbrMat.BaseColorFactor);
			unityMaterial.SetColor("_Color", baseColorFactor);

			// Base color texture
			var baseColorTexture = pbrMat.BaseColorTexture;
			if (baseColorTexture != null)
			{
				var texture = LoadTexture(baseColorTexture.Index);
				unityMaterial.SetTexture("_MainTex", texture);
			}

			// Metallic factor
			var metallic = pbrMat.MetallicFactor;
			unityMaterial.SetFloat("_Metallic", metallic);

			// Roughness factor
			var roughness = pbrMat.RoughnessFactor;
			unityMaterial.SetFloat("_Glossiness", 1f - roughness);

			// Metallic-roughness texture
			var metallicRoughnessTexture = pbrMat.MetallicRoughnessTexture;
			if (metallicRoughnessTexture != null)
			{
				var texture = LoadTexture(metallicRoughnessTexture.Index);
				unityMaterial.SetTexture("_MetallicGlossMap", texture);

				unityMaterial.EnableKeyword("_METALLICGLOSSMAP");
			}
		}

		private void LoadNormalMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var normalTexture = material.NormalTexture;
			if (normalTexture != null)
			{
				// Scale
				var scale = normalTexture.Scale;
				unityMaterial.SetFloat("_BumpScale", scale);

				// Texture
				var texture = LoadTexture(normalTexture.Index);
				unityMaterial.SetTexture("_BumpMap", texture);

				unityMaterial.EnableKeyword("_NORMALMAP");
			}
		}

		private void LoadEmissiveMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var emissiveTexture = material.EmissiveTexture;
			if (emissiveTexture != null)
			{
				// Factor
				var factor = TypeConverter.ConvertColor(material.EmissiveFactor);
				unityMaterial.SetColor("_EmissionColor", factor);

				// Texture
				var texture = LoadTexture(emissiveTexture.Index);
				unityMaterial.SetTexture("_EmissionMap", texture);

				unityMaterial.EnableKeyword("_EMISSION");
			}
		}

		private void LoadOcclusionMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var occlusionTexture = material.OcclusionTexture;
			if (occlusionTexture != null)
			{
				// Strength
				var strength = occlusionTexture.Strength;
				unityMaterial.SetFloat("_OcclusionStrength", strength);

				// Texture
				var texture = LoadTexture(occlusionTexture.Index);
				unityMaterial.SetTexture("_OcclusionMap", texture);
			}
		}

		private Texture2D LoadTexture(int textureIndex)
		{
			var texture = data.gltf.Textures[textureIndex];

			var ddsExtension = ExtensionUtil.LoadExtension<glTFLoader.Schema.Msft_texture_ddsExtension>(texture.Extensions, "MSFT_texture_dds");
			if (ddsExtension != null && ddsExtension.Source.HasValue)
				return LoadDdsTexture(ddsExtension.Source.Value, texture.Sampler);
			else if (texture.Source.HasValue)
				return LoadImageTexture(texture.Source.Value, texture.Sampler);

			return null;
		}

		private Texture2D LoadDdsTexture(int imageIndex, int? samplerIndex)
		{
			var unityTexture = data.cache.images[imageIndex];
			if (unityTexture == null)
			{
				unityTexture = LoadDds(imageIndex);
				LoadSampler(samplerIndex, unityTexture);

				data.cache.images[imageIndex] = unityTexture;
			}

			return unityTexture;
		}

		private Texture2D LoadDds(int imageIndex)
		{
			var externalReferenceSolver = data.GetExternalReferenceSolver();
			using (var stream = data.gltf.OpenImageFile(imageIndex, externalReferenceSolver))
			{
				return DdsTextureLoader.LoadTexture(stream);
			}
		}
		
		private Texture2D LoadImageTexture(int imageIndex, int? samplerIndex)
		{
			var unityTexture = data.cache.images[imageIndex];
			if (unityTexture == null)
			{
				unityTexture = LoadImage(imageIndex);
				LoadSampler(samplerIndex, unityTexture);

				data.cache.images[imageIndex] = unityTexture;
			}

			return unityTexture;
		}

		private Texture2D LoadImage(int imageIndex)
		{
			var texture = new Texture2D(0, 0);

			var data = LoadImageData(imageIndex);
			texture.LoadImage(data, true);

			return texture;
		}

		private byte[] LoadImageData(int imageIndex)
		{
			var externalReferenceSolver = data.GetExternalReferenceSolver();
			using (var stream = data.gltf.OpenImageFile(imageIndex, externalReferenceSolver))
			{
				return stream.ReadAllBytes();
			}
		}

		private void LoadSampler(int? samplerIndex, Texture2D unityTexture)
		{
			var desiredFilterMode = FilterMode.Trilinear;
			if (samplerIndex.HasValue)
			{
				var sampler = data.gltf.Samplers[samplerIndex.Value];

				var minFilter = sampler.MinFilter;
				if (minFilter.HasValue)
					desiredFilterMode = GetFilterMode(sampler.MinFilter.Value);

				unityTexture.wrapModeU = GetTextureWrapMode(sampler.WrapS);
				unityTexture.wrapModeV = GetTextureWrapMode(sampler.WrapT);
			}

			unityTexture.filterMode = desiredFilterMode;
			unityTexture.anisoLevel = GltfConfig.Instance.anisoLevel;
		}

		private FilterMode GetFilterMode(glTFLoader.Schema.Sampler.MinFilterEnum minFilter)
		{
			switch (minFilter)
			{
				case glTFLoader.Schema.Sampler.MinFilterEnum.NEAREST:
				case glTFLoader.Schema.Sampler.MinFilterEnum.NEAREST_MIPMAP_NEAREST:
				case glTFLoader.Schema.Sampler.MinFilterEnum.NEAREST_MIPMAP_LINEAR:
					return FilterMode.Point;
				case glTFLoader.Schema.Sampler.MinFilterEnum.LINEAR:
				case glTFLoader.Schema.Sampler.MinFilterEnum.LINEAR_MIPMAP_NEAREST:
					return FilterMode.Bilinear;
				case glTFLoader.Schema.Sampler.MinFilterEnum.LINEAR_MIPMAP_LINEAR:
					return FilterMode.Trilinear;
				default:
					throw new InvalidOperationException($"Invalid {nameof(glTFLoader.Schema.Sampler.MinFilterEnum)} value ({minFilter})");
			}
		}

		private TextureWrapMode GetTextureWrapMode(glTFLoader.Schema.Sampler.WrapSEnum wrapS)
		{
			switch (wrapS)
			{
				case glTFLoader.Schema.Sampler.WrapSEnum.CLAMP_TO_EDGE:
					return TextureWrapMode.Clamp;
				case glTFLoader.Schema.Sampler.WrapSEnum.MIRRORED_REPEAT:
					return TextureWrapMode.Mirror;
				case glTFLoader.Schema.Sampler.WrapSEnum.REPEAT:
					return TextureWrapMode.Repeat;
				default:
					throw new InvalidOperationException($"Invalid {nameof(glTFLoader.Schema.Sampler.WrapSEnum)} value ({wrapS})");
			}
		}

		private TextureWrapMode GetTextureWrapMode(glTFLoader.Schema.Sampler.WrapTEnum wrapT)
		{
			switch (wrapT)
			{
				case glTFLoader.Schema.Sampler.WrapTEnum.CLAMP_TO_EDGE:
					return TextureWrapMode.Clamp;
				case glTFLoader.Schema.Sampler.WrapTEnum.MIRRORED_REPEAT:
					return TextureWrapMode.Mirror;
				case glTFLoader.Schema.Sampler.WrapTEnum.REPEAT:
					return TextureWrapMode.Repeat;
				default:
					throw new InvalidOperationException($"Invalid {nameof(glTFLoader.Schema.Sampler.WrapTEnum)} value ({wrapT})");
			}
		}
	}
}