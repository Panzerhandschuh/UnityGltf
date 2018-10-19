using System;
using System.IO;
using glTFLoader;
using UnityEngine;

namespace UnityGltf
{
	public class GltfLoader : IDisposable
	{
		private GltfLoaderData data;

		private NodeLoader nodeLoader;
		private AnimationLoader animationLoader;

		private GameObject root;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Path to the gltf file.</param>
		/// <param name="root">Optional scene root. If not specified, a scene root will be created instead.</param>
		/// <param name="cache">Optional pre-allocated cache (useful for sharing cache data between multiple gltf instances).</param>
		/// <param name="pathResolver">Function for resolving uris/paths in gltf files to streams.</param>
		public GltfLoader(string path, GameObject root = null, AssetCache cache = null, Func<string, Stream> pathResolver = null)
		{
			if (pathResolver == null)
				pathResolver = GetDefaultPathResolver();

			glTFLoader.Schema.Gltf gltf;
			using (var stream = pathResolver(path))
				gltf = Interface.LoadModel(stream);

			if (cache == null)
				cache = new AssetCache(gltf);

			if (root != null)
				this.root = root;

			data = new GltfLoaderData(path, gltf, cache, pathResolver);

			nodeLoader = new NodeLoader(data);
			animationLoader = new AnimationLoader(data);
		}

		private static Func<string, Stream> GetDefaultPathResolver()
		{
			return path =>
			{
				return File.OpenRead(path);
			};
		}

		public void Dispose()
		{
			data.Dispose();
		}

		public GameObject Load()
		{
			var sceneIndex = data.gltf.Scene;
			if (!sceneIndex.HasValue)
				throw new IOException($"GLTF file has no scene ({data.path})");

			var obj = LoadScene(sceneIndex.Value);

			return obj;
		}

		private GameObject LoadScene(int sceneIndex)
		{
			var scene = data.gltf.Scenes[sceneIndex];

			if (root == null)
				root = new GameObject();

			root.name = Path.GetFileNameWithoutExtension(data.path);

			var nodeIndices = scene.Nodes;
			foreach (var nodeIndex in nodeIndices)
				nodeLoader.LoadNode(nodeIndex, root.transform);

			if (data.gltf.Animations != null && data.gltf.Animations.Length > 0)
				animationLoader.LoadAnimations(root);

			return root;
		}

		public string GetPath()
		{
			return data.path;
		}

		public AssetCache GetCache()
		{
			return data.cache;
		}
	}
}
