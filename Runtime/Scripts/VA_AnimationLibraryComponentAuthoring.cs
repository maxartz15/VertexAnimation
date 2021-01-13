using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation
{
	[UnityEngine.RequireComponent(typeof(ConvertToEntity))]
	[UnityEngine.DisallowMultipleComponent]
	public class VA_AnimationLibraryComponentAuthoring : UnityEngine.MonoBehaviour
	{
		public VA_AnimationLibrary animationLibrary;
	}
	
	public class VA_AnimationLibraryConversionSystem : GameObjectConversionSystem
	{
		// Static because of multi scene setup.
		public static BlobAssetReference<VA_AnimationLibraryData> animLibAssetRef;

		protected override void OnUpdate()
		{
			Entities.ForEach((VA_AnimationLibraryComponentAuthoring animationLib) =>
			{
				animationLib.animationLibrary.Init();

				// Blob builder to build.
				using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
				{
					// Construct the root.
					ref VA_AnimationLibraryData animationDataBlobAsset = ref blobBuilder.ConstructRoot<VA_AnimationLibraryData>();

					// Set all the data.
					BlobBuilderArray<VA_AnimationData> animationDataArray = blobBuilder.Allocate(ref animationDataBlobAsset.animations, animationLib.animationLibrary.animations.Count);

                    for (int i = 0; i < animationDataArray.Length; i++)
					{
						// Copy data.
						animationDataArray[i] = animationLib.animationLibrary.animations[i];
					}

					// Construct blob asset reference.
					//BlobAssetReference<VA_AnimationLibraryData> animLibAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationLibraryData>(Allocator.Persistent);
					// Static because of multi scene setup.
					animLibAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationLibraryData>(Allocator.Persistent);

					// Add it to the asset store.
					BlobAssetStore.TryAdd(new Hash128(VA_AnimationLibraryUtils.AnimationLibraryAssetStoreName), animLibAssetRef);

					UnityEngine.Debug.Log("VA_AnimationLibrary has " + animLibAssetRef.Value.animations.Length.ToString() + " animations.");
				}

				// Remove the entity since we don't need it anymore.
				DstEntityManager.DestroyEntity(GetPrimaryEntity(animationLib));
			});
		}
	}
}