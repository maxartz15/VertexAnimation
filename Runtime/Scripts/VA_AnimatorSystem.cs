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
			// This is only executed if we have a valid skinning setup
			Entities
				.ForEach((VA_AnimatorComponent animator, in DynamicBuffer<SkinnedMeshEntity> bones) =>
				{
					// Get the animation lib data.
					ref VA_AnimationLibraryData animationsRef = ref animator.animationLibrary.Value;

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
				}).Run();
		}
	}
}