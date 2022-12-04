using System.Collections.Generic;
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
			float deltaTime = SystemAPI.Time.DeltaTime;
			// This is only executed if we have a valid skinning setup
			Entities
				.ForEach((ref VA_AnimatorComponent animator, in VA_AnimatorStateComponent vaAnimatorStateComponent, in DynamicBuffer<SkinnedMeshEntity> bones) =>
				{
					if ( vaAnimatorStateComponent.Enabled )
					{
						// Get the animation lib data.
						ref VA_AnimationLibraryData animationsRef = ref animator.animationLibrary.Value;

						//if ( animator.AnimationName != vaAnimatorStateComponent.CurrentAnimationName )
						//{
						//	// Set the animation index on the AnimatorComponent to play this animation.
						//	animator.animationIndex = VA_AnimationLibraryUtils.GetAnimation(ref animationsRef, vaAnimatorStateComponent.CurrentAnimationName);
						//	animator.AnimationName = vaAnimatorStateComponent.CurrentAnimationName;
						//}
						
						
						// 'Play' the actual animation.
						animator.animationTime += deltaTime * animationsRef.animations[animator.animationIndex].frameTime;

						if (animator.animationTime > animationsRef.animations[animator.animationIndex].duration)
						{
							// Set time. Using the difference to smoothen out animations when looping.
							animator.animationTime -= animationsRef.animations[animator.animationIndex].duration;
						}

						// Lerp animations.
						// Set animation for lerp.
						int animationIndexNext = animator.animationIndexNext;
						if (animator.animationIndexNext < 0)
						{
							animationIndexNext = animator.animationIndex;
							//animator.animationIndexNext = animationIndexNext + 1;
						}

						// Calculate next frame time for lerp.
						float animationTimeNext = animator.animationTime + (1.0f / animationsRef.animations[animationIndexNext].maxFrames);
						if (animationTimeNext > animationsRef.animations[animationIndexNext].duration)
						{
							// Set time. Using the difference to smooth out animations when looping.
							animationTimeNext -= animator.animationTime;
						}

						for ( int i = 0; i < bones.Length; i++ )
						{
							VaAnimationDataComponent vaAnimationDataComponent = new VaAnimationDataComponent();
							vaAnimationDataComponent.Value = new float4
							{
								x = animator.animationTime,
								y = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animator.animationIndex ),
								z = animationTimeNext,
								w = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animationIndexNext )
							};
							SystemAPI.SetComponent<VaAnimationDataComponent>( bones[i].Value, vaAnimationDataComponent );
						}
					}
					
				}).Run();
		}
	}
}