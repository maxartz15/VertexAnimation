using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace TAO.VertexAnimation
{
	// System to update all the animations.
	public class VA_AnimatorSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			var animationData = GetComponentDataFromEntity<VA_AnimationDataComponent>(false);

			Entities.ForEach((ref VA_AnimatorComponent ac, in DynamicBuffer<Child> children) =>
			{
				for (int i = 0; i < children.Length; i++)
				{
					// Get child.
					Entity child = children[i].Value;

					// Get the animation lib data.
					ref VA_AnimationLibraryData animationsRef = ref ac.animationLibrary.Value;

					animationData[child] = new VA_AnimationDataComponent
					{
						Value = new float4
						{
							x = ac.animationTime,
							y = VA_AnimationLibraryUtils.GetAnimationMapIndex(ref animationsRef, ac.animationIndex),
							z = VA_AnimationLibraryUtils.GetColorMapIndex(ref animationsRef, ac.animationIndex),
							w = 0
						}
					};
				}
			})
			.WithNativeDisableContainerSafetyRestriction(animationData)
			.ScheduleParallel();
		}
	}
}