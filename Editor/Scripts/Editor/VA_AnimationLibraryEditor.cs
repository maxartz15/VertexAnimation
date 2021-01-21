using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
	[CustomEditor(typeof(VA_AnimationLibrary))]
	public class VA_AnimationLibraryEditor : UnityEditor.Editor
	{
		private VA_AnimationLibrary library = null;

		void OnEnable()
		{
			library = target as VA_AnimationLibrary;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			InputGUI();
			EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
			InfoGUI();

			serializedObject.ApplyModifiedProperties();
		}

		private void InputGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("animationBooks"));
		}

		private void InfoGUI()
		{
			if (GUILayout.Button("Refresh Preview"))
			{
				library.OnValidate();
			}

			using (new EditorGUI.DisabledScope(disabled: true))
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("loadedAnimationsPreview"));
			}
		}
	}
}
