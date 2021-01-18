using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

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
			EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
			BakeGUI();

			serializedObject.ApplyModifiedProperties();

			EditorGUILayoutUtils.HorizontalLine(color: Color.gray);
			OutputGUI();
		}

		private void InputGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("model"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("animationClips"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("fps"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("textureWidth"));
		}

		private void BakeGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty("saveBakedDataToAsset"));

			int il = EditorGUI.indentLevel;
			if (modelBaker.saveBakedDataToAsset)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("generateAnimationBook"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("lodSettings"));

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty("generatePrefab"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("materialShader"), new GUIContent(""));
				}
			}
			EditorGUI.indentLevel = il;

			if (GUILayout.Button("Bake", GUILayout.Height(32)))
			{
				modelBaker.Bake();

				if (modelBaker.saveBakedDataToAsset)
				{
					modelBaker.SaveAssets();
				}
			}

			if (GUILayout.Button("Delete", EditorStyles.miniButtonRight))
			{
				if (EditorUtility.DisplayDialog("Delete Assets", "Deleting assets will loose references within the project.", "Ok", "Cancel"))
				{
					modelBaker.DeleteSavedAssets();
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
	}
}