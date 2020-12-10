using UnityEngine;

namespace TAO.VertexAnimation
{
    public static class VA_Texture2DArrayUtils
    {
        public static Texture2DArray CreateTextureArray(Texture2D[] a_textures, bool a_useMipChain, bool a_isLinear,
            TextureWrapMode a_wrapMode = TextureWrapMode.Repeat, FilterMode a_filterMode = FilterMode.Bilinear, int a_anisoLevel = 1, string a_name = "")
        {
            if(!IsValidForTextureArray(a_textures))
            {
                return null;
            }

            Texture2DArray textureArray = new Texture2DArray(a_textures[0].width, a_textures[0].height, a_textures.Length, a_textures[0].format, a_useMipChain, a_isLinear);

            for (int i = 0; i < a_textures.Length; i++)
            {
                Graphics.CopyTexture(a_textures[i], 0, 0, textureArray, i, 0);
            }

            textureArray.wrapMode = a_wrapMode;
            textureArray.filterMode = a_filterMode;
            textureArray.anisoLevel = a_anisoLevel;
            textureArray.name = a_name;

            textureArray.Apply(false, false);

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
            }

            return true;
        }
    }
}