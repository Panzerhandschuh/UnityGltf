using UnityGltf.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace UnityGltf
{
	public static class CoordinateSystemConverter
	{
		public static void Convert(Vector3[] vecs)
		{
			for (int i = 0; i < vecs.Length; i++)
				Convert(ref vecs[i]);
		}

		public static void Convert(ref Vector3 vec)
		{
			vec.x = -vec.x;
		}

		public static void Convert(Vector4[] vecs)
		{
			for (int i = 0; i < vecs.Length; i++)
				Convert(ref vecs[i]);
		}

		public static void Convert(ref Vector4 vec)
		{
			vec.x = -vec.x;
			vec.w = -vec.w;
		}

		public static void Convert(Quaternion[] quats)
		{
			for (int i = 0; i < quats.Length; i++)
				Convert(ref quats[i]);
		}

		public static void Convert(ref Quaternion quat)
		{
			quat.x = -quat.x;
			quat.w = -quat.w;
		}

		public static void Convert(Matrix4x4[] mats)
		{
			for (int i = 0; i < mats.Length; i++)
				Convert(ref mats[i]);
		}

		public static void Convert(ref Matrix4x4 mat)
		{
			var pos = mat.GetPosition();
			Convert(ref pos);

			var rot = mat.GetRotation();
			Convert(ref rot);

			var scale = mat.GetScale();

			mat.SetTRS(pos, rot, scale);
		}

		/// <summary>
		/// Reverses triangle winding order
		/// </summary>
		public static void FlipIndices(int[] indices)
		{
			for (int i = 0; i < indices.Length; i += 3)
			{
				var temp = indices[i];
				indices[i] = indices[i + 2];
				indices[i + 2] = temp;
			}
		}

		public static void FlipTexCoords(Vector2[] texCoords)
		{
			for (int i = 0; i < texCoords.Length; i++)
				texCoords[i].y = 1f - texCoords[i].y;
		}
	}
}