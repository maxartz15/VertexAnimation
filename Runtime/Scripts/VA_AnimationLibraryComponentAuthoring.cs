using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation
{
	[UnityEngine.RequireComponent(typeof(ConvertToEntity))]
	[UnityEngine.DisallowMultipleComponent]
	public class VA_AnimationLibraryComponentAuthoring : UnityEngine.MonoBehaviour
	{
		public VA_AnimationLibrarySO animationLibrary;
	}
	
	public class VA_AnimationLibraryConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((VA_AnimationLibraryComponentAuthoring animationLib) =>
			{
				// Blob builder to build.
				using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
				{
					// Construct the root.
					ref VA_AnimationLibrary animationDataBlobAsset = ref blobBuilder.ConstructRoot<VA_AnimationLibrary>();

					// Set all the data.
					BlobBuilderArray<VA_AnimationData> animationDataArray = blobBuilder.Allocate(ref animationDataBlobAsset.animations, animationLib.animationLibrary.animations.Count);

                    for (int i = 0; i < animationDataArray.Length; i++)
					{
						// Copy data.
						animationDataArray[i] = animationLib.animationLibrary.animations[i];
					}

					// Construct blob asset reference.
					BlobAssetReference<VA_AnimationLibrary> animLibAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationLibrary>(Allocator.Persistent);

					// Add it to the asset store.
					// TODO: Generate Hash based on Guid.
					BlobAssetStore.TryAdd(new Hash128("AnimationLib"), animLibAssetRef);

					UnityEngine.Debug.Log("VA_AnimationLibrary has " + animLibAssetRef.Value.animations.Length.ToString() + " animations.");
				}

				// Remove the entity since we don't need it anymore.
				DstEntityManager.DestroyEntity(GetPrimaryEntity(animationLib));
			});
		}
	}
}