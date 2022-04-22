using UnityEngine;
using UnityEngine.Rendering;

namespace UnityGltf
{
	public class MaterialLoader
	{
		private GltfLoaderData data;
		private TextureLoader textureLoader;

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

				unityMaterial = CreatePbrMetallicRoughnessMaterial(material.Name);
				LoadDoubleSided(material, unityMaterial);
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
			return CreatePbrMetallicRoughnessMaterial("Default");
		}

		private Material CreatePbrMetallicRoughnessMaterial(string materialName)
		{
			var shader = Shader.Find("GLTF/PbrMetallicRoughness");
			var unityMaterial = new Material(shader);
			unityMaterial.name = materialName;

			return unityMaterial;
		}

		private void LoadDoubleSided(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			if (material.DoubleSided)
				unityMaterial.SetInt("_Cull", (int)CullMode.Off);
			else
				unityMaterial.SetInt("_Cull", (int)CullMode.Back);
		}

		private void LoadAlphaMode(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			switch (material.AlphaMode)
			{
				case glTFLoader.Schema.Material.AlphaModeEnum.MASK:
					unityMaterial.SetOverrideTag("RenderType", "TransparentCutout");
					unityMaterial.SetFloat("_Mode", 1f);
					unityMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
					unityMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
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
					unityMaterial.SetFloat("_Mode", 2f);
					unityMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
					unityMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
					unityMaterial.SetInt("_ZWrite", 0);
					unityMaterial.DisableKeyword("_ALPHATEST_ON");
					unityMaterial.EnableKeyword("_ALPHABLEND_ON");
					unityMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					unityMaterial.renderQueue = (int)RenderQueue.Transparent;
					break;
				case glTFLoader.Schema.Material.AlphaModeEnum.OPAQUE:
				default:
					unityMaterial.SetOverrideTag("RenderType", "Opaque");
					unityMaterial.SetFloat("_Mode", 0f);
					unityMaterial.SetInt("_SrcBlend", (int)BlendMode.One);
					unityMaterial.SetInt("_DstBlend", (int)BlendMode.Zero);
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
			if (pbrMat != null)
			{
				// Base color factor
				var baseColorFactor = TypeConverter.ConvertColor(pbrMat.BaseColorFactor);
				unityMaterial.SetColor("_Color", baseColorFactor);

				// Base color texture
				var baseColorTexture = pbrMat.BaseColorTexture;
				if (baseColorTexture != null)
				{
					var texture = textureLoader.LoadTexture(baseColorTexture.Index, false);
					unityMaterial.SetTexture("_MainTex", texture);
				}

				// Metallic factor
				var metallic = pbrMat.MetallicFactor;
				unityMaterial.SetFloat("_Metallic", metallic);

				// Roughness factor
				var roughness = pbrMat.RoughnessFactor;
				unityMaterial.SetFloat("_Glossiness", roughness);

				// Metallic-roughness texture
				var metallicRoughnessTexture = pbrMat.MetallicRoughnessTexture;
				if (metallicRoughnessTexture != null)
				{
					var texture = textureLoader.LoadTexture(metallicRoughnessTexture.Index, true);
					unityMaterial.SetTexture("_MetallicGlossMap", texture);

					unityMaterial.EnableKeyword("_METALLICGLOSSMAP");
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
				unityMaterial.SetFloat("_BumpScale", scale);

				// Texture
				var texture = textureLoader.LoadTexture(normalTexture.Index, true);
				unityMaterial.SetTexture("_BumpMap", texture);

				unityMaterial.EnableKeyword("_NORMALMAP");
			}
		}

		private void LoadEmissiveMap(glTFLoader.Schema.Material material, Material unityMaterial)
		{
			var emissiveTexture = material.EmissiveTexture;
			if (emissiveTexture != null || material.ShouldSerializeEmissiveFactor())
			{
				// Strength
				var strengthExtension = ExtensionUtil.LoadExtension<glTFLoader.Schema.Khr_materials_emissive_strength>(material.Extensions, "KHR_materials_emissive_strength");
				var strength = strengthExtension != null ? strengthExtension.EmissiveStrength : 1f;

				// Factor
				var factor = TypeConverter.ConvertColor(material.EmissiveFactor);
				unityMaterial.SetColor("_EmissionColor", factor * strength);

				// Texture
				if (emissiveTexture != null)
				{
					var texture = textureLoader.LoadTexture(emissiveTexture.Index, false);
					unityMaterial.SetTexture("_EmissionMap", texture);
				}

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
				var texture = textureLoader.LoadTexture(occlusionTexture.Index, true);
				unityMaterial.SetTexture("_OcclusionMap", texture);
			}
		}
	}
}