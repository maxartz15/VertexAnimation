using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;
using Random = Unity.Mathematics.Random;

namespace TAO.VertexAnimation
{
[UnityEngine.DisallowMultipleComponent]
	public class AnimationLibraryComponentAuthoring : UnityEngine.MonoBehaviour
	{
		public AnimationLibrary AnimationLibrary;
		public bool DebugMode = false;
		public uint Seed;
	}

public struct AnimatorWaitingForBaking : IComponentData
{
	public bool IsInitialized;
	public Entity AnimatorEntity;
}

public struct AnimationLibraryComponent : IComponentData
{
	public BlobAssetReference<VA_AnimationLibraryData> AnimLibAssetRef;
	public BlobAssetStore BlobAssetStore;
}

public class AnimationLibraryComponentBaker : Baker < AnimationLibraryComponentAuthoring >
{
	public override void Bake( AnimationLibraryComponentAuthoring authoring )
	{
		authoring.AnimationLibrary.Init();
		AnimationLibraryComponent animationLibrary = new AnimationLibraryComponent();
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
		// Get the animation lib data.
		ref VA_AnimationLibraryData animationsRef = ref animLib.Value;
		Random random = new Random( authoring.Seed != 0 ? authoring.Seed : 42 );
		int index = random.NextInt( 20 );

		// Add animator to 'parent'.
		AnimatorComponent animatorComponent = new AnimatorComponent
		{
			Enabled = true,
			AnimationName = animationsRef.animations[5].name,
			AnimationIndex = 2,
			AnimationIndexNext = -1,
			AnimationTime = 0,
			AnimationLibrary = animLib

		};
		AddComponent(animatorComponent);

		AnimatorBlendStateComponent animatorStateComponent = new AnimatorBlendStateComponent
		{
			BlendingEnabled = true,
			ToAnimationIndex = 1,
			BlendingCurrentTime = 0.0f,
			AnimationBlendingCurveIndex = 2,
			BlendingDuration = 2.5f,
			AnimationTime = 0.0f,
		};

		AddComponent( animatorStateComponent );
		AddComponent<AnimatorWaitingForBaking>(new AnimatorWaitingForBaking{ AnimatorEntity = GetEntity(), IsInitialized = false});
	}
}


//[GenerateAuthoringComponent]
public struct AnimatorComponent : IComponentData
{
	public bool Enabled;
	public FixedString64Bytes AnimationName;
	public int AnimationIndex;
	public int AnimationIndexNext;
	public float AnimationTime;
	public BlobAssetReference<VA_AnimationLibraryData> AnimationLibrary;
	public NativeArray < Entity > SkinnedMeshes;
}

public struct AnimatorBlendStateComponent : IComponentData
{
	public bool BlendingEnabled;
	public float AnimationTime;
	public int ToAnimationIndex;
	public float BlendingDuration;
	public float BlendingCurrentTime;
	public int AnimationBlendingCurveIndex;
}

}