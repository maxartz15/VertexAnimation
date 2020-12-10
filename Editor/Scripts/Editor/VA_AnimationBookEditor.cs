using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering.VirtualTexturing;
using System;

namespace TAO.VertexAnimation.Editor
{
    [CustomEditor(typeof(VA_AnimationBook))]
    public class VA_AnimationBookEditor : UnityEditor.Editor
    {
        private VA_AnimationBook animationBook = null;
        private Vector2 textireGroupScollPos;
        private Vector2 animationPagesScollPos;

        private UnityEditor.Editor previewEditor = null;
        private int previewIndex = 0;

        void OnEnable()
        {
            animationBook = target as VA_AnimationBook;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Texture Groups.
            GeneralGUI();
            TextureGroupsGUI();
            SyncListSize();
            AnimationPagesGUI();
            MaterialGUI();
            Texture2DGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void SyncListSize()
        {
            foreach (var page in animationBook.animationPages)
            {
                if(page.textures.Count < animationBook.textureGroups.Count)
                {
                    int diff = animationBook.textureGroups.Count - page.textures.Count;

                    for (int i = 0; i < diff; i++)
                    {
                        page.textures.Add(null);
                    }
                }
                else if(page.textures.Count > animationBook.textureGroups.Count)
                {
                    int diff = page.textures.Count - animationBook.textureGroups.Count;

                    for (int i = 0; i < diff; i++)
                    {
                        page.textures.RemoveRange(page.textures.Count - diff, diff);
                    }
                }
            }
        }

        private void GeneralGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("General", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFrames"));
            }
        }

        private void TextureGroupsGUI()
        {
            SerializedProperty textureGroups = serializedObject.FindProperty("textureGroups");
            int removeWidth = 16;
            int nameWidth = 152;
            int linearWidth = 50;

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("TextureGroups", EditorStyles.centeredGreyMiniLabel);

                textireGroupScollPos = EditorGUILayout.BeginScrollView(textireGroupScollPos, false, false);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(removeWidth));
                    EditorGUILayout.LabelField("material parameter name", GUILayout.Width(nameWidth));
                    EditorGUILayout.LabelField("is linear", GUILayout.MinWidth(linearWidth));
                }
                EditorGUILayout.EndScrollView();

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
                        EditorGUILayout.PropertyField(textureGroups.GetArrayElementAtIndex(i).FindPropertyRelative("isLinear"), GUIContent.none, GUILayout.MinWidth(linearWidth));
                    }
                }

                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    animationBook.textureGroups.Add(new TextureGroup
                    {
                        shaderParamName = "_ShaderPropertyName",
                        isLinear = false
                    });
                }
            }
        }

        private void AnimationPagesGUI()
        {
            SerializedProperty animationPages = serializedObject.FindProperty("animationPages");
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
                    foreach (var t in animationBook.textureGroups)
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
                            EditorGUILayout.PropertyField(textures.GetArrayElementAtIndex(t), GUIContent.none, GUILayout.MinWidth(textureWidth));
                        }
                    }
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("+", EditorStyles.miniButton))
                {
                    animationPages.InsertArrayElementAtIndex(animationPages.arraySize);
                }
            }
        }

        private void MaterialGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Materials", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("materials"));
            }
        }

        private void Texture2DGUI()
        {
            if (HasPreviewGUI())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    SerializedProperty texture2DArray = serializedObject.FindProperty("texture2DArray");

                    EditorGUILayout.LabelField("Texture2DArray", EditorStyles.centeredGreyMiniLabel);
                    previewIndex = EditorGUILayout.IntSlider(previewIndex, 0, texture2DArray.arraySize - 1);
                }
            }
        }

        public override bool HasPreviewGUI()
        {
            bool hasPreview = false;

            if(animationBook.texture2DArray != null && animationBook.texture2DArray.Count > 0 && animationBook.texture2DArray[previewIndex] != null)
            {
                hasPreview = true;
            }

            return hasPreview;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (previewEditor == null)
            {
                previewEditor = UnityEditor.Editor.CreateEditor(animationBook.texture2DArray[previewIndex]);
            }

            previewEditor.OnInteractivePreviewGUI(r, background);
        }
    }
}