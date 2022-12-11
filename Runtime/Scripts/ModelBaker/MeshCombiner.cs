// References:
// https://forum.unity.com/threads/help-combining-and-manipulating-skinned-mesh-renderers-imported-from-blender.505078/
// http://wiki.unity3d.com/index.php/CombineSkinnedMeshes
// http://wiki.unity3d.com/index.php/SkinnedMeshCombiner

using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Rendering;

namespace TAO.VertexAnimation
{
public static class SkinnedMeshCombiner
{
	public static GameObject Combine(this SkinnedMeshRenderer target, List<SkinnedMeshRenderer> skinnedMeshRenderers, string name, Vector3 position, Quaternion rotation, Vector3 scale)
	{

		List<BoneWeight> boneWeights = new List<BoneWeight>();
		List<Transform> bones = new List<Transform>();
		List<CombineInstance> combineInstances = new List<CombineInstance>();
		Material sharedMaterial = skinnedMeshRenderers[0].sharedMaterial;
		Bounds newBounds = skinnedMeshRenderers[0].bounds;
		int num = 0;
		for( int i = 0; i < skinnedMeshRenderers.Count; ++i )
		{
			SkinnedMeshRenderer skinnedMeshRenderer = skinnedMeshRenderers[i];
			BoneWeight[] bws = skinnedMeshRenderer.sharedMesh.boneWeights;
			Transform[] bs = skinnedMeshRenderer.bones;

			for( int bwIndex = 0; bwIndex < bws.Length; ++bwIndex )
			{
				BoneWeight boneWeight = bws[bwIndex];
				boneWeight.boneIndex0 += num;
				boneWeight.boneIndex1 += num;
				boneWeight.boneIndex2 += num;
				boneWeight.boneIndex3 += num;

				boneWeights.Add( boneWeight );
			}
			num += bs.Length;

			for( int boneIndex = 0; boneIndex < bs.Length; ++boneIndex )
			{
				bones.Add( bs[boneIndex] );
			}

			CombineInstance combineInstance = new CombineInstance()
			{
				mesh = skinnedMeshRenderer.sharedMesh,
				transform = skinnedMeshRenderer.transform.localToWorldMatrix
			};
			combineInstances.Add( combineInstance );

			if ( i > 0 )
			{
				newBounds.Encapsulate( skinnedMeshRenderers[i].bounds );
			}
			//skinnedMeshRenderer.enabled = false;
		}

		List<Matrix4x4> bindposes = new List<Matrix4x4>();
		for( int i = 0; i < bones.Count; ++i )
		{
			Transform bone = bones[i];
			bindposes.Add( bone.worldToLocalMatrix * target.transform.worldToLocalMatrix );
			
		}

		SkinnedMeshRenderer combinedSkinnedMeshRenderer = target;
		combinedSkinnedMeshRenderer.updateWhenOffscreen = false;

		combinedSkinnedMeshRenderer.sharedMesh = new Mesh();
		combinedSkinnedMeshRenderer.sharedMesh.indexFormat = IndexFormat.UInt32;

		if ( combineInstances.Count == 1 )
		{
			combinedSkinnedMeshRenderer.sharedMesh = combineInstances[0].mesh;
		}
		else
		{
			combinedSkinnedMeshRenderer.sharedMesh.CombineMeshes( combineInstances.ToArray(), true, true );
		}

		foreach ( CombineInstance combineInstance in combineInstances )
		{
			combinedSkinnedMeshRenderer.sharedMesh.subMeshCount += combineInstance.mesh.subMeshCount;
		}

		combinedSkinnedMeshRenderer.sharedMaterials = new Material[combinedSkinnedMeshRenderer.sharedMesh.subMeshCount];
		for ( int i = 0; i < combinedSkinnedMeshRenderer.sharedMesh.subMeshCount; i++ )
		{
			combinedSkinnedMeshRenderer.sharedMaterials[i] = sharedMaterial;
		}
		//combinedSkinnedMeshRenderer.sharedMaterial = sharedMaterial;
		combinedSkinnedMeshRenderer.bones = bones.ToArray();
		combinedSkinnedMeshRenderer.sharedMesh.boneWeights = boneWeights.ToArray();
		combinedSkinnedMeshRenderer.sharedMesh.bindposes = bindposes.ToArray();
		combinedSkinnedMeshRenderer.sharedMesh.RecalculateBounds();
		//combinedSkinnedMeshRenderer.localBounds = new Bounds( new Vector3( 0.0f, 1.0f, 0.0f ), new Vector3( 0.5f, 1.0f, 0.5f ) );
		//AssetDatabase.CreateAsset( combinedSkinnedMeshRenderer.sharedMesh, $"Assets/CombinedSkinnedMeshRendererPrefabs/{name}(Mesh{System.DateTime.Now:MM_dd_yyyy-H_mm}).asset" );
		//AssetDatabase.SaveAssets();
		return target.gameObject;
	}
}
	public static class MeshCombiner
	{
		private struct MaterialMeshGroup
		{
			public List<SkinnedMeshRenderer> skinnedMeshes;
			public List<(MeshFilter mf, MeshRenderer mr)> meshes;
			public Material material;
		}

		public static SkinnedMeshRenderer Combine(this SkinnedMeshRenderer target, List<SkinnedMeshRenderer> skinnedMeshes, List<(MeshFilter mf, MeshRenderer mr)> meshes)
		{
			List<MaterialMeshGroup> groups = new List<MaterialMeshGroup>();

			// Group skinnedMeshes.
			foreach (var sm in skinnedMeshes)
			{
				bool hasGroup = false;
				foreach (var g in groups)
				{
					if (sm.sharedMaterial == g.material)
					{
						hasGroup = true;
						g.skinnedMeshes.Add(sm);
					}
				}

				if (!hasGroup)
				{
					groups.Add(new MaterialMeshGroup()
					{ 
						skinnedMeshes = new List<SkinnedMeshRenderer>()
						{
							sm
						},
						meshes = new List<(MeshFilter mf, MeshRenderer mr)>(),
						material = sm.sharedMaterial
					});
				}
			}

			// Group Meshes.
			foreach (var m in meshes)
			{
				bool hasGroup = false;
				foreach (var g in groups)
				{
					if (m.mr.sharedMaterial == g.material)
					{
						hasGroup = true;
						g.meshes.Add(m);
					}
				}

				if (!hasGroup)
				{
					groups.Add(new MaterialMeshGroup()
					{
						skinnedMeshes = new List<SkinnedMeshRenderer>(),
						meshes = new List<(MeshFilter mf, MeshRenderer mr)>()
						{
							m
						},
						material = m.mr.sharedMaterial
					});
				}
			}

			List<GameObject> tmp = new List<GameObject>();
			for (int i = 0; i < groups.Count; i++)
			{
				tmp.Add(new GameObject("tmpChild", typeof(SkinnedMeshRenderer)));
				tmp[i].transform.parent = target.transform;

				MaterialMeshGroup mmg = groups[i];
				tmp[i].GetComponent<SkinnedMeshRenderer>().Combine(mmg.skinnedMeshes, mmg.meshes, mmg.material);
			}

			// TODO: Merge materialMergedObjects.
			// TEMP: Remove when materialMergedObjects.
			SkinnedMeshRenderer newSkinnedMeshRenderer = tmp[0].GetComponent<SkinnedMeshRenderer>();
			target.sharedMesh = newSkinnedMeshRenderer.sharedMesh;
			target.sharedMaterial = newSkinnedMeshRenderer.sharedMaterial;
			target.bones = newSkinnedMeshRenderer.bones;

			foreach (var go in tmp)
			{
				GameObject.DestroyImmediate(go);
			}

			// Set a name to make it more clear.
			target.sharedMesh.name = target.transform.name.Replace("(Clone)", "");
			return target;
		}

		public static SkinnedMeshRenderer Combine(this SkinnedMeshRenderer target, List<SkinnedMeshRenderer> skinnedMeshes, List<(MeshFilter mf, MeshRenderer mr)> meshes, Material mainMaterial)
		{
			List<Transform> bones = new List<Transform>();
			List<BoneWeight> boneWeights = new List<BoneWeight>();
			List<Matrix4x4> bindPoses = new List<Matrix4x4>();
			List<CombineInstance> combineInstances = new List<CombineInstance>();

			// Combine SkinnedMeshes.
			int boneOffset = 0;
			for (int s = 0; s < skinnedMeshes.Count; s++)
			{
				SkinnedMeshRenderer smr = skinnedMeshes[s];

				//if the skinned mesh renderer has a material other than the default
				//we assume it's a one-off face material and deal with it later
				if (smr.sharedMaterial != mainMaterial)
				{
					continue;
				}

				BoneWeight[] meshBoneweight = smr.sharedMesh.boneWeights;

				// May want to modify this if the renderer shares bones as unnecessary bones will get added.
				// We don't care since it is going to be converted into vertex animations later anyways.
				for (int i = 0; i < meshBoneweight.Length; ++i)
				{
					BoneWeight bWeight = meshBoneweight[i];

					bWeight.boneIndex0 += boneOffset;
					bWeight.boneIndex1 += boneOffset;
					bWeight.boneIndex2 += boneOffset;
					bWeight.boneIndex3 += boneOffset;

					boneWeights.Add(bWeight);
				}

				boneOffset += smr.bones.Length;

				Transform[] meshBones = smr.bones;
				for (int i = 0; i < meshBones.Length; ++i)
				{
					bones.Add(meshBones[i]);

					//we take the old bind pose that mapped from our mesh to world to bone,
					//and take out our localToWorldMatrix, so now it's JUST the bone matrix
					//since our skinned mesh renderer is going to be on the root of our object that works
					bindPoses.Add(smr.sharedMesh.bindposes[i] * smr.transform.worldToLocalMatrix);
				}

				CombineInstance ci = new CombineInstance
				{
					mesh = smr.sharedMesh,
					transform = smr.transform.localToWorldMatrix
				};
				combineInstances.Add(ci);

				GameObject.DestroyImmediate(smr);
			}

			// Combine Meshes.
			for (int s = 0; meshes != null && s < meshes.Count; s++)
			{
				MeshFilter filter = meshes[s].mf;
				MeshRenderer renderer = meshes[s].mr;
			
				//if the skinned mesh renderer has a material other than the default
				//we assume it's a one-off face material and deal with it later
				if (renderer.sharedMaterial != mainMaterial)
				{
					continue;
				}

				// May want to modify this if the renderer shares bones as unnecessary bones will get added.
				// We don't care since it is going to be converted into vertex animations later anyways.
				int vertCount = filter.sharedMesh.vertexCount;
				for (int i = 0; i < vertCount; ++i)
				{
					BoneWeight bWeight = new BoneWeight
					{
						boneIndex0 = boneOffset,
						boneIndex1 = boneOffset,
						boneIndex2 = boneOffset,
						boneIndex3 = boneOffset,
						weight0 = 1
					};

					boneWeights.Add(bWeight);
				}

				boneOffset += 1;

				bones.Add(filter.transform);

				// TODO: figure out what this should be.
				bindPoses.Add(filter.transform.worldToLocalMatrix);

				CombineInstance ci = new CombineInstance
				{
					mesh = filter.sharedMesh,
					transform = filter.transform.localToWorldMatrix
				};
				combineInstances.Add(ci);

				GameObject.DestroyImmediate(filter);
				GameObject.DestroyImmediate(renderer);
			}

			// Actually combine and recalculate mesh.
			Mesh skinnedMesh = new Mesh();

			// Large mesh support.
			int vertexCount = 0;
			foreach (var ci in combineInstances)
			{
				vertexCount += ci.mesh.vertexCount;
			}

			if (vertexCount > 65535)
			{
				skinnedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			// Combine meshes.
			skinnedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
			skinnedMesh.RecalculateBounds();

			// Copy settings to target.
			target.sharedMesh = skinnedMesh;
			target.sharedMaterial = mainMaterial;
			target.bones = bones.ToArray();
			target.sharedMesh.boneWeights = boneWeights.ToArray();
			target.sharedMesh.bindposes = bindPoses.ToArray();

			return target;
		}

		public static void ConbineAndConvertGameObject(this GameObject gameObject, bool includeInactive = false)
		{
			// Get Skinned Meshes.
			List<SkinnedMeshRenderer> skinnedMeshes = new List<SkinnedMeshRenderer>();
			gameObject.GetComponentsInChildren(includeInactive, skinnedMeshes);

			List<MeshFilter> meshFilters = new List<MeshFilter>();
			gameObject.GetComponentsInChildren(includeInactive, meshFilters);

			// Get Meshes.
			List<(MeshFilter, MeshRenderer)> meshes = new List<(MeshFilter, MeshRenderer)>();
			foreach (var mf in meshFilters)
			{
				if (mf.TryGetComponent(out MeshRenderer mr))
				{
					if (includeInactive || (!includeInactive && mr.enabled))
					{
						meshes.Add((mf, mr));
					}
				}
			}

			// Add target mesh.
			SkinnedMeshRenderer target = gameObject.AddComponent<SkinnedMeshRenderer>();
			target.Combine(skinnedMeshes, gameObject.name, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.localScale);
		}
    }
}