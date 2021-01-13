using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
	public static class AnimationPrefab
	{
		public static GameObject Create(string path, string name, Mesh[] meshes, Material material, float[] lodTransitions)
		{
			// Create parent.
			GameObject parent = new GameObject(name, typeof(LODGroup), typeof(VA_AnimatorComponentAuthoring), typeof(Unity.Entities.ConvertToEntity));

			// Create all LODs.
			LOD[] lods = new LOD[meshes.Length];

			for (int i = 0; i < meshes.Length; i++)
			{
				GameObject lod = new GameObject(string.Format("{0}_LOD{1}", name, i), typeof(MeshFilter), typeof(MeshRenderer));
				
				var mf = lod.GetComponent<MeshFilter>();
				mf.sharedMesh = meshes[i];
				var mr = lod.GetComponent<MeshRenderer>();
				mr.sharedMaterial = material;

				lod.transform.SetParent(parent.transform);
				lods[i] = new LOD(lodTransitions[i], new Renderer[1] { mr });
			}

			var lodGroup = parent.GetComponent<LODGroup>();
			lodGroup.SetLODs(lods);
			lodGroup.RecalculateBounds();

			// Create prefab.
			GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(parent, path, InteractionMode.AutomatedAction);
			GameObject.DestroyImmediate(parent);

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