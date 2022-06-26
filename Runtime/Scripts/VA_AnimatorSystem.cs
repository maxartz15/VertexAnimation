using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace TAO.VertexAnimation
{
	// System to update all the animations.
	public partial class VA_AnimatorSystem : SystemBase
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

					// Lerp animations.
					// Set animation for lerp.
					int animationIndexNext = ac.animationIndexNext;
					if (ac.animationIndexNext < 0)
					{
						animationIndexNext = ac.animationIndex;
					}

					// Calculate next frame time for lerp.
					float animationTimeNext = ac.animationTime + (1.0f / animationsRef.animations[animationIndexNext].maxFrames);
					if (animationTimeNext > animationsRef.animations[animationIndexNext].duration)
					{
						// Set time. Using the difference to smooth out animations when looping.
						animationTimeNext -= ac.animationTime;
					}

					// Set material data.
					animationData[child] = new VA_AnimationDataComponent
					{
						Value = new float4
						{
							x = ac.animationTime,
							y = VA_AnimationLibraryUtils.GetAnimationMapIndex(ref animationsRef, ac.animationIndex),
							z = animationTimeNext,
							w = VA_AnimationLibraryUtils.GetAnimationMapIndex(ref animationsRef, animationIndexNext)
						}
					};
				}
			})
			.WithNativeDisableContainerSafetyRestriction(animationData)
			.WithName("VA_AnimatorSystem")
			.ScheduleParallel();
		}
	}
}