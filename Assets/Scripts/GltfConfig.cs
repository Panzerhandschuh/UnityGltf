namespace UnityGltf
{
	public class GltfConfig
	{
		private static GltfConfig instance;
		public static GltfConfig Instance
		{
			get
			{
				if (instance == null)
					instance = new GltfConfig();

				return instance;
			}
		}

		public int anisoLevel;
	}
}
