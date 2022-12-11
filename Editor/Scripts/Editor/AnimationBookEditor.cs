using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
    [CustomEditor(typeof(AnimationBook))]
    public class AnimationBookEditor : UnityEditor.Editor
    {
        private AnimationBook animationBook = null;

        void OnEnable()
        {
            animationBook = target as AnimationBook;
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