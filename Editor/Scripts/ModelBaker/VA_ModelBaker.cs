using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TAO.VertexAnimation.Editor
{
	[CreateAssetMenu(fileName = "new ModelBaker", menuName = "TAO/VertexAnimation/ModelBaker", order = 400)]
	public class VA_ModelBaker : ScriptableObject
	{
#if UNITY_EDITOR
		// Input.
		public GameObject model;
		public AnimationClip[] animationClips;
		[Range(1, 60)]
		public int fps = 24;
		public int textureWidth = 512;
	
		public LODSettings lodSettings = new LODSettings();
		public bool generateAnimationBook = true;
		public bool generatePrefab = true;
		public Shader materialShader = null;

		// Output.
		public GameObject prefab = null;
		public Texture2DArray positionMap = null;
		public Material material = null;
		public Mesh[] meshes = null;
		public VA_AnimationBook book = null;
		public List<VA_Animation> animations = new List<VA_Animation>();

		// TODO: release baked data from memory when done.
		[SerializeField]
		private AnimationBaker.BakedData bakedData;

		[System.Serializable]
		public class LODSettings
		{
			public LODSetting[] lodSettings = new LODSetting[3] { new LODSetting(1, .4f), new LODSetting(.6f, .15f), new LODSetting(.3f, .01f) };

			public float[] GetQualitySettings()
			{
				float[] q = new float[lodSettings.Length];

				for (int i = 0; i < lodSettings.Length; i++)
				{
					q[i] = lodSettings[i].quality;
				}

				return q;
			}

			public float[] GetTransitionSettings()
			{
				float[] t = new float[lodSettings.Length];

				for (int i = 0; i < lodSettings.Length; i++)
				{
					t[i] = lodSettings[i].screenRelativeTransitionHeight;
				}

				return t;
			}

			public int LODCount()
			{
				return lodSettings.Length;
			}
		}

		[System.Serializable]
		public struct LODSetting
		{
			[Range(1.0f, 0.0f)]
			public float quality;
			[Range(1.0f, 0.0f)]
			public float screenRelativeTransitionHeight;

			public LODSetting(float q, float t)
			{
				quality = q;
				screenRelativeTransitionHeight = t;
			}
		}

		public void Bake()
		{
			var target = Instantiate(model);
			target.name = model.name;

			target.ConbineAndConvertGameObject();
			bakedData = target.Bake(animationClips, fps, textureWidth);

			positionMap = VA_Texture2DArrayUtils.CreateTextureArray(bakedData.positionMaps.ToArray(), false, true, TextureWrapMode.Repeat, FilterMode.Point, 1, string.Format("{0}_PositionMap", name), true);
			meshes = bakedData.mesh.GenerateLOD(lodSettings.LODCount(), lodSettings.GetQualitySettings());

			DestroyImmediate(target);
		}

		public void SaveAssets()
		{
			AssetDatabaseUtils.RemoveChildAssets(this, new Object[2] { book, material });

			foreach (var m in meshes)
			{
				AssetDatabase.AddObjectToAsset(m, this);
			}

			AssetDatabase.AddObjectToAsset(positionMap, this);

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

		private void GeneratePrefab()
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

			material.SetTexture("_PositionMap", positionMap);
			material.SetInt("_MaxFrames", bakedData.maxFrames);

			// Generate Prefab
			prefab = AnimationPrefab.Create(path, name, meshes, material, lodSettings.GetTransitionSettings());
		}

		private void GenerateBook()
		{
			if (!book)
			{
				book = CreateInstance<VA_AnimationBook>();
			}

			book.name = string.Format("{0}_Book", name);
			book.positionMap = positionMap;
			book.TryAddMaterial(material);

			if (!AssetDatabaseUtils.HasChildAsset(this, book))
			{
				AssetDatabase.AddObjectToAsset(book, this);
			}

			// Add animations.
			List<NamingConventionUtils.TextureInfo> info = new List<NamingConventionUtils.TextureInfo>();

			foreach (var t in bakedData.positionMaps)
			{
				info.Add(t.name.GetTextureInfo());
			}

			for (int i = 0; i < info.Count; i++)
			{
				string animationName = string.Format("{0}_{1}", name, info[i].name);
				VA_AnimationData newData = new VA_AnimationData(animationName, info[i].frames, info[i].maxFrames, info[i].fps, i, -1);
				
				if (TryGetAnimationWithName(animationName, out VA_Animation animation))
				{
					animation.SetData(newData);
				}
				else
				{
					animation = CreateInstance<VA_Animation>();
					animation.name = animationName;
					animation.SetData(newData);
					animations.Add(animation);
				}

				book.TryAddAnimation(animation);

				if (!AssetDatabaseUtils.HasChildAsset(book, animation))
				{
					AssetDatabase.AddObjectToAsset(animation, book);
				}
			}

			// TODO: Remove unused animations.
		}

		private bool TryGetAnimationWithName(string name, out VA_Animation animation)
		{
			foreach (var a in animations)
			{
				if (a.name == name)
				{
					animation = a;
					return true;
				}
			}

			animation = null;
			return false;
		}
#endif
	}
}
