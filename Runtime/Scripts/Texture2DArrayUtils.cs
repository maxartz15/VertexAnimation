using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class Texture2DArrayUtils
	{
		public static Texture2DArray CreateTextureArray(Texture2D[] a_textures, bool a_useMipChain, bool a_isLinear,
			TextureWrapMode a_wrapMode = TextureWrapMode.Repeat, FilterMode a_filterMode = FilterMode.Bilinear, int a_anisoLevel = 1, string a_name = "", bool a_makeNoLongerReadable = true)
		{
			if(!IsValidForTextureArray(a_textures) || !IsValidCopyTexturePlatform())
			{
				return null;
			}

			Texture2DArray textureArray = new Texture2DArray(a_textures[0].width, a_textures[0].height, a_textures.Length, a_textures[0].format, a_useMipChain, a_isLinear);

			if (IsValidCopyTexturePlatform())
			{
				for (int i = 0; i < a_textures.Length; i++)
				{
					Graphics.CopyTexture(a_textures[i], 0, 0, textureArray, i, 0);
				}
			}

			textureArray.wrapMode = a_wrapMode;
			textureArray.filterMode = a_filterMode;
			textureArray.anisoLevel = a_anisoLevel;
			textureArray.name = a_name;

			textureArray.Apply(false, a_makeNoLongerReadable);

			return textureArray;
		}

		public static bool IsValidForTextureArray(Texture2D[] a_textures)
		{
			if (a_textures == null || a_textures.Length <= 0)
			{
				Debug.LogError("No textures assigned!");
				return false;
			}

			for (int i = 0; i < a_textures.Length; i++)
			{
				if (a_textures[i] == null)
				{
					Debug.LogError("Texture " + i.ToString() + " not assigned!");
					return false;
				}

				if (a_textures[0].width != a_textures[i].width || a_textures[0].height != a_textures[i].height)
				{
					Debug.LogError("Texture " + a_textures[i].name + " has a different size!");
					return false;
				}

				if (a_textures[0].format != a_textures[i].format || a_textures[0].graphicsFormat != a_textures[i].graphicsFormat)
				{
					Debug.LogError("Texture " + a_textures[i].name + " has a different format/graphics format!");
					return false;
				}

				if (!a_textures[0].isReadable)
				{
#if UNITY_EDITOR
					//Debug.LogWarning("Texture " + a_textures[i].name + " is not readable!");
					return true;
#else
					Debug.LogError("Texture " + a_textures[i].name + " is not readable!");
					return false;
#endif
				}
			}

			return true;
		}

		public static bool IsValidCopyTexturePlatform()
		{
			switch (SystemInfo.copyTextureSupport)
			{
				case UnityEngine.Rendering.CopyTextureSupport.None:
					Debug.LogError("No CopyTextureSupport on this platform!");
					return false;
				case UnityEngine.Rendering.CopyTextureSupport.Basic:
					return true;
				case UnityEngine.Rendering.CopyTextureSupport.Copy3D:
					return true;
				case UnityEngine.Rendering.CopyTextureSupport.DifferentTypes:
					return true;
				case UnityEngine.Rendering.CopyTextureSupport.TextureToRT:
					return true;
				case UnityEngine.Rendering.CopyTextureSupport.RTToTexture:
					return true;
				default:
#if UNITY_EDITOR
					return true;
#else
					Debug.LogError("No CopyTextureSupport on this platform!");
					return false;
#endif
			}
		}
	}
}