using Unity.Entities;
using Unity.Mathematics;
using TAO.VertexAnimation;

public partial class CharacterAnimationSystem : SystemBase
{
	protected override void OnUpdate()
	{
		float deltaTime = UnityEngine.Time.deltaTime;
		Random random = Random.CreateFromIndex((uint)UnityEngine.Time.time);

		Entities.ForEach((Entity entity, ref VA_AnimatorComponent ac, ref CharacterAnimationComponent cac) =>
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

				// Select new animation to play.
				if (random.NextBool())
				{
					cac.curAnimation = cac.idle;
				}
				else
				{
					cac.curAnimation = cac.run;
				}
			}
		}).ScheduleParallel();
	}
}