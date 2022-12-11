using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
	[CustomEditor(typeof(AnimationLibrary))]
	public class AnimationLibraryEditor : UnityEditor.Editor
	{
		private AnimationLibrary library = null;

		void OnEnable()
		{
			library = target as AnimationLibrary;
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
