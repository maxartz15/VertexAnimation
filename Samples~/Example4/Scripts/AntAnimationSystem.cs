using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TAO.VertexAnimation.Example4
{
	public class AntAnimationSystem : SystemBase
	{
		
		
		protected override void OnUpdate()
		{
			float deltaTime = Time.DeltaTime;

			

			Entities.ForEach((Entity entity, ref VA_AnimatorComponent ac, ref AntAnimationComponent cac) =>
			{
				// Get the animation lib data.
				ref VA_AnimationLibraryData animationsRef = ref ac.animationLibrary.Value;

				// Set the animation index on the AnimatorComponent to play this animation.
				ac.animationIndex = VA_AnimationLibraryUtils.GetAnimation(ref animationsRef, cac.curAnimation);

				// 'Play' the actual animation.
				ac.animationTime += deltaTime * animationsRef.animations[ac.animationIndex].frameTime;

				if (ac.animationTime > animationsRef.animations[ac.animationIndex].duration)
				{
					// Set time. Using the difference to smoothen out animations when looping.
					//ac.animationTime -= animationsRef.animations[ac.animationIndex].duration;
					ac.animationTime = 0;

                  

                }
			}).ScheduleParallel();
		}
	}
}
