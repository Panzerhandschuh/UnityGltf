using glTFLoader;
using NUnit.Framework;

namespace UnityGltf.Tests
{
	public class GltfInterfaceTest
	{
		[TestCase(@"Assets\Scenes\Lantern\Lantern.gltf")]
		public void CanReadGltfFile(string path)
		{
			var gltf = Interface.LoadModel(path);
			Assert.NotNull(gltf);
		}
	}
}