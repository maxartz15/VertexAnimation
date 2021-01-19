using System.Linq;

namespace TAO.VertexAnimation
{
	public static class NamingConventionUtils
	{
		public struct PositionMapInfo
		{
			public string name;
			public int frames;
			public int maxFrames;
			public int fps;
		}

		public static PositionMapInfo GetTextureInfo(this string name)
		{
			PositionMapInfo textureInfo = new PositionMapInfo();

			string[] parts = name.Split('_');
			foreach (var p in parts)
			{
				if (p.StartsWith("N-"))
				{
					textureInfo.name = p.Remove(0, 2);
				}
				else if (p.StartsWith("F-"))
				{
					if (int.TryParse(p.Remove(0, 2), out int frames))
					{
						textureInfo.frames = frames;
					}
				}
				else if (p.StartsWith("MF-"))
				{
					if (int.TryParse(p.Remove(0, 3), out int maxFrames))
					{
						textureInfo.maxFrames = maxFrames;
					}
				}
				else if (p.StartsWith("FPS-"))
				{
					if (int.TryParse(p.Remove(0, 4), out int fps))
					{
						textureInfo.fps = fps;
					}
				}
			}

			return textureInfo;
		}

		public static string ConvertToValidString(this string str)
		{
			return string.Concat(str.Where(char.IsLetterOrDigit));
		}
	}
}
