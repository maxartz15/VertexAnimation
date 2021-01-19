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

            // Texture Groups.
            DrawDefaultInspector();
            EditorGUILayoutUtils.HorizontalLine(color: Color.gray);

            serializedObject.ApplyModifiedProperties();
        }
    }
}