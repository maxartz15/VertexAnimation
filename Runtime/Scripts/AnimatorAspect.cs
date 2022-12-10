using Unity.Entities;
using Unity.Mathematics;

namespace TAO.VertexAnimation
{

public readonly partial struct AnimatorAspect : IAspect
{
    private readonly RefRW < VA_AnimatorComponent > m_Animator;
    private readonly RefRW < VA_AnimatorBlendStateComponent > m_AnimatorBlendState;
    private readonly DynamicBuffer < SkinnedMeshEntity > m_SkinnedMeshEntities;

    public void UpdateAnimator(float deltaTime, EntityCommandBuffer.ParallelWriter ecb, ref int startIndex)
    {
        if ( m_Animator.ValueRO.Enabled )
        {
              // Get the animation lib data.
                ref VA_AnimationLibraryData animationsRef = ref m_Animator.ValueRO.AnimationLibrary.Value;

                //if ( m_Animator.ValueRO.AnimationName != vaAnimatorStateComponent.CurrentAnimationName )
                //{
                //	// Set the animation index on the AnimatorComponent to play this animation.
                //	m_Animator.ValueRO.AnimationIndexNext = VA_AnimationLibraryUtils.GetAnimation(ref animationsRef, vaAnimatorStateComponent.CurrentAnimationName);
                //	m_Animator.ValueRO.AnimationName = vaAnimatorStateComponent.CurrentAnimationName;
                //}

                // 'Play' the actual animation.
                m_Animator.ValueRW.AnimationTime += deltaTime * animationsRef.animations[m_Animator.ValueRO.AnimationIndex].frameTime;

                if ( m_Animator.ValueRO.AnimationTime > animationsRef.animations[m_Animator.ValueRO.AnimationIndex].duration )
                {
                    // Set time. Using the difference to smoothen out animations when looping.
                    m_Animator.ValueRW.AnimationTime -= animationsRef.animations[m_Animator.ValueRO.AnimationIndex].duration;

                    //m_Animator.ValueRW.animationIndexNext = vaAnimatorStateComponent.Rand.NextInt( 20 );
                }

                // Lerp animations.
                // Set animation for lerp.
                int animationIndexNext = m_Animator.ValueRO.AnimationIndexNext;

                if ( animationIndexNext < 0 )
                {
                    animationIndexNext = m_Animator.ValueRO.AnimationIndex;

                    //m_Animator.ValueRO.animationIndexNext = animationIndexNext + 1;
                }

                // Calculate next frame time for lerp.
                float animationTimeNext = m_Animator.ValueRO.AnimationTime +
                                          ( 1.0f / animationsRef.animations[animationIndexNext].maxFrames );

                if ( animationTimeNext > animationsRef.animations[animationIndexNext].duration )
                {
                    // Set time. Using the difference to smooth out animations when looping.
                    animationTimeNext -= m_Animator.ValueRO.AnimationTime;
                }

                int animationIndexNextBlend = 0;
                float animationTimeNextBlend = 0.0f;

                if ( m_AnimatorBlendState.ValueRO.BlendingEnabled )
                {
                    // 'Play' the actual animation.
                    m_AnimatorBlendState.ValueRW.AnimationTime += deltaTime *
                                                                  animationsRef.
                                                                      animations[m_AnimatorBlendState.ValueRO.AnimationIndex].
                                                                      frameTime;

                    if ( m_AnimatorBlendState.ValueRO.AnimationTime >
                         animationsRef.animations[m_AnimatorBlendState.ValueRO.AnimationIndex].duration )
                    {
                        // Set time. Using the difference to smoothen out animations when looping.
                        m_AnimatorBlendState.ValueRW.AnimationTime -=
                            animationsRef.animations[m_AnimatorBlendState.ValueRO.AnimationIndex].duration;

                        //m_Animator.ValueRO.animationIndexNext = vaAnimatorStateComponent.Rand.NextInt( 20 );
                    }

                    // Lerp animations.
                    // Set animation for lerp.
                    animationIndexNextBlend = m_AnimatorBlendState.ValueRO.AnimationIndexNext;

                    if ( animationIndexNextBlend < 0 )
                    {
                        animationIndexNextBlend = m_AnimatorBlendState.ValueRO.AnimationIndex;

                        //m_Animator.ValueRO.animationIndexNext = animationIndexNext + 1;
                    }

                    // Calculate next frame time for lerp.
                    animationTimeNextBlend = m_AnimatorBlendState.ValueRO.AnimationTime +
                                             ( 1.0f / animationsRef.animations[animationIndexNextBlend].maxFrames );

                    if ( animationTimeNextBlend > animationsRef.animations[animationIndexNextBlend].duration )
                    {
                        // Set time. Using the difference to smooth out animations when looping.
                        animationTimeNextBlend -= m_AnimatorBlendState.ValueRO.AnimationTime;
                    }
                }

                for ( int i = 0; i < m_SkinnedMeshEntities.Length; i++ )
                {
                    FirstAnimationDataComponent animationDataComponent = new FirstAnimationDataComponent();

                    animationDataComponent.Value = new float4
                    {
                        x = m_Animator.ValueRO.AnimationTime,
                        y = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                            ref animationsRef,
                            m_Animator.ValueRO.AnimationIndex ),
                        z = animationTimeNext,
                        w = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animationIndexNext )
                    };

                    ecb.SetComponent < FirstAnimationDataComponent >(startIndex,
                        m_SkinnedMeshEntities[i].Value,
                        animationDataComponent );

                    startIndex++;

                    if ( m_AnimatorBlendState.ValueRO.BlendingEnabled )
                    {
                        SecondAnimationDataComponent animationDataComponent2 = new SecondAnimationDataComponent();

                        animationDataComponent2.Value = new float4
                        {
                            x = m_AnimatorBlendState.ValueRO.AnimationTime,
                            y = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                                ref animationsRef,
                                m_AnimatorBlendState.ValueRO.AnimationIndex ),
                            z = animationTimeNextBlend,
                            w = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                                ref animationsRef,
                                animationIndexNextBlend )
                        };

                        ecb.SetComponent < SecondAnimationDataComponent >(startIndex,
                            m_SkinnedMeshEntities[i].Value,
                            animationDataComponent2 );

                        startIndex++;
                    }
                }

                if ( m_Animator.ValueRO.AnimationIndexNext >= 0 )
                {
                    m_Animator.ValueRW.AnimationIndex = animationIndexNext;
                    m_Animator.ValueRW.AnimationIndexNext = -1;
                }
        }
    }
}

}
