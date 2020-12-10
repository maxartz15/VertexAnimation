using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
    [CreateAssetMenu(fileName = "new AnimationBook", menuName = "AnimationBook", order = 0)]
    public class VA_AnimationBook : ScriptableObject
    {
        public int maxFrames;
        public List<TextureGroup> textureGroups = new List<TextureGroup>() { new TextureGroup { shaderParamName = "_PositionMap", isLinear = false } };
        public List<VA_AnimationPage> animationPages = new List<VA_AnimationPage>();

        public Material[] materials;
        public List<Texture2DArray> texture2DArray = null;

        public void Create()
        {
            // Create textures.
            texture2DArray.Clear();
            foreach (var item in GetTextures())
            {
                if(VA_Texture2DArrayUtils.IsValidForTextureArray(item.Value.ToArray()))
                {
                    texture2DArray.Add(VA_Texture2DArrayUtils.CreateTextureArray(item.Value.ToArray(), false, textureGroups[item.Key].isLinear, TextureWrapMode.Repeat, FilterMode.Point, 1, name + "-" + item.Key.ToString()));
                }
            }

            // Assign material parameters.
            if(materials != null)
            {
                foreach (Material mat in materials)
                {
                    if(mat != null)
                    {
                        for (int i = 0; i < texture2DArray.Count; i++)
                        {
                            mat.SetTexture(textureGroups[i].shaderParamName, texture2DArray[i]);
                        }
                    }
                }
            }
        }

        private void OnValidate()
        {
            foreach (var item in GetTextures())
            {
                VA_Texture2DArrayUtils.IsValidForTextureArray(item.Value.ToArray());
            }
        }

        private Dictionary<int, List<Texture2D>> GetTextures()
        {
            Dictionary<int, List<Texture2D>> dict = new Dictionary<int, List<Texture2D>>();

            // Group and collect the textures.
            for (int i = 0; i < textureGroups.Count; i++)
            {
                dict.Add(i, new List<Texture2D>());

                for (int j = 0; j < animationPages.Count; j++)
                {
                    dict[i].Add(animationPages[j].textures[i]);
                }
            }

            return dict;
        }
    }

    [System.Serializable]
    public struct VA_AnimationPage
    {
        public string name;
        public int frames;
        public List<Texture2D> textures;
    }

    [System.Serializable]
    public struct TextureGroup
    {
        public string shaderParamName;
        public bool isLinear;
    }
}