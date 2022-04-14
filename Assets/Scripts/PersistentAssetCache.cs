using System;
using UnityEngine;

namespace UnityGltf
{
	public class PersistentAssetCache : IDisposable
	{
		public int nodeCount;
		public int bufferCount;
		public Texture2D[] images;
		public Mesh[] meshes;
		public Material[] materials;
		public AnimationClip[] animations;

		public PersistentAssetCache(AssetCache cache)
		{
			nodeCount = cache.nodes?.Length ?? 0;
			bufferCount = cache.buffers?.Length ?? 0;
			images = cache.images;
			meshes = cache.meshes;
			materials = cache.materials;
			animations = cache.animations;
		}

		public void Dispose()
		{
			foreach (var image in images)
				UnityEngine.Object.Destroy(image);

			foreach (var mesh in meshes)
				UnityEngine.Object.Destroy(mesh);

			foreach (var material in materials)
				UnityEngine.Object.Destroy(material);

			foreach (var animation in animations)
				UnityEngine.Object.Destroy(animation);
		}
	}
}