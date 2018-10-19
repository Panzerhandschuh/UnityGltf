using UnityEngine;

namespace UnityGltf
{
	public class AssetManagerReference : MonoBehaviour
	{
		public string gltfPath;

		private bool isInitialized;

		public void Init(string gltfPath)
		{
			this.gltfPath = gltfPath;
			Init();
		}

		private void Start()
		{
			Init();
		}

		private void Init()
		{
			if (isInitialized)
				return;

			AssetManager.Instance.IncrementReferenceCounter(gltfPath);
			isInitialized = true;
		}

		private void OnDestroy()
		{
			AssetManager.Instance.DecrementReferenceCounter(gltfPath);
		}
	}
}