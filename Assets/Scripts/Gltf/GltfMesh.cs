using glTFLoader.Schema;
using UnityEngine;

namespace UnityGltf
{
	public class GltfMesh
	{
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector4[] tangents;
		public Vector2[] texCoords;
		public int[] indices;
		public Vector4[] joints;
		public Vector4[] weights;
	}
}