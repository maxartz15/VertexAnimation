using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
    [CustomEditor(typeof(VA_ModelBaker))]
    public class VA_ModelBakerEditor : UnityEditor.Editor
    {
        private VA_ModelBaker modelBaker = null;

        void OnEnable()
        {
            modelBaker = target as VA_ModelBaker;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InputGUI();
            BakeGUI();
            OutputGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void InputGUI()
        {
            //public GameObject model;
            //public AnimationClip[] animationClips;
            //public int fps = 24;
            //public int textureWidth = 512;
            //public bool saveBakedDataToAsset = true;
            //public bool generateAnimationBook = false;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("model"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClips"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fps"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textureWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("saveBakedDataToAsset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generateAnimationBook"));
        }

        private void BakeGUI()
        {
            if (GUILayout.Button("Bake"))
            {
                ClearBakedData();

                modelBaker.Bake();

                if (modelBaker.saveBakedDataToAsset)
                {
                    SaveBakedData();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("SaveBakedData", EditorStyles.miniButtonLeft))
                {
                    SaveBakedData();
                }

                if (GUILayout.Button("ClearBakedData", EditorStyles.miniButtonRight))
                {
                    ClearBakedData();
                }
            }
        }

        private void OutputGUI()
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bakedData"));
            }
        }

        private void SaveBakedData()
        {
            AssetDatabase.AddObjectToAsset(modelBaker.BakedData.mesh, modelBaker);

            foreach (var pm in modelBaker.BakedData.positionMaps)
            {
                AssetDatabase.AddObjectToAsset(pm, modelBaker);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void ClearBakedData()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(modelBaker));

            foreach (var a in assets)
            {
                if (a != modelBaker)
                {
                    AssetDatabase.RemoveObjectFromAsset(a);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
