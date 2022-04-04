using glTFLoader.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityGltf.Utilities;
using UnityEngine;

namespace UnityGltf
{
	public class NodeLoader
	{
		private GltfLoaderData data;

		private MeshLoader meshLoader;
		private ColliderLoader colliderLoader;

		public NodeLoader(GltfLoaderData data)
		{
			this.data = data;

			meshLoader = new MeshLoader(data);
			colliderLoader = new ColliderLoader(data);
		}

		public void LoadNode(int nodeIndex, Transform parent)
		{
			var node = data.gltf.Nodes[nodeIndex];

			// Create object for node
			var name = string.IsNullOrEmpty(node.Name) ? $"Node{nodeIndex}" : node.Name;
			var obj = new GameObject(name);
			var transform = obj.transform;
			transform.SetParent(parent);

			data.cache.nodes[nodeIndex] = obj;

			// Set object transform
			var matrix = GetNodeMatrix(node);
			transform.localPosition = matrix.GetPosition();
			transform.localRotation = matrix.GetRotation();
			transform.localScale = matrix.GetScale();

			var meshIndex = node.Mesh;
			if (meshIndex.HasValue)
				meshLoader.LoadMeshAndMaterials(meshIndex.Value, node.Skin, obj);

			var colliderExtension = ExtensionUtil.LoadExtension<Unity_collidersNodeExtension>(node.Extensions, "Unity_colliders");
			if (colliderExtension != null)
				colliderLoader.LoadCollider(colliderExtension.Collider.Value, obj);

			// Recursively load child nodes
			var childNodeIndices = node.Children;
			if (childNodeIndices != null)
			{
				foreach (var childNodeIndex in childNodeIndices)
					LoadNode(childNodeIndex, transform);
			}
		}

		private Matrix4x4 GetNodeMatrix(Node node)
		{
			var matrix = TypeConverter.ConvertMatrix4x4(node.Matrix);
			var matrixTrs = GetNodeMatrixTrs(node);

			return matrix * matrixTrs;
		}

		private Matrix4x4 GetNodeMatrixTrs(Node node)
		{
			var translation = TypeConverter.ConvertVector3(node.Translation);
			var rotation = TypeConverter.ConvertQuaternion(node.Rotation);
			var scale = TypeConverter.ConvertVector3(node.Scale, false);

			var matrixTrs = Matrix4x4.TRS(translation, rotation, scale);
			return matrixTrs;
		}
	}
}