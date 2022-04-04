using glTFLoader;
using System;
using UnityDds;
using UnityEngine;
using UnityGltf.Utilities;

namespace UnityGltf
{
	public class TextureLoader
	{
		private GltfLoaderData data;

		public TextureLoader(GltfLoaderData data)
		{
			this.data = data;
		}

		public Texture2D LoadTexture(int textureIndex)
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
