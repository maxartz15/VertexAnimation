using Unity.Entities;
using Unity.Collections;
using Unity.Assertions;
using System;

namespace TAO.VertexAnimation
{
	[UnityEngine.RequireComponent(typeof(ConvertToEntity))]
	[UnityEngine.DisallowMultipleComponent]
	public class VA_AnimationLibraryComponentAuthoring : UnityEngine.MonoBehaviour
	{
		public VA_AnimationLibrary animationLibrary;
		public bool debugMode = false;
	}
	
	public class VA_AnimationLibraryConversionSystem : GameObjectConversionSystem
	{
		// Static because of multi scene setup.
		//public static BlobAssetReference<VA_AnimationLibraryData> animLibAssetRef;

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
					BlobBuilderArray<VA_AnimationData> animationDataArray = blobBuilder.Allocate(ref animationDataBlobAsset.animations, animationLib.animationLibrary.animationData.Count);

					for (int i = 0; i < animationDataArray.Length; i++)
					{
						// Copy data.
						animationDataArray[i] = animationLib.animationLibrary.animationData[i];

						if (animationLib.debugMode)
						{
							UnityEngine.Debug.Log("VA_AnimationLibrary added " + animationDataArray[i].name.ToString());
						}
					}

					// Construct blob asset reference.
					BlobAssetReference<VA_AnimationLibraryData> animLibAssetRef = blobBuilder.CreateBlobAssetReference<VA_AnimationLibraryData>(Allocator.Persistent);


					// Add it to the asset store.
					var anumLibName = animationLib.animationLibrary.name;

					var hash = animationLib.animationLibrary.key;
					var result = BlobAssetStore.TryAdd(hash, animLibAssetRef);

					if (animationLib.debugMode)
					{
						UnityEngine.Debug.Log($"blob asset {anumLibName} key: {hash.Value.x} {hash.Value.y} {hash.Value.z} {hash.Value.w} ");
						UnityEngine.Debug.Log($"VA_AnimationLibrary {anumLibName} has {animLibAssetRef.Value.animations.Length.ToString()} animations.");
					}


					Assert.IsTrue(result, $"{anumLibName} hasn't been added to the blob asset store");
				}

				// Remove the entity since we don't need it anymore.
				DstEntityManager.DestroyEntity(GetPrimaryEntity(animationLib));
			});
		}
	}
}