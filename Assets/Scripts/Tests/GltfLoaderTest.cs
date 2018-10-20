using System.Collections;
using UnityEngine.TestTools;

namespace UnityGltf.Tests
{
	public class GltfLoaderTest
	{
		[UnityTest]
		public IEnumerator CanReadGltfFile()
		{
			using (var loader = new GltfLoader(@"Assets\Scenes\Lantern\Lantern.gltf"))
				loader.Load();

			yield return null;
		}
	}
}
