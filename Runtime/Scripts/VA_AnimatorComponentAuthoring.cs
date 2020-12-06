using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace TAO.VertexAnimation
{
	[DisallowMultipleComponent]
	public class VA_AnimatorComponentAuthoring : MonoBehaviour
	{

	}

	//[GenerateAuthoringComponent]
	public struct VA_AnimatorComponent : IComponentData
	{
		public int animationIndex;
		public int animationIndexSchedule;
		public float animationTime;
	}

	public class VA_AnimatorConversionSystem : GameObjectConversionSystem
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((VA_AnimatorComponentAuthoring animator) =>
			{
				Entity entity = GetPrimaryEntity(animator);
			
				// Add animator to 'parent'.
				VA_AnimatorComponent animatorComponent = new VA_AnimatorComponent
				{
					animationIndex = 0,
					animationIndexSchedule = -1,
					animationTime = 0,
			};			
				DstEntityManager.AddComponentData(entity, animatorComponent);

				// Add the Material data to the children.
				var children = animator.GetComponentsInChildren<MeshRenderer>();
				for (int i = 0; i < children.Length; i++)
				{
					Entity ent = GetPrimaryEntity(children[i]);
				
					VA_AnimationIndexComponent animationIndex = new VA_AnimationIndexComponent { Value = 0 };
					DstEntityManager.AddComponentData(ent, animationIndex);
					VA_AnimationTimeComponent animationTime = new VA_AnimationTimeComponent { Value = 0 };
					DstEntityManager.AddComponentData(ent, animationTime);
				}
			});
		}
	}
}
