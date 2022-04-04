using UnityEngine;

namespace UnityGltf
{
	public static class TypeConverter
	{
		public static Vector2 ConvertVector2(float[] arr)
		{
			var vec = new Vector2();
			for (int i = 0; i < arr.Length; i++)
				vec[i] = arr[i];

			return vec;
		}

		public static Vector3 ConvertVector3(float[] arr, bool convertCoordinateSystem = true)
		{
			var vec = new Vector3();
			for (int i = 0; i < arr.Length; i++)
				vec[i] = arr[i];

			if (convertCoordinateSystem)
				CoordinateSystemConverter.Convert(ref vec);

			return vec;
		}

		public static Vector4 ConvertVector4(float[] arr, bool convertCoordinateSystem = true)
		{
			var vec = new Vector4();
			for (int i = 0; i < arr.Length; i++)
				vec[i] = arr[i];

			if (convertCoordinateSystem)
				CoordinateSystemConverter.Convert(ref vec);

			return vec;
		}

		public static Quaternion ConvertQuaternion(float[] arr, bool convertCoordinateSystem = true)
		{
			var quat = new Quaternion();
			for (int i = 0; i < arr.Length; i++)
				quat[i] = arr[i];

			if (convertCoordinateSystem)
				CoordinateSystemConverter.Convert(ref quat);

			return quat;
		}

		public static Color ConvertColor(float[] arr)
		{
			var color = new Color();
			for (int i = 0; i < arr.Length; i++)
				color[i] = arr[i];

			return color;
		}

		public static Matrix4x4 ConvertMatrix4x4(float[] arr, bool convertCoordinateSystem = true)
		{
			var mat = new Matrix4x4();
			for (int i = 0; i < arr.Length; i++)
				mat[i] = arr[i];

			if (convertCoordinateSystem)
				CoordinateSystemConverter.Convert(ref mat);

			return mat;
		}
	}
}
