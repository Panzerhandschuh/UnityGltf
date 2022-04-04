using UnityEngine;
using UnityEngine.Rendering;

namespace UnityGltf
{
	public class MaterialLoader
	{
		private GltfLoaderData data;
		private TextureLoader textureLoader;

		private enum SurfaceType
		{
			Opaque = 0,
			Transparent = 1
		}

		public MaterialLoader(GltfLoaderData data)
		{
			this.data = data;
			textureLoader = new TextureLoader(data);
		}

		public Material LoadMaterial(int materialIndex)
		{
			var unityMaterial = data.cache.materials[materialIndex];
			if (unityMaterial == null)
			{
				var material = data.gltf.Materials[materialIndex];

				var isTransparent = material.AlphaMode != glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE;
				unityMaterial = CreateMaterial(isTransparent);
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
			return CreateMaterial(false);
		}

		private Material CreateMaterial(bool isTransparent)
		{
			if (isTransparent)
				return new Material(Resources.Load<Material>("LitTransparent"));
			else
				return new Material(Resources.Load<Material>("LitOpaque"));
		}

		private void LoadAlphaMode(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			if (material.AlphaMode == glTFLoader.Schema.Material.AlphaModeEnum.MASK)
			{
				unityMaterial.SetFloat("_AlphaCutoffEnable", 1f);
				unityMaterial.SetFloat("_AlphaCutoff", material.AlphaCutoff);
			}
		}

		private void LoadMetallicRoughnessMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var pbrMat = material.PbrMetallicRoughness;
			if (pbrMat != null)
			{
				// Base color factor
				var baseColorFactor = TypeConverter.ConvertColor(pbrMat.BaseColorFactor);
				unityMaterial.SetColor("_BaseColor", baseColorFactor);

				// Base color texture
				var baseColorTexture = pbrMat.BaseColorTexture;
				if (baseColorTexture != null)
				{
					var texture = textureLoader.LoadTexture(baseColorTexture.Index);
					unityMaterial.SetTexture("_BaseColorMap", texture);
				}

				// Metallic factor
				var metallic = pbrMat.MetallicFactor;
				unityMaterial.SetFloat("_Metallic", metallic);

				// Roughness factor
				var roughness = pbrMat.RoughnessFactor;
				unityMaterial.SetFloat("_Smoothness", 1f - roughness);

				// Metallic-roughness texture
				var metallicRoughnessTexture = pbrMat.MetallicRoughnessTexture;
				if (metallicRoughnessTexture != null)
				{
					var texture = textureLoader.LoadTexture(metallicRoughnessTexture.Index);
					unityMaterial.SetTexture("_MaskMap", texture);
				}
			}
		}

		private void LoadNormalMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var normalTexture = material.NormalTexture;
			if (normalTexture != null)
			{
				// Scale
				var scale = normalTexture.Scale;
				unityMaterial.SetFloat("_NormalScale", scale);

				// Texture
				var texture = textureLoader.LoadTexture(normalTexture.Index);
				unityMaterial.SetTexture("_NormalMap", texture);
			}
		}

		private void LoadEmissiveMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var emissiveTexture = material.EmissiveTexture;
			if (emissiveTexture != null || material.ShouldSerializeEmissiveFactor())
			{
				// Exposure Weight
				unityMaterial.SetFloat("_EmissiveExposureWeight", 0f);

				// Strength
				var strengthExtension = ExtensionUtil.LoadExtension<glTFLoader.Schema.Khr_materials_emissive_strength>(material.Extensions, "KHR_materials_emissive_strength");
				var strength = strengthExtension != null ? strengthExtension.EmissiveStrength : 1f;

				// Factor
				var factor = TypeConverter.ConvertColor(material.EmissiveFactor);
				unityMaterial.SetColor("_EmissiveColor", factor * strength);

				// Texture
				if (emissiveTexture != null)
				{
					var texture = textureLoader.LoadTexture(emissiveTexture.Index);
					unityMaterial.SetTexture("_EmissiveColorMap", texture);
					unityMaterial.EnableKeyword("_EMISSIVE_COLOR_MAP");
				}
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
				var texture = textureLoader.LoadTexture(occlusionTexture.Index);
				unityMaterial.SetTexture("_OcclusionMap", texture);
			}
		}
	}
}