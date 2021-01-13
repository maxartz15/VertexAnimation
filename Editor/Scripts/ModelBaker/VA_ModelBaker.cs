using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
	[CreateAssetMenu(fileName = "new ModelBaker", menuName = "VA_ModelBaker/ModelBaker", order = 400)]
	public class VA_ModelBaker : ScriptableObject
	{
#if UNITY_EDITOR
		// Input.
		public GameObject model;
		public AnimationClip[] animationClips;
		[Range(1, 60)]
		public int fps = 24;
		public int textureWidth = 512;

		public bool generateLODS = true;
		public AnimationCurve lodCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0.01f));
		public bool saveBakedDataToAsset = true;
		public bool generateAnimationBook = true;
		public bool generatePrefab = true;
		public Shader materialShader = null;

		// Output.
		public GameObject prefab = null;
		public Material material = null;
		public Mesh[] meshes = null;
		public VA_AnimationBook book = null;

		[SerializeField]
		private AnimationBaker.BakedData bakedData;

		public void Bake()
		{
			var target = Instantiate(model);
			target.name = model.name;

			target.ConbineAndConvertGameObject();
			bakedData = target.Bake(animationClips, fps, textureWidth);

			if (generateLODS)
			{
				// TODO: LODs.
				meshes = new Mesh[1] { bakedData.mesh };
			}
			else
			{
				meshes = new Mesh[1] { bakedData.mesh };
			}

			DestroyImmediate(target);
		}

		public void SaveAssets()
		{
			AssetDatabaseUtils.RemoveChildAssets(this, new Object[2] { book, material });

			// TODO: LODs
			AssetDatabase.AddObjectToAsset(bakedData.mesh, this);

			foreach (var pm in bakedData.positionMaps)
			{
				AssetDatabase.AddObjectToAsset(pm, this);
			}

			AssetDatabase.SaveAssets();

			if (generatePrefab)
			{
				GeneratePrefab();
			}

			if (generateAnimationBook)
			{
				GenerateBook();
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public void DeleteSavedAssets()
		{
			// Remove assets.
			var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
			foreach (var a in assets)
			{
				if (a != this)
				{
					AssetDatabase.RemoveObjectFromAsset(a);
				}
			}

			// Delete prefab.
			string path = AssetDatabase.GetAssetPath(prefab);
			AssetDatabase.DeleteAsset(path);

			// Clear variables.
			prefab = null;
			material = null;
			meshes = null;
			book = null;

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public void GeneratePrefab()
		{
			string path = AssetDatabase.GetAssetPath(this);
			int start = path.LastIndexOf('/');
			path = path.Remove(start, path.Length - start);
			path += "/" + name + ".prefab";

			// Generate Material
			if (!AssetDatabaseUtils.HasChildAsset(this, material))
			{
				material = AnimationMaterial.Create(name, materialShader);
				AssetDatabase.AddObjectToAsset(material, this);
			}
			else
			{
				material.shader = materialShader;
			}

			// Generate Prefab
			prefab = AnimationPrefab.Create(path, name, meshes, material, lodCurve);
		}

		public void GenerateBook()
		{
			if (!book)
			{
				book = CreateInstance<VA_AnimationBook>();
			}

			book.name = string.Format("{0}Book", name);
			book.editorData = new VA_AnimationBook.EditorData
			{
				materials = new Material[1] { material }
			};

			foreach (Texture2D tex in bakedData.positionMaps)
			{
				book.editorData.animationPages.Add(new VA_AnimationBook.EditorAnimationPage
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

			VA_AssetBuilder.AutoFill(ref book);

			if (!AssetDatabaseUtils.HasChildAsset(this, book))
			{
				AssetDatabase.AddObjectToAsset(book, this);
			}
		}
#endif
	}
}
