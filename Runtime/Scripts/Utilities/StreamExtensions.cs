using System.Collections.Generic;
using System.IO;

namespace UnityGltf.Utilities
{
	internal static class StreamExtensions
	{
		public static byte[] ReadAllBytes(this Stream stream)
		{
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				return memoryStream.ToArray();
			}
		}

		public static string ReadAllText(this Stream stream)
		{
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public static List<string> ReadAllLines(this Stream stream)
		{
			var lines = new List<string>();
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
					lines.Add(line);
			}

			return lines;
		}

		public static MemoryStream ToMemoryStream(this Stream stream)
		{
			var ms = new MemoryStream();
			stream.CopyTo(ms);
			ms.Position = 0;
			return ms;
		}
	}
}
