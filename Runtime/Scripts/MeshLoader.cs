using System;
using System.Collections.Generic;
using glTFLoader;
using UnityEngine;

namespace UnityGltf
{
	public class MeshLoader
	{
		private GltfLoaderData data;

		private MaterialLoader materialLoader;
		private AccessorLoader accessorLoader;

		public MeshLoader(GltfLoaderData data)
		{
			this.data = data;

			materialLoader = new MaterialLoader(data);
			accessorLoader = new AccessorLoader(data);
		}

		/// <summary>
		/// Loads the mesh and materials for the specified mesh index
		/// </summary>
		public void LoadMeshAndMaterials(int meshIndex, int? skinIndex, GameObject obj)
		{
			var mesh = LoadMesh(meshIndex);
			var materials = LoadMaterials(meshIndex);

			AddMeshFilter(obj, mesh);
			AddRenderer(obj, skinIndex, materials, mesh);
		}

		/// <summary>
		/// Loads only the mesh for the specified mesh index
		/// </summary>
		public Mesh LoadMesh(int meshIndex)
		{
			var mesh = data.cache.meshes[meshIndex];
			if (mesh == null)
			{
				var primitives = GetPrimitives(meshIndex);
				mesh = LoadMesh(primitives);

				data.cache.meshes[meshIndex] = mesh;
			}

			return mesh;
		}

		private Mesh LoadMesh(glTFLoader.Schema.MeshPrimitive[] primitives)
		{
			var subMeshes = new GltfMesh[primitives.Length];
			for (int i = 0; i < primitives.Length; i++)
			{
				var primitive = primitives[i];
				subMeshes[i] = LoadSubMesh(primitive);
			}

			return CreateUnityMesh(subMeshes);
		}

		/// <summary>
		/// Loads only the materials for the specified mesh index
		/// </summary>
		public Material[] LoadMaterials(int meshIndex)
		{
			var primitives = GetPrimitives(meshIndex);
			return LoadMaterials(primitives);
		}

		private Material[] LoadMaterials(glTFLoader.Schema.MeshPrimitive[] primitives)
		{
			var materials = new Material[primitives.Length];
			for (int i = 0; i < primitives.Length; i++)
			{
				var primitive = primitives[i];
				materials[i] = LoadMaterial(primitive, materialLoader);
			}

			return materials;
		}

		private glTFLoader.Schema.MeshPrimitive[] GetPrimitives(int meshIndex)
		{
			var mesh = data.gltf.Meshes[meshIndex];
			return mesh.Primitives;
		}

		private Material LoadMaterial(glTFLoader.Schema.MeshPrimitive primitive, MaterialLoader materialLoader)
		{
			var materialIndex = primitive.Material;
			if (materialIndex.HasValue)
				return materialLoader.LoadMaterial(materialIndex.Value);
			else
				return materialLoader.LoadDefaultMaterial();
		}

		private GltfMesh LoadSubMesh(glTFLoader.Schema.MeshPrimitive primitive)
		{
			var mesh = new GltfMesh();

			// Consider moving this logic into another class (SubMeshLoader.cs?)
			foreach (var attribute in primitive.Attributes)
				LoadMeshAttribute(mesh, attribute.Key, attribute.Value);

			LoadMeshAttribute(mesh, "INDEX", primitive.Indices.GetValueOrDefault());

			return mesh;
		}

		private void LoadMeshAttribute(GltfMesh mesh, string attributeName, int accessorIndex)
		{
			var arr = accessorLoader.LoadAccessor(accessorIndex);
			switch (attributeName)
			{
				case "POSITION":
					{
						var vertices = (Vector3[])arr;
						CoordinateSystemConverter.Convert(vertices);
						mesh.vertices = vertices;
						break;
					}
				case "NORMAL":
					{
						var normals = (Vector3[])arr;
						CoordinateSystemConverter.Convert(normals);
						mesh.normals = normals;
						break;
					}
				case "TANGENT":
					{
						var tangents = (Vector4[])arr;
						CoordinateSystemConverter.Convert(tangents);
						mesh.tangents = tangents;
						break;
					}
				case "TEXCOORD_0":
					{
						var texCoords = (Vector2[])arr;
						CoordinateSystemConverter.FlipTexCoords(texCoords);
						mesh.texCoords = texCoords;
						break;
					}
				case "INDEX":
					{
						var indices = new int[arr.Length];
						for (int i = 0; i < arr.Length; i++)
							indices[i] = Convert.ToInt32(arr.GetValue(i));

						CoordinateSystemConverter.FlipIndices(indices);
						mesh.indices = indices;
						break;
					}
				case "COLOR_0":
					{
						var colors = (Vector4[])arr;
						mesh.colors = TypeConverter.ConvertColors(colors);
						break;
					}
				case "JOINTS_0":
					mesh.joints = (Vector4[])arr;
					break;
				case "WEIGHTS_0":
					mesh.weights = (Vector4[])arr;
					break;
				default:
					Debug.LogWarning($"Invalid accessor name ({attributeName})");
					break;
			}
		}

		private Mesh CreateUnityMesh(GltfMesh[] subMeshes)
		{
			var unityMesh = new Mesh();

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var tangents = new List<Vector4>();
			var texCoords = new List<Vector2>();
			var colors = new List<Color>();
			var joints = new List<Vector4>();
			var weights = new List<Vector4>();
			foreach (var subMesh in subMeshes)
			{
				if (subMesh.vertices != null)
					vertices.AddRange(subMesh.vertices);
				if (subMesh.normals != null)
					normals.AddRange(subMesh.normals);
				if (subMesh.tangents != null)
					tangents.AddRange(subMesh.tangents);
				if (subMesh.texCoords != null)
					texCoords.AddRange(subMesh.texCoords);
				if (subMesh.colors != null)
					colors.AddRange(subMesh.colors);
				if (subMesh.joints != null)
					joints.AddRange(subMesh.joints);
				if (subMesh.weights != null)
					weights.AddRange(subMesh.weights);
			}

			unityMesh.SetVertices(vertices);
			unityMesh.SetNormals(normals);
			unityMesh.SetTangents(tangents);
			unityMesh.SetUVs(0, texCoords);
			unityMesh.SetColors(colors);
			unityMesh.boneWeights = CreateBoneWeights(joints, weights);

			var vertexCount = 0;
			unityMesh.subMeshCount = subMeshes.Length;
			for (int i = 0; i < subMeshes.Length; i++)
			{
				var subMesh = subMeshes[i];

				var indices = subMesh.indices;
				IncrementIndicesByVertexCount(indices, vertexCount);
				unityMesh.SetTriangles(indices, i);

				vertexCount += subMesh.vertices.Length;
			}

			if (unityMesh.normals.Length == 0)
				unityMesh.RecalculateNormals();

			if (unityMesh.tangents.Length == 0)
				unityMesh.RecalculateTangents();

			return unityMesh;
		}

		private BoneWeight[] CreateBoneWeights(List<Vector4> joints, List<Vector4> weights)
		{
			NormalizeWeights(weights);

			var boneWeights = new BoneWeight[joints.Count];
			for (int i = 0; i < joints.Count; i++)
			{
				boneWeights[i].boneIndex0 = (int)joints[i].x;
				boneWeights[i].boneIndex1 = (int)joints[i].y;
				boneWeights[i].boneIndex2 = (int)joints[i].z;
				boneWeights[i].boneIndex3 = (int)joints[i].w;

				boneWeights[i].weight0 = weights[i].x;
				boneWeights[i].weight1 = weights[i].y;
				boneWeights[i].weight2 = weights[i].z;
				boneWeights[i].weight3 = weights[i].w;
			}

			return boneWeights;
		}

		/// <summary>
		/// Ensures that bone weights add up to 1
		/// </summary>
		private void NormalizeWeights(List<Vector4> weights)
		{
			for (int i = 0; i < weights.Count; i++)
			{
				var sum = (weights[i].x + weights[i].y + weights[i].z + weights[i].w);
				if (!Mathf.Approximately(sum, 1f))
					weights[i] /= sum;
			}
		}

		private void IncrementIndicesByVertexCount(int[] indices, int vertexCount)
		{
			for (int i = 0; i < indices.Length; i++)
				indices[i] += vertexCount;
		}

		private void AddMeshFilter(GameObject obj, Mesh unityMesh)
		{
			var filter = obj.AddComponent<MeshFilter>();
			filter.sharedMesh = unityMesh;
		}

		private void AddRenderer(GameObject obj, int? skinIndex, Material[] materials, Mesh unityMesh)
		{
			Renderer renderer;
			if (skinIndex != null)
				renderer = AddSkinnedRenderer(obj, skinIndex, unityMesh);
			else
				renderer = obj.AddComponent<MeshRenderer>();

			renderer.materials = materials;
		}

		private Renderer AddSkinnedRenderer(GameObject obj, int? skinIndex, Mesh unityMesh)
		{
			var skin = data.gltf.Skins[skinIndex.Value];
			var skinnedRenderer = obj.AddComponent<SkinnedMeshRenderer>();
			skinnedRenderer.sharedMesh = unityMesh;
			LoadBones(skin, skinnedRenderer, unityMesh);

			return skinnedRenderer;
		}

		private void LoadBones(glTFLoader.Schema.Skin skin, SkinnedMeshRenderer renderer, Mesh unityMesh)
		{
			var inverseBindMatrices = (Matrix4x4[])accessorLoader.LoadAccessor(skin.InverseBindMatrices.Value);
			CoordinateSystemConverter.Convert(inverseBindMatrices);

			var boneCount = skin.Joints.Length;
			var bones = new Transform[boneCount];
			var bindPoses = new Matrix4x4[boneCount];
			for (int i = 0; i < boneCount; i++)
			{
				bones[i] = data.cache.nodes[skin.Joints[i]].transform;
				bindPoses[i] = inverseBindMatrices[i];
			}

			renderer.rootBone = data.cache.nodes[skin.Skeleton.Value].transform;
			renderer.bones = bones;
			unityMesh.bindposes = bindPoses;
		}
	}
}
