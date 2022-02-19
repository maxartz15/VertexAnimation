using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace TAO.VertexAnimation
{
	[DisallowMultipleComponent]
	public class VA_AnimatorComponentAuthoring : MonoBehaviour
	{
		public VA_AnimationLibrary lib;
	}

	//[GenerateAuthoringComponent]
	public struct VA_AnimatorComponent : IComponentData
	{
		public int animationIndex;
		public int animationIndexNext;
		public float animationTime;
		public BlobAssetReference<VA_AnimationLibraryData> animationLibrary;
	}

	[UpdateAfter(typeof(VA_AnimationLibraryConversionSystem))]
	public class VA_AnimatorConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			// Static because of multi scene setup.
			//BlobAssetReference<VA_AnimationLibraryData> animLib = VA_AnimationLibraryConversionSystem.animLibAssetRef;

			Entities.ForEach((VA_AnimatorComponentAuthoring animator) =>
			{

				BlobAssetStore.TryGet(animator.lib.key, out BlobAssetReference<VA_AnimationLibraryData> animLib);

				Entity entity = GetPrimaryEntity(animator);

				// Add animator to 'parent'.
				VA_AnimatorComponent animatorComponent = new VA_AnimatorComponent
				{
					animationIndex = 0,
					animationIndexNext = -1,
					animationTime = 0,
					animationLibrary = animLib
				};
				DstEntityManager.AddComponentData(entity, animatorComponent);

				// Add the Material data to the children.
				var children = animator.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < children.Length; i++)
				{
					Entity ent = GetPrimaryEntity(children[i]);
				
					VA_AnimationDataComponent animationData = new VA_AnimationDataComponent();
					DstEntityManager.AddComponentData(ent, animationData);
				}
			});
		}
	}
}
