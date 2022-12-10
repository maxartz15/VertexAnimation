using System.IO;
using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
	public static class AnimationPrefab
	{
		public static GameObject Create(string path, string name, Mesh[] meshes, Material material, float[] lodTransitions)
		{
			GameObject parent = null;
			if (AssetDatabaseUtils.HasAsset(path, typeof(GameObject)))
			{
				// Load existing parent.
				parent = PrefabUtility.LoadPrefabContents(path);

				// Check setup.
				if (!parent.TryGetComponent(out LODGroup _))
				{
					parent.AddComponent<LODGroup>();
				}

				if (!parent.TryGetComponent(out AnimationLibraryComponentAuthoring _))
				{
					parent.AddComponent<AnimationLibraryComponentAuthoring>();
				}

				//if (!parent.TryGetComponent(out Unity.Entities.ConvertToEntity _))
				//{
				//	parent.AddComponent<Unity.Entities.ConvertToEntity>();
				//}
			}
			else
			{
				// Create parent.
				parent = new GameObject(name, typeof(LODGroup), typeof(AnimationLibraryComponentAuthoring));
			}

			// Create all LODs.
			LOD[] lods = new LOD[meshes.Length];

			//string meshPath = "Assets/Mesh" + parent.name;
			//int index = 0;
			//foreach ( Mesh mesh in meshes )
			//{
			//	if ( !AssetDatabaseUtils.HasAsset( meshPath + index + ".asset", typeof( Mesh ) ) )
			//	{
			//		AssetDatabase.CreateAsset( mesh, meshPath + index + ".asset" );
			//	}
//
			//	index++;
			//}
			
			AssetDatabase.SaveAssets();
			for (int i = 0; i < meshes.Length; i++)
			{
				string childName = string.Format("{0}_LOD{1}", name, i);

				GameObject child;
				{
					Transform t = parent.transform.Find(childName);
					if (t)
					{
						child = t.gameObject;
					}
					else
					{
						child = new GameObject(childName, typeof(MeshFilter), typeof(MeshRenderer), typeof(AnimationDataComponentAuthoring));
					}
				}

				if (!child.TryGetComponent(out AnimationDataComponentAuthoring ad))
				{
					child.AddComponent<AnimationDataComponentAuthoring>();
				}
				
				if (child.TryGetComponent(out MeshFilter mf))
				{
					mf.sharedMesh = meshes[i];
				}

				if (child.TryGetComponent(out MeshRenderer mr))
				{
					mr.sharedMaterial = material;
				}

				child.transform.SetParent(parent.transform);
				lods[i] = new LOD(lodTransitions[i], new Renderer[1] { mr });
			}

			var lodGroup = parent.GetComponent<LODGroup>();
			lodGroup.SetLODs(lods);
			lodGroup.RecalculateBounds();

			// Create prefab.
			GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(parent, path, InteractionMode.AutomatedAction);
			//GameObject.DestroyImmediate(parent);

			return prefab;
		}

		public static GameObject Create(string path, string name, Mesh[] meshes, Material material, AnimationCurve lodTransitions)
		{
			float[] lt = new float[meshes.Length];

			for (int i = 0; i < lt.Length; i++)
			{
				lt[i] = lodTransitions.Evaluate((1.0f / lt.Length) * (i + 1));
			}

			return Create(path, name, meshes, material, lt);
		}
	}
}