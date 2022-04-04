using UnityEngine;

namespace UnityGltf.Utilities
{
	internal static class Matrix4x4Extensions
	{
		public static Vector3 GetPosition(this Matrix4x4 mat)
		{
			return mat.GetColumn(3);
		}

		public static Quaternion GetRotation(this Matrix4x4 mat)
		{
			var forward = mat.GetColumn(2);
			var upwards = mat.GetColumn(1);

			return Quaternion.LookRotation(forward, upwards);
		}

		public static Vector3 GetScale(this Matrix4x4 mat)
		{
			var x = (Vector3)mat.GetColumn(0);
			var y = (Vector3)mat.GetColumn(1);
			var z = (Vector3)mat.GetColumn(2);

			return new Vector3(x.magnitude, y.magnitude, z.magnitude);
		}

		public static Vector3 GetRight(this Matrix4x4 mat)
		{
			return mat.GetColumn(0).normalized;
		}

		public static Vector3 GetUp(this Matrix4x4 mat)
		{
			return mat.GetColumn(1).normalized;
		}

		public static Vector3 GetForward(this Matrix4x4 mat)
		{
			return mat.GetColumn(2).normalized;
		}
	}
}