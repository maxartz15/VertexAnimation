using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.Collections;

namespace TAO.VertexAnimation.Editor
{
    [InitializeOnLoad]
    public class VA_AssetBuilder : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private const string parentFolder = "Assets";
        private const string childFolder = "VA_AssetBuilder";
        private const string folderPath = parentFolder + "/" + childFolder;

        #region EditorMenus
        private const string buildClearKey = "VA_AssetsBuilderClearBuildAssets";
        private const string editorGenerateKey = "VA_AssetsBuilderGenerateEditorAssets";
        private const string editorClearKey = "VA_AssetsBuilderClearEditorAssets";

        private const string editorMenuFolder = "TAO/Vertex Animation";
        private const string buildClearMenuName = editorMenuFolder + "/ClearBuildAssets";
        private const string editorGeneratePlayMenuName = editorMenuFolder + "/GenerateEditorAssetsOnStartPlay";
        private const string editorClearPlayMenuName = editorMenuFolder + "/ClearEditorAssetsOnEndPlay";

        public static bool ClearBuildAssets
        {
            get { return EditorPrefs.GetBool(buildClearKey, true); }
            set { EditorPrefs.SetBool(buildClearKey, value); }
        }

        [MenuItem(buildClearMenuName)]
        private static void ToggleClearBuildAssetsAction()
        {
            ClearBuildAssets = !ClearBuildAssets;
        }

        [MenuItem(buildClearMenuName, true)]
        private static bool ToggleClearBuildAssetsValidate()
        {
            Menu.SetChecked(buildClearMenuName, ClearBuildAssets);
            return true;
        }

        public static bool GenerateEditorPlayModeAssets
        {
            get { return EditorPrefs.GetBool(editorGenerateKey, true); }
            set { EditorPrefs.SetBool(editorGenerateKey, value); }
        }

        [MenuItem(editorGeneratePlayMenuName)]
        private static void ToggleGenerateEditorPlayModeAssetsAction()
        {
            GenerateEditorPlayModeAssets = !GenerateEditorPlayModeAssets;
        }

        [MenuItem(editorGeneratePlayMenuName, true)]
        private static bool ToggleGenerateEditorPlayModeAssetsValidate()
        {
            Menu.SetChecked(editorGeneratePlayMenuName, GenerateEditorPlayModeAssets);
            return true;
        }

        public static bool ClearEditorPlayModeAssets
        {
            get { return EditorPrefs.GetBool(editorClearKey, true); }
            set { EditorPrefs.SetBool(editorClearKey, value); }
        }

        [MenuItem(editorClearPlayMenuName)]
        private static void ToggleClearEditorPlayModeAssetsAction()
        {
            ClearEditorPlayModeAssets = !ClearEditorPlayModeAssets;
        }

        [MenuItem(editorClearPlayMenuName, true)]
        private static bool ToggleClearEditorPlayModeAssetsValidate()
        {
            Menu.SetChecked(editorClearPlayMenuName, ClearEditorPlayModeAssets);
            return true;
        }
        #endregion

        #region EditorPlayMode
        static VA_AssetBuilder()
        {
            EditorApplication.playModeStateChanged += OnPlayModeEnter;
        }

        private static void OnPlayModeEnter(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (ClearEditorPlayModeAssets)
                    {
                        ClearBuildData();
                    }
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    if (GenerateEditorPlayModeAssets)
                    {
                        GenerateBuildData();
                    }
                    Debug.Log("VA_AssetBuilder generated editor data.");
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region BuildProcess
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            GenerateBuildData();
            Debug.Log("VA_AssetBuilder generated play data.");
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if(!ClearBuildAssets)
            {
                return;
            }

            ClearBuildData();
            Debug.Log("VA_AssetBuilder cleared play data.");
        }
        #endregion

        #region MainFunctions
        [MenuItem(editorMenuFolder + "/Generate Build Data", false, 65)]
        public static void GenerateBuildData()
        {
            string filter = string.Format("t:{0}", typeof(VA_AnimationBook).Name);
            string[] guids = AssetDatabase.FindAssets(filter);

            foreach (var guid in guids)
            {
                VA_AnimationBook book = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(VA_AnimationBook)) as VA_AnimationBook;

                // Generate all assets.
                GenerateTextures(ref book);

                // Assign run time data.
                ConvertEditorDataToPlayData(ref book);

                // Save them to disk.
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    AssetDatabase.CreateFolder(parentFolder, childFolder);
                }

                // Generate run time data.
                List<string> savedAssets = new List<string>();
                foreach (var t in book.editorData.texture2DArray)
                {
                    string assetPath = string.Format("{0}/{1}.asset", folderPath, t.name);

                    // Delete existing asset.
                    if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }

                    AssetDatabase.CreateAsset(t, assetPath);
                    savedAssets.Add(assetPath);
                }
                AssetDatabase.SaveAssets();

                book.playData.texture2DArray = new List<Texture2DArray>();
                foreach (var s in savedAssets)
                {
                    var savedT = AssetDatabase.LoadAssetAtPath(s, typeof(Texture2DArray)) as Texture2DArray;
                    book.playData.texture2DArray.Add(savedT);
                }
                AssetDatabase.SaveAssets();
            }
        }

        [MenuItem(editorMenuFolder + "/Clear Build Data", false, 66)]
        public static void ClearBuildData()
        {
            string filter = string.Format("t:{0}", typeof(VA_AnimationBook).Name);
            string[] guids = AssetDatabase.FindAssets(filter);

            // Clear Generated Data.
            foreach (var guid in guids)
            {
                VA_AnimationBook book = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(VA_AnimationBook)) as VA_AnimationBook;
                book.playData.texture2DArray = null;
            }

            // Remove generated assets from disk.
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.DeleteAsset(folderPath);
            }

            AssetDatabase.SaveAssets();
        }
        #endregion

        #region GenerationHelperFunctions
        // Assign the texture ID's to the texture entries.
        private static void ReferenceDuplicates(ref VA_AnimationBook book)
        {
            for (int i = 0; i < book.editorData.textureGroups.Count; i++)
            {
                List<Texture2D> t = new List<Texture2D>();

                for (int j = 0; j < book.editorData.animationPages.Count; j++)
                {
                    // Check if exist.
                    if (!t.Contains(book.editorData.animationPages[j].textures[i].texture2D))
                    {
                        t.Add(book.editorData.animationPages[j].textures[i].texture2D);
                    }

                    // Add index reference.
                    book.editorData.animationPages[j].textures[i].textureArrayIndex = t.IndexOf(book.editorData.animationPages[j].textures[i].texture2D);
                }
            }
        }

        // Get the textures from a specific texture index.
        private static List<Texture2D> GetTextures(ref VA_AnimationBook book, int textureIndex)
        {
            List<Texture2D> textures = new List<Texture2D>();

            foreach (var ap in book.editorData.animationPages)
            {
                // Check if exist.
                if (!textures.Contains(ap.textures[textureIndex].texture2D))
                {
                    textures.Add(ap.textures[textureIndex].texture2D);
                }
            }

            return textures;
        }

        // Generate texture arrays.
        public static void GenerateTextures(ref VA_AnimationBook book)
        {
            ReferenceDuplicates(ref book);

            book.editorData.texture2DArray = new List<Texture2DArray>();

            for (int i = 0; i < book.editorData.textureGroups.Count; i++)
            {
                var t = GetTextures(ref book, i).ToArray();

                if (VA_Texture2DArrayUtils.IsValidForTextureArray(t))
                {
                    book.editorData.texture2DArray.Add(VA_Texture2DArrayUtils.CreateTextureArray(t, false, book.editorData.textureGroups[i].isLinear, book.editorData.textureGroups[i].wrapMode, book.editorData.textureGroups[i].filterMode, 1, book.name + book.editorData.textureGroups[i].shaderParamName));
                }
            }
        }

        // Auto fill names and frames.
        public static void AutoFill(ref VA_AnimationBook book)
        {
            if (book.editorData.animationPages != null)
            {
                for (int i = 0; i < book.editorData.animationPages.Count; i++)
                {
                    VA_AnimationBook.EditorAnimationPage ap = book.editorData.animationPages[i];
                    if (ap.textures != null && ap.textures.Count > 0)
                    {
                        string textureName = ap.textures[0].texture2D.name;

                        string[] parts = textureName.Split('_');

                        foreach (var p in parts)
                        {
                            if (p.StartsWith("N-"))
                            {
                                ap.name = p.Remove(0, 2);
                            }
                            else if (p.StartsWith("F-"))
                            {
                                if (int.TryParse(p.Remove(0, 2), out int frames))
                                {
                                    ap.frames = frames;
                                }
                            }
                        }
                    }
                    book.editorData.animationPages[i] = ap;
                }
            }
        }

        public static int GetFirstAnimationMapIndex(in List<VA_AnimationBook.EditorTextureEntry> textures, in List<VA_AnimationBook.EditorTextureGroup> textureGroups)
        {
            for (int i = 0; i < textureGroups.Count; i++)
            {
                if (textureGroups[i].textureType == VA_AnimationBook.TextureType.AnimationMap)
                {
                    return textures[i].textureArrayIndex;
                }
            }

            return -1;
        }

        public static int GetFirstColorMapIndex(in List<VA_AnimationBook.EditorTextureEntry> textures, in List<VA_AnimationBook.EditorTextureGroup> textureGroups)
        {
            for (int i = 0; i < textureGroups.Count; i++)
            {
                if (textureGroups[i].textureType == VA_AnimationBook.TextureType.ColorMap)
                {
                    return textures[i].textureArrayIndex;
                }
            }

            return -1;
        }

        // Convert editor data into play data.
        // NOTE: Textures need to be assigned with stored ones on build.
        public static void ConvertEditorDataToPlayData(ref VA_AnimationBook book)
        {
            book.playData = new VA_AnimationBook.PlayData
            {
                maxFrames = book.editorData.maxFrames,
                materials = book.editorData.materials
            };

            foreach (var tg in book.editorData.textureGroups)
            {
                book.playData.textureGroups.Add(new VA_AnimationBook.PlayTextureGroup
                {
                    shaderParamName = tg.shaderParamName,
                    textureType = tg.textureType
                });
            }

            foreach (var ap in book.editorData.animationPages)
            {
                // NOTE: for some reason FixedString32 data gets lost when entering play mode.
                // That is why this is here... and also the animationPages...
                //book.playData.animations.Add(new VA_AnimationData
                //{
                //    name = ap.name,
                //    frames = ap.frames,
                //    maxFrames = book.editorData.maxFrames,
                //    frameTime = 1.0f / book.editorData.maxFrames,
                //    duration = 1.0f / book.editorData.maxFrames * ap.frames,
                //    animationMapIndex = GetFirstAnimationMapIndex(in ap.textures, in book.editorData.textureGroups),
                //    colorMapIndex = GetFirstColorMapIndex(in ap.textures, in book.editorData.textureGroups)
                //});

                var pap = new VA_AnimationBook.PlayAnimationPage
                {
                    name = ap.name,
                    frames = ap.frames,
                    textures = new List<VA_AnimationBook.PlayTextureEntry>()
                };

                foreach (var t in ap.textures)
                {
                    pap.textures.Add(new VA_AnimationBook.PlayTextureEntry 
                    {
                        textureArrayIndex = t.textureArrayIndex
                    });
                }

                book.playData.animationPages.Add(pap);
            }

            book.playData.texture2DArray = book.editorData.texture2DArray;
        }   
        #endregion
    }
}
