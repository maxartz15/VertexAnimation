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

				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty("generatePrefab"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("materialShader"), new GUIContent(""));
				}
			}
			EditorGUI.indentLevel = il;

			if (GUILayout.Button("Bake", GUILayout.Height(32)))
			{
				ClearBakedData();

				modelBaker.Bake();

				if (modelBaker.saveBakedDataToAsset)
				{
					SaveBakedData();
				}
			}

			if (modelBaker.BakedData.mesh != null)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Save", EditorStyles.miniButtonLeft))
					{
						SaveBakedData();
					}

					if (GUILayout.Button("Clear", EditorStyles.miniButtonRight))
					{
						ClearBakedData();
					}
				}

				if (modelBaker.prefab && GUILayout.Button("Remove Prefab"))
				{
					DeletePrefab();
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
			ClearBakedData();

			AssetDatabase.AddObjectToAsset(modelBaker.BakedData.mesh, modelBaker);

			foreach (var pm in modelBaker.BakedData.positionMaps)
			{
				AssetDatabase.AddObjectToAsset(pm, modelBaker);
			}

			AssetDatabase.SaveAssets();

			if (modelBaker.generatePrefab)
			{
				GeneratePrefab();
			}

			if(modelBaker.generateAnimationBook)
			{
				GenerateBook();
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

		private void DeletePrefab()
		{
			string path = AssetDatabase.GetAssetPath(modelBaker.prefab);
			AssetDatabase.DeleteAsset(path);
			AssetDatabase.Refresh();
		}

		private void GeneratePrefab()
		{
			string path = AssetDatabase.GetAssetPath(modelBaker);
			int start = path.LastIndexOf('/');
			path = path.Remove(start, path.Length - start);
			path += "/" + modelBaker.name + ".prefab";

			// Generate Material
			modelBaker.material = new Material(modelBaker.materialShader);
			modelBaker.material.name = modelBaker.name;
			AssetDatabase.AddObjectToAsset(modelBaker.material, modelBaker);

			// Generate Object.
			if (!modelBaker.prefab)
			{
				GameObject go = new GameObject(modelBaker.model.name, typeof(MeshFilter), typeof(MeshRenderer));
				modelBaker.prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
				DestroyImmediate(go);
			}

			GameObject inst = PrefabUtility.InstantiatePrefab(modelBaker.prefab) as GameObject;

			inst.GetComponent<MeshFilter>().sharedMesh = modelBaker.BakedData.mesh;
			inst.GetComponent<MeshRenderer>().sharedMaterial = modelBaker.material;

			// Save.
			PrefabUtility.ApplyPrefabInstance(inst, InteractionMode.UserAction);
			AssetDatabase.SaveAssets();

			DestroyImmediate(inst);
		}

		private void GenerateBook()
		{
			if (!modelBaker.book)
			{
				modelBaker.book = CreateInstance<VA_AnimationBook>();
			}

			modelBaker.book.name = modelBaker.model.name;
			modelBaker.book.editorData = new VA_AnimationBook.EditorData();

			modelBaker.book.editorData.materials = new Material[1] { modelBaker.material };

			foreach (Texture2D tex in modelBaker.BakedData.positionMaps)
			{
				modelBaker.book.editorData.animationPages.Add(new VA_AnimationBook.EditorAnimationPage
				{
					name = "",
					frames = 0,
					textures = new List<VA_AnimationBook.EditorTextureEntry>()
					{
						new VA_AnimationBook.EditorTextureEntry
						{
							texture2D = tex
						}
					}
				});
			}

			VA_AssetBuilder.AutoFill(ref modelBaker.book);

			AssetDatabase.AddObjectToAsset(modelBaker.book, modelBaker);
			AssetDatabase.SaveAssets();
		}
	}
}