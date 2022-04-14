using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace UnityGltf
{
	public class AssetManager
	{
		private static AssetManager instance;
		public static AssetManager Instance
		{
			get
			{
				if (instance == null)
					instance = new AssetManager();

				return instance;
			}
		}

		private Dictionary<string, ReferenceCounter<PersistentAssetCache>> assets;
		public ReadOnlyDictionary<string, ReferenceCounter<PersistentAssetCache>> Assets
		{
			get
			{
				return new ReadOnlyDictionary<string, ReferenceCounter<PersistentAssetCache>>(assets);
			}
		}

		public AssetManager()
		{
			assets = new Dictionary<string, ReferenceCounter<PersistentAssetCache>>();
		}

		/// <summary>
		/// Loads a gltf file with a pre-existing asset cache (if any)
		/// </summary>
		public GameObject LoadGltf(string gltfPath, GameObject root = null, Func<string, Stream> pathResolver = null)
		{
			var cache = GetAssetCache(gltfPath);
			var loader = new GltfLoader(gltfPath, root, cache, pathResolver);
			return LoadObject(loader);
		}

		private GameObject LoadObject(GltfLoader loader)
		{
			var obj = loader.Load();

			var path = loader.GetPath();
			var cache = loader.GetCache();
			AddReference(path, cache);

			var reference = obj.AddComponent<AssetManagerReference>();
			reference.Init(path);

			return obj;
		}

		/// <summary>
		/// Stores a reference to the specified AssetCache if it does not already exist in the reference dictionary.
		/// </summary>
		private void AddReference(string gltfPath, AssetCache cache)
		{
			ReferenceCounter<PersistentAssetCache> refCounter;
			if (!assets.ContainsKey(gltfPath))
			{
				var persistentCache = new PersistentAssetCache(cache);
				refCounter = new ReferenceCounter<PersistentAssetCache>(persistentCache);

				assets.Add(gltfPath, refCounter);
			}
		}

		/// <summary>
		/// Increments the reference counter at the specified gltfPath.
		/// </summary>
		public void IncrementReferenceCounter(string gltfPath)
		{
			ReferenceCounter<PersistentAssetCache> refCounter;
			if (assets.TryGetValue(gltfPath, out refCounter))
				refCounter.counter++;
		}

		/// <summary>
		/// Decrements the reference counter at the specified gltfPath.
		/// </summary>
		public void DecrementReferenceCounter(string gltfPath)
		{
			ReferenceCounter<PersistentAssetCache> refCounter;
			if (assets.TryGetValue(gltfPath, out refCounter))
			{
				refCounter.counter--;
				if (refCounter.counter <= 0)
				{
					refCounter.reference.Dispose();
					assets.Remove(gltfPath);
				}
			}
		}

		/// <summary>
		/// Gets the AssetCache at the specified path. If it doesn't exist, return null.
		/// </summary>
		public AssetCache GetAssetCache(string gltfPath)
		{
			ReferenceCounter<PersistentAssetCache> refCounter;
			if (assets.TryGetValue(gltfPath, out refCounter))
				return new AssetCache(refCounter.reference);

			return null;
		}

		/// <summary>
		/// Prints all loaded assets. Used for debugging.
		/// </summary>
		public void PrintAssetPaths()
		{
			foreach (var kv in assets)
				Debug.Log($"glTF File: {kv.Key}\tReferences: {kv.Value.counter}");
		}
	}
}