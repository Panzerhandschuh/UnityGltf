using glTFLoader;
using System;
using System.IO;
using UnityEngine;

namespace UnityGltf
{
	public class AccessorLoader
	{
		private GltfLoaderData data;

		public AccessorLoader(GltfLoaderData data)
		{
			this.data = data;
		}

		public Array LoadAccessor(int accessorIndex)
		{
			var accessor = data.gltf.Accessors[accessorIndex];
			var bufferView = data.gltf.BufferViews[accessor.BufferView.Value];
			var bufferIndex = bufferView.Buffer;

			var bufferData = data.cache.buffers[bufferIndex];
			if (bufferData == null)
			{
				bufferData = LoadData(bufferIndex);
				data.cache.buffers[bufferIndex] = bufferData;
			}

			var offset = accessor.ByteOffset + bufferView.ByteOffset;
			var stride = bufferView.ByteStride;
			var size = GetTypeSize(accessor.ComponentType);

			switch (accessor.Type)
			{
				case glTFLoader.Schema.Accessor.TypeEnum.SCALAR:
					return CreateScalarArray(accessor, bufferData, offset, stride, size);
				case glTFLoader.Schema.Accessor.TypeEnum.VEC2:
					return CreateVec2Array(accessor, bufferData, offset, stride, size);
				case glTFLoader.Schema.Accessor.TypeEnum.VEC3:
					return CreateVec3Array(accessor, bufferData, offset, stride, size);
				case glTFLoader.Schema.Accessor.TypeEnum.VEC4:
					return CreateVec4Array(accessor, bufferData, offset, stride, size);
				case glTFLoader.Schema.Accessor.TypeEnum.MAT4:
					return CreateMat4Array(accessor, bufferData, offset, stride, size);
				default:
					throw new InvalidOperationException($"Invalid accessor type ({accessor.Type})");
			}
		}

		private byte[] LoadData(int bufferIndex)
		{
			var externalReferenceSolver = data.GetExternalReferenceSolver();
			return data.gltf.LoadBinaryBuffer(bufferIndex, externalReferenceSolver);
		}

		private int GetTypeSize(glTFLoader.Schema.Accessor.ComponentTypeEnum componentType)
		{
			switch (componentType)
			{
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.BYTE:
					return sizeof(sbyte);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
					return sizeof(byte);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.SHORT:
					return sizeof(short);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
					return sizeof(ushort);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_INT:
					return sizeof(uint);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.FLOAT:
					return sizeof(float);
				default:
					throw new InvalidOperationException($"Invalid component type ({componentType})");
			}
		}

		private Array CreateScalarArray(glTFLoader.Schema.Accessor accessor, byte[] data, int offset, int? stride, int size)
		{
			var arr = CreateScalarArrayInstance(accessor);
			var strideVal = stride ?? size;

			for (int i = 0; i < accessor.Count; i++)
			{
				var elementIndex = offset + i * strideVal;
				arr.SetValue(GetElement(accessor.ComponentType, data, elementIndex), i);
			}

			return arr;
		}

		private Array CreateScalarArrayInstance(glTFLoader.Schema.Accessor accessor)
		{
			var type = GetType(accessor.ComponentType);
			return Array.CreateInstance(type, accessor.Count);
		}

		private Type GetType(glTFLoader.Schema.Accessor.ComponentTypeEnum componentType)
		{
			switch (componentType)
			{
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.BYTE:
					return typeof(sbyte);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
					return typeof(byte);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.SHORT:
					return typeof(short);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
					return typeof(ushort);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_INT:
					return typeof(uint);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.FLOAT:
					return typeof(float);
				default:
					throw new InvalidOperationException($"Invalid component type ({componentType})");
			}
		}

		private Array CreateVec2Array(glTFLoader.Schema.Accessor accessor, byte[] data, int offset, int? stride, int size)
		{
			var arr = Array.CreateInstance(typeof(Vector2), accessor.Count);
			var strideVal = stride ?? size * 2;

			for (int i = 0; i < accessor.Count; i++)
			{
				var elementIndex = offset + i * strideVal;

				var vec = new Vector2();
				for (int j = 0; j < 2; j++)
					vec[j] = Convert.ToSingle(GetElement(accessor.ComponentType, data, elementIndex + size * j));

				arr.SetValue(vec, i);
			}

			return arr;
		}

		private Array CreateVec3Array(glTFLoader.Schema.Accessor accessor, byte[] data, int offset, int? stride, int size)
		{
			var arr = Array.CreateInstance(typeof(Vector3), accessor.Count);
			var strideVal = stride ?? size * 3;

			for (int i = 0; i < accessor.Count; i++)
			{
				var elementIndex = offset + i * strideVal;

				var vec = new Vector3();
				for (int j = 0; j < 3; j++)
					vec[j] = Convert.ToSingle(GetElement(accessor.ComponentType, data, elementIndex + size * j));

				arr.SetValue(vec, i);
			}

			return arr;
		}

		private Array CreateVec4Array(glTFLoader.Schema.Accessor accessor, byte[] data, int offset, int? stride, int size)
		{
			var arr = Array.CreateInstance(typeof(Vector4), accessor.Count);
			var strideVal = stride ?? size * 4;

			for (int i = 0; i < accessor.Count; i++)
			{
				var elementIndex = offset + i * strideVal;

				var vec = new Vector4();
				for (int j = 0; j < 4; j++)
					vec[j] = Convert.ToSingle(GetElement(accessor.ComponentType, data, elementIndex + size * j));

				arr.SetValue(vec, i);
			}

			return arr;
		}

		private Array CreateMat4Array(glTFLoader.Schema.Accessor accessor, byte[] data, int offset, int? stride, int size)
		{
			var arr = Array.CreateInstance(typeof(Matrix4x4), accessor.Count);
			var strideVal = stride ?? size * 16;

			for (int i = 0; i < accessor.Count; i++)
			{
				var elementIndex = offset + i * strideVal;

				var mat = new Matrix4x4();
				for (int j = 0; j < 16; j++)
					mat[j] = Convert.ToSingle(GetElement(accessor.ComponentType, data, elementIndex + size * j));

				arr.SetValue(mat, i);
			}

			return arr;
		}

		private object GetElement(glTFLoader.Schema.Accessor.ComponentTypeEnum componentType, byte[] buffer, int index)
		{
			switch (componentType)
			{
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.BYTE:
					return (sbyte)buffer[index];
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_BYTE:
					return buffer[index];
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.SHORT:
					return BitConverter.ToInt16(buffer, index);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_SHORT:
					return BitConverter.ToUInt16(buffer, index);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.UNSIGNED_INT:
					return BitConverter.ToUInt32(buffer, index);
				case glTFLoader.Schema.Accessor.ComponentTypeEnum.FLOAT:
					return BitConverter.ToSingle(buffer, index);
				default:
					throw new InvalidOperationException($"Invalid component type ({componentType})");
			}
		}
	}
}