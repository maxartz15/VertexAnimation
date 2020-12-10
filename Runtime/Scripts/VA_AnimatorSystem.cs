using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

namespace TAO.VertexAnimation
{
	public class VA_AnimatorSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			var atc = GetComponentDataFromEntity<VA_AnimationTimeComponent>(false);
			var aic = GetComponentDataFromEntity<VA_AnimationIndexComponent>(false);

			Entities.ForEach((ref VA_AnimatorComponent ac, in DynamicBuffer<Child> children) =>
			{
				for (int i = 0; i < children.Length; i++)
				{
					// Get child.
					Entity child = children[i].Value;

					atc[child] = new VA_AnimationTimeComponent { Value = ac.animationTime };

					//// Get a copy of the time Component of the child.
					//VA_AnimationTimeComponent atcCopy = atc[child];
					//// Set new value.
					//atcCopy.Value = ac.animationTime;
					//// Update original.
					//atc[child] = atcCopy;

					aic[child] = new VA_AnimationIndexComponent { Value = ac.animationIndex };

					//VA_AnimationIndexComponent aicCopy = aic[child];
					//aicCopy.Value = ac.animationIndex;
					//aic[child] = aicCopy;
				}
			})
			.WithNativeDisableContainerSafetyRestriction(atc)
			.WithNativeDisableContainerSafetyRestriction(aic)
			.ScheduleParallel();
		}
	}

	// Example systems to update animation parameters.
	[UpdateBefore(typeof(VA_AnimatorSystem))]
	public class VA_AnimationTimeSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			float deltaTime = UnityEngine.Time.deltaTime;
			
			Entities.ForEach((ref VA_AnimatorComponent ac) =>
			{
				// Get the animation lib data.
				ref VA_AnimationLibraryData animationsRef = ref ac.animationLibrary.Value;

				ac.animationTime += deltaTime;

                if (ac.animationTime > animationsRef.animations[ac.animationIndex].duration)
				{
					ac.animationTime = ac.animationTime - animationsRef.animations[ac.animationIndex].duration;
				}

			}).ScheduleParallel();
		}
	}
	
	[UpdateBefore(typeof(VA_AnimatorSystem))]
	public class VA_AnimationIndexSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref VA_AnimatorComponent ac) =>
			{
                // Get the animation lib data.
                ref VA_AnimationLibraryData animationLib = ref ac.animationLibrary.Value;

				int animationIndex = VA_AnimationLibraryUtils.GetAnimation(ref animationLib, "Shoot");

                //int index = entity.Index % 2;
                //ac.animationIndex = index;
                ac.animationIndex = animationIndex;
			}).ScheduleParallel();
		}
	}
}