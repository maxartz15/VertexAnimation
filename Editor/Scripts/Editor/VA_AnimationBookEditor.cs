using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
    [CustomEditor(typeof(VA_AnimationBook))]
    public class VA_AnimationBookEditor : UnityEditor.Editor
    {
        private VA_AnimationBook animationBook = null;

        void OnEnable()
        {
            animationBook = target as VA_AnimationBook;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("positionMap"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animations"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("materials"));

			EditorGUILayoutUtils.HorizontalLine(color: Color.gray);

			using (new EditorGUI.DisabledScope(disabled: true))
            {
                EditorGUILayout.IntField(new GUIContent("MaxFrames"), animationBook.MaxFrames);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}