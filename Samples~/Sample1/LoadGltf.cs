using UnityEngine;
using UnityGltf;

public class LoadGltf : MonoBehaviour
{
	public string gltfPath;

	private void Awake()
	{
		AssetManager.Instance.LoadGltf(gltfPath, gameObject);
	}
}
