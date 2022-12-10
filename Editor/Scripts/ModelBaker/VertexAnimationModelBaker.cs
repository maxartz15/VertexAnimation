using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.Rendering;

namespace TAO.VertexAnimation.Editor
{
	[CreateAssetMenu(fileName = "new ModelBaker", menuName = "TAO/VertexAnimation/ModelBaker", order = 400)]
	public class VertexAnimationModelBaker : ScriptableObject
	{
#if UNITY_EDITOR
		// Input.
		public GameObject model;
		public AnimationClip[] animationClips;
		[Range(1, 60)]
		public int fps = 24;
		public int textureWidth = 512;
		public bool applyRootMotion = false;
		public bool includeInactive = false;
	
		public LODSettings lodSettings = new LODSettings();
		public bool applyAnimationBounds = true;
		public bool generateAnimationBook = true;
		public bool generatePrefab = true;
		public Shader materialShader = null;
		public bool useInterpolation = true;
		public bool useNormalA = true;

		// Output.
		public GameObject prefab = null;
		public Texture2DArray positionMap = null;
		public Material material = null;
		public Mesh[] meshes = null;
		public AnimationBook book = null;
		public List<Animation> animations = new List<Animation>();

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

		private void OnValidate()
		{
			if (materialShader == null)
			{
				materialShader = Shader.Find("TAO/Lit");
			}
		}

		public void Bake()
		{
			var target = Instantiate(model);
			target.name = model.name;

			target.ConbineAndConvertGameObject(includeInactive);
			AnimationBaker.BakedData bakedData = target.Bake(animationClips, applyRootMotion, fps, textureWidth);

			positionMap = Texture2DArrayUtils.CreateTextureArray(bakedData.positionMaps.ToArray(), false, true, TextureWrapMode.Repeat, FilterMode.Point, 1, string.Format("{0}_PositionMap", name), true);
			meshes = bakedData.mesh.GenerateLOD(lodSettings.LODCount(), lodSettings.GetQualitySettings());

			DestroyImmediate(target);

			SaveAssets(bakedData);
		}

		private void SaveAssets(AnimationBaker.BakedData bakedData)
		{
			AssetDatabaseUtils.RemoveChildAssets(this, new Object[2] { book, material });

			Bounds bounds = new Bounds
			{
				max = bakedData.maxBounds,
				min = bakedData.minBounds
			};

			for (int i = 0; i < meshes.Length; i++)
			{
				if (applyAnimationBounds)
				{
					meshes[i].bounds = bounds;
				}

				meshes[i].Finalize();
				SerializedObject s = new SerializedObject(meshes[i]);
				s.FindProperty("m_IsReadable").boolValue = true;
				AssetDatabase.AddObjectToAsset(s.targetObject, this);
			}

			AssetDatabase.AddObjectToAsset(positionMap, this);
			AssetDatabase.SaveAssets();

			if (generatePrefab)
			{
				GeneratePrefab(bakedData);
			}

			if (generateAnimationBook)
			{
				GenerateBook(bakedData);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void GeneratePrefab(AnimationBaker.BakedData bakedData)
		{
			string path = AssetDatabase.GetAssetPath(this);
			int start = path.LastIndexOf('/');
			path = path.Remove(start, path.Length - start);
			path += "/" + name + ".prefab";

			// Get info.
			NamingConventionUtils.PositionMapInfo info = bakedData.GetPositionMap.name.GetTextureInfo();

			//bakedData.mesh.SetTriangles( bakedData.mesh.triangles, 0 );
			//meshes = new[] { bakedData.mesh };
			
			// Generate Material
			if (!AssetDatabaseUtils.HasChildAsset(this, material))
			{
				material = AnimationMaterial.Create(name, materialShader, positionMap, useNormalA, useInterpolation, info.maxFrames);
				AssetDatabase.AddObjectToAsset(material, this);
			}
			else
			{
				material.Update(name, materialShader, positionMap, useNormalA, useInterpolation, info.maxFrames);
			}

			// Generate Prefab
			prefab = AnimationPrefab.Create(path, name, meshes, material, lodSettings.GetTransitionSettings());
		}

		private void GenerateBook(AnimationBaker.BakedData bakedData)
		{
			// Create book.
			if (!book)
			{
				book = CreateInstance<AnimationBook>();
			}

			book.name = string.Format("{0}_Book", name);
			book.positionMap = positionMap;
			book.animations = new List<Animation>();
			book.TryAddMaterial(material);

			// Save book.
			if (!AssetDatabaseUtils.HasChildAsset(this, book))
			{
				AssetDatabase.AddObjectToAsset(book, this);
			}

			// Get animation info.
			List<NamingConventionUtils.PositionMapInfo> info = new List<NamingConventionUtils.PositionMapInfo>();
			foreach (var t in bakedData.positionMaps)
			{
				info.Add(t.name.GetTextureInfo());
			}

			// Create animations.
			for (int i = 0; i < info.Count; i++)
			{
				string animationName = string.Format("{0}_{1}", name, info[i].name);
				VA_AnimationData newData = new VA_AnimationData(animationName, info[i].frames, info[i].maxFrames, info[i].fps, i, -1);

				// Either update existing animation or create a new one.
				if (TryGetAnimationWithName(animationName, out Animation animation))
				{
					animation.SetData(newData);
				}
				else
				{
					animation = CreateInstance<Animation>();
					animation.name = animationName;
					animation.SetData(newData);
					animations.Add(animation);
				}

				book.TryAddAnimation(animation);
			}

			// Save animation objects.
			foreach (var a in animations)
			{
				AssetDatabaseUtils.TryAddChildAsset(book, a);
			}
		}

		private bool TryGetAnimationWithName(string name, out Animation animation)
		{
			foreach (var a in animations)
			{
				if (a != null)
				{
					if (a.name == name)
					{
						animation = a;
						return true;
					}
				}
			}

			animation = null;
			return false;
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
			positionMap = null;
			material = null;
			meshes = null;
			book = null;
			animations = new List<Animation>();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		public void DeleteUnusedAnimations()
		{
			if (book != null)
			{
				// Remove unused animations.
				for (int i = 0; i < animations.Count; i++)
				{
					if (!book.animations.Contains(animations[i]))
					{
						AssetDatabase.RemoveObjectFromAsset(animations[i]);
						animations[i] = null;
					}
				}

				// Remove zero entries.
				animations.RemoveAll(a => a == null);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
#endif
	}
}
