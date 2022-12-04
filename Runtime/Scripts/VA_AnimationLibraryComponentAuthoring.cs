using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace TAO.VertexAnimation
{
[UnityEngine.DisallowMultipleComponent]
	public class VA_AnimationLibraryComponentAuthoring : UnityEngine.MonoBehaviour
	{
		public VA_AnimationLibrary AnimationLibrary;
		public bool DebugMode = false;
	}
	
internal struct SkinnedMeshEntity : IBufferElementData
{
	public Entity Value;
}

public struct VA_AnimationLibraryComponent : IComponentData
{
	public BlobAssetReference<VA_AnimationLibraryData> AnimLibAssetRef;
	public BlobAssetStore BlobAssetStore;
}

public class VA_AnimationLibraryComponentBaker : Baker < VA_AnimationLibraryComponentAuthoring >
{
	public override void Bake( VA_AnimationLibraryComponentAuthoring authoring )
	{
		authoring.AnimationLibrary.Init();
		VA_AnimationLibraryComponent animationLibrary = new VA_AnimationLibraryComponent();
		using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
		{
			// Construct the root.
			ref VA_AnimationLibraryData animationDataBlobAsset = ref blobBuilder.ConstructRoot<VA_AnimationLibraryData>();

			// Set all the data.
			BlobBuilderArray<VA_AnimationData> animationDataArray = blobBuilder.Allocate(ref animationDataBlobAsset.animations, authoring.AnimationLibrary.animationData.Count);

			for (int i = 0; i < animationDataArray.Length; i++)
			{
				// Copy data.
				animationDataArray[i] = authoring.AnimationLibrary.animationData[i];

				if (authoring.DebugMode)
				{
					UnityEngine.Debug.Log("VA_AnimationLibrary added " + animationDataArray[i].name.ToString());
				}
			}

			// Construct blob asset reference.
			//BlobAssetReference<VA_AnimationLibraryData> animLibAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationLibraryData>(Allocator.Persistent);
			// Static because of multi scene setup.
			animationLibrary.AnimLibAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationLibraryData>(Allocator.Persistent);
			Hash128 hash128 = new Hash128( VA_AnimationLibraryUtils.AnimationLibraryAssetStoreName );
			// Add it to the asset store.
			animationLibrary.BlobAssetStore = new BlobAssetStore( 50);
			animationLibrary.BlobAssetStore.TryAdd(hash128, ref animationLibrary.AnimLibAssetRef);

			if (authoring.DebugMode)
			{
				UnityEngine.Debug.Log("VA_AnimationLibrary has " + animationLibrary.AnimLibAssetRef.Value.animations.Length.ToString() + " animations.");
			}
		}
		AddComponent( animationLibrary );
		
		BlobAssetReference<VA_AnimationLibraryData> animLib = animationLibrary.AnimLibAssetRef;

		// Add animator to 'parent'.
		VA_AnimatorComponent animatorComponent = new VA_AnimatorComponent
		{
			animationIndex = 0,
			animationIndexNext = -1,
			animationTime = 0,
			animationLibrary = animLib
		};
		AddComponent(animatorComponent);
		var boneEntityArray = AddBuffer<SkinnedMeshEntity>();

		MeshRenderer[] skinnedMeshRenderers =
			authoring.transform.GetComponentsInChildren < MeshRenderer >();
		boneEntityArray.ResizeUninitialized(skinnedMeshRenderers.Length);

		for (int boneIndex = 0; boneIndex < skinnedMeshRenderers.Length; ++boneIndex)
		{
			var boneEntity = GetEntity(skinnedMeshRenderers[boneIndex]);
			boneEntityArray[boneIndex] = new SkinnedMeshEntity {Value = boneEntity};
		}
	}
}

//[GenerateAuthoringComponent]
public struct VA_AnimatorComponent : IComponentData
{
	public int animationIndex;
	public int animationIndexNext;
	public float animationTime;
	public BlobAssetReference<VA_AnimationLibraryData> animationLibrary;
}
}