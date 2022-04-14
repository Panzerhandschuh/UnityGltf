using glTFLoader;
using UnityGltf.Utilities;
using System;
using System.IO;

namespace UnityGltf
{
	public class GltfLoaderData : IDisposable
	{
		public string path;
		public glTFLoader.Schema.Gltf gltf;
		public AssetCache cache;
		public Func<string, Stream> pathResolver;

		public GltfLoaderData(string path, glTFLoader.Schema.Gltf gltf, AssetCache cache, Func<string, Stream> pathResolver)
		{
			this.path = path;
			this.gltf = gltf;
			this.cache = cache;
			this.pathResolver = pathResolver;
		}

		public void Dispose()
		{
			cache.Dispose();
		}

		public Func<string, byte[]> GetExternalReferenceSolver()
		{
			return asset =>
			{
				if (string.IsNullOrEmpty(asset))
				{
					using (var stream = pathResolver(path))
						return Interface.LoadBinaryBuffer(stream);
				}
				else
				{
					var pathToResolve = Path.Combine(Path.GetDirectoryName(path), asset);
					using (var stream = pathResolver(pathToResolve))
						return stream.ReadAllBytes();
				}
			};
		}
	}
}