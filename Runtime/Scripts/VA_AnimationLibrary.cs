using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation
{
	[System.Serializable]
	public struct VA_AnimationData
	{
		public int frames;
		public int maxFrames;
	}
	
	public struct VA_AnimationDataBlobAsset
	{
		public BlobArray<VA_AnimationData> animations;
	}

	public class VA_AnimationDataBlobAssetConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			BlobAssetReference<VA_AnimationDataBlobAsset> animationDataBlobAssetRef;

			// Blob builder to build.
			using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
			{
				// Construct the root.
				ref VA_AnimationDataBlobAsset animationDataBlobAsset = ref blobBuilder.ConstructRoot<VA_AnimationDataBlobAsset>();

				// Set all the data.
				BlobBuilderArray<VA_AnimationData> animationDataArray = blobBuilder.Allocate(ref animationDataBlobAsset.animations, 2);
				for (int i = 0; i < animationDataArray.Length; i++)
				{
					animationDataArray[i] = new VA_AnimationData
					{ 
						frames = 36,
						maxFrames = 43
					};
				}

				// Construct blob asset reference.
				animationDataBlobAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationDataBlobAsset>(Allocator.Persistent);

				UnityEngine.Debug.Log("Created: " + animationDataBlobAssetRef.Value.animations.Length.ToString());
			}

			// TODO: Generate Hash based on Guid.
			BlobAssetStore.TryAdd(new Hash128("AnimationLib"), animationDataBlobAssetRef);
		}
	}
}