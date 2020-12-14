using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
    [CreateAssetMenu(fileName = "new AnimationBook", menuName = "AnimationBook", order = 0)]
    public class VA_AnimationBook : ScriptableObject
    {
        public int maxFrames;
        public List<TextureGroup> textureGroups = new List<TextureGroup>() { new TextureGroup { shaderParamName = "_PositionMap", textureType = TextureType.AnimationMap, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, isLinear = false } };
        public List<VA_AnimationPage> animationPages = new List<VA_AnimationPage>();

        public Material[] materials;
        public List<Texture2DArray> texture2DArray = null;

        public void Create()
        {
            GenerateTextures();
            SetMaterials();
        }

        private void OnValidate()
        {
            // TODO: Check for naming conflicts and textures.
            // TODO: Debug message box instead of debug logs.
        }

        private void ReferenceDuplicates()
        {
            for (int i = 0; i < textureGroups.Count; i++)
            {
                List<Texture2D> t = new List<Texture2D>();

                for (int j = 0; j < animationPages.Count; j++)
                {
                    // Check if exist.
                    if (!t.Contains(animationPages[j].textures[i].texture2D))
                    {
                        t.Add(animationPages[j].textures[i].texture2D);
                    }

                    // Add index reference.
                    animationPages[j].textures[i].textureArrayIndex = t.IndexOf(animationPages[j].textures[i].texture2D);
                }
            }
        }

        private void GenerateTextures()
        {
            ReferenceDuplicates();

            texture2DArray.Clear();

            for (int i = 0; i < textureGroups.Count; i++)
            {
                var t = GetTextures(i).ToArray();
                if (VA_Texture2DArrayUtils.IsValidForTextureArray(t))
                {
                    texture2DArray.Add(VA_Texture2DArrayUtils.CreateTextureArray(t, false, textureGroups[i].isLinear, textureGroups[i].wrapMode, textureGroups[i].filterMode, 1, name + textureGroups[i].shaderParamName));
                }
            }
        }

        private List<Texture2D> GetTextures(int textureIndex)
        {
            List<Texture2D> textures = new List<Texture2D>();

            foreach (var ap in animationPages)
            {
                // Check if exist.
                if (!textures.Contains(ap.textures[textureIndex].texture2D))
                {
                    textures.Add(ap.textures[textureIndex].texture2D);
                }
            }

            return textures;
        }

        private void SetMaterials()
        {
            if (materials != null)
            {
                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        if (mat.HasProperty("_MaxFrames"))
                        {
                            mat.SetFloat("_MaxFrames", maxFrames);
                        }

                        for (int i = 0; i < texture2DArray.Count; i++)
                        {
                            if (mat.HasProperty(textureGroups[i].shaderParamName))
                            {
                                mat.SetTexture(textureGroups[i].shaderParamName, texture2DArray[i]);
                            }
                        }
                    }
                }
            }
        }

        public List<VA_AnimationData> GetAnimationData()
        {
            List<VA_AnimationData> data = new List<VA_AnimationData>();

            foreach (var ap in animationPages)
            {               
                data.Add(new VA_AnimationData
                {
                    name = ap.name,
                    frames = ap.frames,
                    maxFrames = maxFrames,
                    frameTime = 1.0f / maxFrames,
                    duration = 1.0f / maxFrames * ap.frames,
                    animationMapIndex = GetFirstAnimationMapIndex(in ap.textures),
                    colorMapIndex = GetFirstColorMapIndex(in ap.textures)
                });
            }

            return data;
        }

        private int GetFirstAnimationMapIndex(in List<TextureEntry> textures)
        {
            for (int i = 0; i < textureGroups.Count; i++)
            {
                if(textureGroups[i].textureType == TextureType.AnimationMap)
                {
                    return textures[i].textureArrayIndex;
                }
            }

            return -1;
        }

        private int GetFirstColorMapIndex(in List<TextureEntry> textures)
        {
            for (int i = 0; i < textureGroups.Count; i++)
            {
                if (textureGroups[i].textureType == TextureType.ColorMap)
                {
                    return textures[i].textureArrayIndex;
                }
            }

            return -1;
        }
    }

    [System.Serializable]
    public struct VA_AnimationPage
    {
        public string name;
        public int frames;
        public List<TextureEntry> textures;
    }

    [System.Serializable]
    public struct TextureGroup
    {
        public string shaderParamName;
        public TextureType textureType;
        public TextureWrapMode wrapMode;
        public FilterMode filterMode;
        public bool isLinear;
    }

    [System.Serializable]
    public class TextureEntry
    {
        public Texture2D texture2D = null;
        public int textureArrayIndex = -1;
    }

    public enum TextureType
    {
        AnimationMap,
        ColorMap
    }
}