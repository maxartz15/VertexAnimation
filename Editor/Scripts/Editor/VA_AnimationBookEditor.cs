using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
    [CustomEditor(typeof(VA_AnimationBook))]
    public class VA_AnimationBookEditor : UnityEditor.Editor
    {
        private VA_AnimationBook animationBook = null;
        private Vector2 textureGroupScollPos;
        private Vector2 animationPagesScollPos;

        void OnEnable()
        {
            animationBook = target as VA_AnimationBook;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Texture Groups.
            GeneralGUI();
            EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
            TextureGroupsGUI();
            EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
            SyncListSize();
            AnimationPagesGUI();
            EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
            MaterialGUI();
            EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
            AssetBuilderGUI();
            EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
            Texture2DGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void SyncListSize()
        {
            foreach (var page in animationBook.editorData.animationPages)
            {
                if(page.textures.Count < animationBook.editorData.textureGroups.Count)
                {
                    int diff = animationBook.editorData.textureGroups.Count - page.textures.Count;

                    for (int i = 0; i < diff; i++)
                    {
                        page.textures.Add(null);
                    }
                }
                else if(page.textures.Count > animationBook.editorData.textureGroups.Count)
                {
                    int diff = page.textures.Count - animationBook.editorData.textureGroups.Count;

                    for (int i = 0; i < diff; i++)
                    {
                        page.textures.RemoveRange(page.textures.Count - diff, diff);
                    }
                }
            }
        }

        private void GeneralGUI()
        {
            SerializedProperty editorData = serializedObject.FindProperty("editorData");

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("General", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.PropertyField(editorData.FindPropertyRelative("maxFrames"));
            }
        }

        private void TextureGroupsGUI()
        {
            SerializedProperty editorData = serializedObject.FindProperty("editorData");
            SerializedProperty textureGroups = editorData.FindPropertyRelative("textureGroups");
            int removeWidth = 16;
            int nameWidth = 152;
            int optionWidth = 110;
            int linearWidth = 50;

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("TextureGroups", EditorStyles.centeredGreyMiniLabel);

                textureGroupScollPos = EditorGUILayout.BeginScrollView(textureGroupScollPos, false, false);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(removeWidth));
                    EditorGUILayout.LabelField("material parameter name", GUILayout.Width(nameWidth));
                    EditorGUILayout.LabelField("texture type", GUILayout.Width(optionWidth));
                    EditorGUILayout.LabelField("wrap mode", GUILayout.Width(optionWidth));
                    EditorGUILayout.LabelField("filter mode", GUILayout.Width(optionWidth));
                    EditorGUILayout.LabelField("is linear", GUILayout.MinWidth(linearWidth));
                }
                EditorGUILayout.EndScrollView();

                textureGroupScollPos = EditorGUILayout.BeginScrollView(textureGroupScollPos, false, false);
                for (int i = 0; i < textureGroups.arraySize; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("-", GUILayout.Width(removeWidth)))
                        {
                            textureGroups.DeleteArrayElementAtIndex(i);
                            continue;
                        }

                        EditorGUILayout.PropertyField(textureGroups.GetArrayElementAtIndex(i).FindPropertyRelative("shaderParamName"), GUIContent.none, GUILayout.Width(nameWidth));
                        EditorGUILayout.PropertyField(textureGroups.GetArrayElementAtIndex(i).FindPropertyRelative("textureType"), GUIContent.none, GUILayout.Width(optionWidth));
                        EditorGUILayout.PropertyField(textureGroups.GetArrayElementAtIndex(i).FindPropertyRelative("wrapMode"), GUIContent.none, GUILayout.Width(optionWidth));
                        EditorGUILayout.PropertyField(textureGroups.GetArrayElementAtIndex(i).FindPropertyRelative("filterMode"), GUIContent.none, GUILayout.Width(optionWidth));
                        EditorGUILayout.PropertyField(textureGroups.GetArrayElementAtIndex(i).FindPropertyRelative("isLinear"), GUIContent.none, GUILayout.MinWidth(linearWidth));
                    }
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    animationBook.editorData.textureGroups.Add(new VA_AnimationBook.EditorTextureGroup
                    {
                        shaderParamName = "_ShaderPropertyName",
                        isLinear = false
                    });
                }
            }
        }

        private void AnimationPagesGUI()
        {
            SerializedProperty editorData = serializedObject.FindProperty("editorData");
            SerializedProperty animationPages = editorData.FindPropertyRelative("animationPages");
            int removeWidth = 16;
            int nameWidth = 100;
            int frameWidth = 50;
            int textureWidth = 150;

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("AnimationPages", EditorStyles.centeredGreyMiniLabel);

                animationPagesScollPos = EditorGUILayout.BeginScrollView(animationPagesScollPos, false, false);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(removeWidth));
                    EditorGUILayout.LabelField("name", GUILayout.Width(nameWidth));
                    EditorGUILayout.LabelField("frames", GUILayout.Width(frameWidth));
                    foreach (var t in animationBook.editorData.textureGroups)
                    {
                        EditorGUILayout.LabelField(t.shaderParamName, GUILayout.MinWidth(textureWidth));
                    }
                }
                EditorGUILayout.EndScrollView();

                animationPagesScollPos = EditorGUILayout.BeginScrollView(animationPagesScollPos, false, false);
                for (int i = 0; i < animationPages.arraySize; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("-", GUILayout.Width(removeWidth)))
                        {
                            animationPages.DeleteArrayElementAtIndex(i);
                            continue;
                        }

                        EditorGUILayout.PropertyField(animationPages.GetArrayElementAtIndex(i).FindPropertyRelative("name"), GUIContent.none, GUILayout.Width(nameWidth));

                        EditorGUILayout.PropertyField(animationPages.GetArrayElementAtIndex(i).FindPropertyRelative("frames"), GUIContent.none, GUILayout.Width(frameWidth));

                        SerializedProperty textures = animationPages.GetArrayElementAtIndex(i).FindPropertyRelative("textures");
                        for (int t = 0; t < textures.arraySize; t++)
                        {
                            EditorGUILayout.PropertyField(textures.GetArrayElementAtIndex(t).FindPropertyRelative("texture2D"), GUIContent.none, GUILayout.MinWidth(textureWidth));
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    animationPages.InsertArrayElementAtIndex(animationPages.arraySize);
                }

                if (GUILayout.Button("auto fill", EditorStyles.miniButton))
                {
                    Undo.RecordObject(animationBook, "AutoFill");
                    VA_AssetBuilder.AutoFill(ref animationBook);
                    EditorUtility.SetDirty(animationBook);
                }
            }
        }

        private void MaterialGUI()
        {
            SerializedProperty editorData = serializedObject.FindProperty("editorData");

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Materials", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.PropertyField(editorData.FindPropertyRelative("materials"));
            }
        }

        private void AssetBuilderGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("build assets", EditorStyles.miniButtonLeft))
                {
                    VA_AssetBuilder.GenerateBuildData();
                }

                if (GUILayout.Button("clear assets", EditorStyles.miniButtonRight))
                {
                    VA_AssetBuilder.ClearBuildData();
                }
            }
        }

        private void Texture2DGUI()
        {
            SerializedProperty editorData = serializedObject.FindProperty("editorData");

            if (HasPreviewGUI())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    SerializedProperty texture2DArray = editorData.FindPropertyRelative("texture2DArray");

                    EditorGUILayout.LabelField("Texture2DArray", EditorStyles.centeredGreyMiniLabel);

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(texture2DArray);
                    }

                    //previewIndex = EditorGUILayout.IntSlider("Preview" ,previewIndex, 0, texture2DArray.arraySize - 1);
                }
            }
        }

        //public override bool HasPreviewGUI()
        //{
        //    bool hasPreview = false;

        //    if(animationBook.editorData.texture2DArray != null && animationBook.editorData.texture2DArray.Count > 0 && animationBook.editorData.texture2DArray[previewIndex] != null)
        //    {
        //        hasPreview = true;
        //    }

        //    return hasPreview;
        //}

        //public override void OnPreviewGUI(Rect r, GUIStyle background)
        //{
        //    if (previewEditor == null || curviewIndex != previewIndex)
        //    {
        //        curviewIndex = previewIndex;
        //        previewEditor = CreateEditor(animationBook.editorData.texture2DArray[previewIndex]);
        //    }

        //    previewEditor.OnInteractivePreviewGUI(r, background);
        //}
    }
}