using System;
using UnityEngine;

namespace UnityGltf
{
	public class AssetCache : IDisposable
	{
		public GameObject[] nodes;
		public byte[][] buffers;
		public Texture2D[] images;
		public Mesh[] meshes;
		public Material[] materials;
		public AnimationClip[] animations;

		public AssetCache(glTFLoader.Schema.Gltf gltf)
		{
			nodes = new GameObject[gltf.Nodes?.Length ?? 0];
			buffers = new byte[gltf.Buffers?.Length ?? 0][];
			images = new Texture2D[gltf.Images?.Length ?? 0];
			meshes = new Mesh[gltf.Meshes?.Length ?? 0];
			materials = new Material[gltf.Materials?.Length ?? 0];
			animations = new AnimationClip[gltf.Animations?.Length ?? 0];
		}

		public AssetCache(PersistentAssetCache cache)
		{
			nodes = new GameObject[cache.nodeCount];
			buffers = new byte[cache.bufferCount][];
			images = cache.images;
			meshes = cache.meshes;
			materials = cache.materials;
			animations = cache.animations;
		}

		public void Dispose()
		{
			nodes = null;
			buffers = null;
			images = null;
			meshes = null;
			materials = null;
			animations = null;
		}
	}
}