using System;
using glTFLoader.Schema;
using UnityEngine;

namespace UnityGltf
{
	public class ColliderLoader
	{
		private GltfLoaderData data;

		private MeshLoader meshLoader;

		public ColliderLoader(GltfLoaderData data)
		{
			this.data = data;

			meshLoader = new MeshLoader(data);
		}

		public void LoadCollider(int colliderIndex, GameObject obj)
		{
			var colliderExtension = ExtensionUtil.LoadExtension<Unity_collidersGltfExtension>(data.gltf.Extensions, "Unity_colliders");
			var collider = colliderExtension.Colliders[colliderIndex];
			if (collider.BoxCollider != null)
				LoadBoxCollider(collider.BoxCollider, obj);
			else if (collider.SphereCollider != null)
				LoadSphereCollider(collider.SphereCollider, obj);
			else if (collider.CapsuleCollider != null)
				LoadCapsuleCollider(collider.CapsuleCollider, obj);
			else if (collider.MeshCollider != null)
				LoadMeshCollider(collider.MeshCollider, obj);
		}

		private void LoadBoxCollider(Unity_collidersBoxCollider boxCollider, GameObject obj)
		{
			var collider = obj.AddComponent<BoxCollider>();
			if (boxCollider.Center != null)
				collider.center = TypeConverter.ConvertVector3(boxCollider.Center);
			if (boxCollider.Size != null)
				collider.size = TypeConverter.ConvertVector3(boxCollider.Size, false);
		}

		private void LoadSphereCollider(Unity_collidersSphereCollider sphereCollider, GameObject obj)
		{
			var collider = obj.AddComponent<SphereCollider>();
			if (sphereCollider.Center != null)
				collider.center = TypeConverter.ConvertVector3(sphereCollider.Center);
			if (sphereCollider.Radius.HasValue)
				collider.radius = sphereCollider.Radius.Value;
		}

		private void LoadCapsuleCollider(Unity_collidersCapsuleCollider capsuleCollider, GameObject obj)
		{
			var collider = obj.AddComponent<CapsuleCollider>();
			if (capsuleCollider.Center != null)
				collider.center = TypeConverter.ConvertVector3(capsuleCollider.Center);
			if (capsuleCollider.Radius.HasValue)
				collider.radius = capsuleCollider.Radius.Value;
			if (capsuleCollider.Height.HasValue)
				collider.height = capsuleCollider.Height.Value;
			if (capsuleCollider.Direction.HasValue)
				collider.direction = GetDirection(capsuleCollider.Direction.Value);
		}

		private int GetDirection(Unity_collidersCapsuleCollider.DirectionEnum direction)
		{
			switch (direction)
			{
				case Unity_collidersCapsuleCollider.DirectionEnum.x:
					return 0;
				case Unity_collidersCapsuleCollider.DirectionEnum.y:
					return 1;
				case Unity_collidersCapsuleCollider.DirectionEnum.z:
					return 2;
				default:
					throw new InvalidOperationException($"Invalid capsule collider direction ({direction})");
			}
		}

		private void LoadMeshCollider(Unity_collidersMeshCollider meshCollider, GameObject obj)
		{
			var collider = obj.AddComponent<MeshCollider>();
			if (meshCollider.Convex.HasValue)
				collider.convex = meshCollider.Convex.Value;
			if (meshCollider.Mesh.HasValue)
			{
				var mesh = meshLoader.LoadMesh(meshCollider.Mesh.Value);
				collider.sharedMesh = mesh;
			}
		}
	}
}