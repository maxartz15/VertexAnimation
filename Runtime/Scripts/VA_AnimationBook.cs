using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
    [CreateAssetMenu(fileName = "new AnimationBook", menuName = "VA_Animation/AnimationBook", order = 400)]
    public class VA_AnimationBook : ScriptableObject
    {
        public PlayData playData = null;
#if UNITY_EDITOR
        public EditorData editorData = new EditorData();
#endif

        private void OnValidate()
        {
            // TODO: Check for naming conflicts and textures.
            // TODO: Debug message box instead of debug logs.
        }

        public void SetMaterials()
        {
            if (playData.materials != null)
            {
                foreach (Material mat in playData.materials)
                {
                    if (mat != null)
                    {
                        if (mat.HasProperty("_MaxFrames"))
                        {
                            mat.SetFloat("_MaxFrames", playData.maxFrames);
                        }

                        for (int i = 0; i < playData.texture2DArray.Count; i++)
                        {
                            if (mat.HasProperty(playData.textureGroups[i].shaderParamName))
                            {
                                mat.SetTexture(playData.textureGroups[i].shaderParamName, playData.texture2DArray[i]);
                            }
                        }
                    }
                }
            }
        }

        #region PlayData
        [System.Serializable]
        public class PlayData
        {
            public List<PlayTextureGroup> textureGroups = new List<PlayTextureGroup>();
            public List<PlayAnimationPage> animationPages = new List<PlayAnimationPage>();

            public int maxFrames;
            public Material[] materials;
            public List<Texture2DArray> texture2DArray = new List<Texture2DArray>();

            // NOTE: for some reason FixedString32 data gets lost when entering play mode.
            // That is why this is here... and also the animationPages...
            public List<VA_AnimationData> GetAnimations
            {
                get
                {
                    List<VA_AnimationData> animations = new List<VA_AnimationData>();
                    foreach (var ap in animationPages)
                    {
                        animations.Add(new VA_AnimationData
                        {
                            name = ap.name,
                            frames = ap.frames,
                            maxFrames = maxFrames,
                            frameTime = 1.0f / maxFrames,
                            duration = 1.0f / maxFrames * ap.frames,
                            animationMapIndex = GetFirstAnimationMapIndex(in ap.textures, in textureGroups),
                            colorMapIndex = GetFirstColorMapIndex(in ap.textures, in textureGroups)
                        });
                    }
                    return animations;
                }
            }

            public static int GetFirstAnimationMapIndex(in List<PlayTextureEntry> textures, in List<PlayTextureGroup> textureGroups)
            {
                for (int i = 0; i < textureGroups.Count; i++)
                {
                    if (textureGroups[i].textureType == TextureType.AnimationMap)
                    {
                        return textures[i].textureArrayIndex;
                    }
                }

                return -1;
            }

            public static int GetFirstColorMapIndex(in List<PlayTextureEntry> textures, in List<PlayTextureGroup> textureGroups)
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
        public struct PlayAnimationPage
        {
            public string name;
            public int frames;
            public List<PlayTextureEntry> textures;
        }

        [System.Serializable]
        public struct PlayTextureGroup
        {
            public string shaderParamName;
            public TextureType textureType;
        }

        [System.Serializable]
        public struct PlayTextureEntry
        {
            public int textureArrayIndex;
        }
        #endregion

        #region EditorData
#if UNITY_EDITOR
        [System.Serializable]
        public class EditorData
        {
            public List<EditorTextureGroup> textureGroups = new List<EditorTextureGroup>() { new EditorTextureGroup { shaderParamName = "_PositionMap", textureType = TextureType.AnimationMap, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Point, isLinear = false } };
            public List<EditorAnimationPage> animationPages = new List<EditorAnimationPage>();

            public int maxFrames;
            public Material[] materials;
            public List<Texture2DArray> texture2DArray = null;
        }

        [System.Serializable]
        public struct EditorAnimationPage
        {
            public string name;
            public int frames;
            public List<EditorTextureEntry> textures;
        }

        [System.Serializable]
        public struct EditorTextureGroup
        {
            public string shaderParamName;
            public TextureType textureType;
            public TextureWrapMode wrapMode;
            public FilterMode filterMode;
            public bool isLinear;
        }

        [System.Serializable]
        public class EditorTextureEntry
        {
            public Texture2D texture2D = null;
            public int textureArrayIndex = -1;
        }
#endif
        #endregion

        public enum TextureType
        {
            AnimationMap,
            ColorMap
        }
    }
}