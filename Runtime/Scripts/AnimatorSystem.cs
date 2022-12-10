using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace TAO.VertexAnimation
{

// System to update all the animations.

[BurstCompile]
[UpdateInGroup( typeof( SimulationSystemGroup ) )]
[UpdateAfter( typeof( BeginSimulationEntityCommandBufferSystem ) )]
public partial struct AnimatorSystem : ISystem
{
    //private EntityQuery m_Group;
    [BurstCompile]
    public void OnCreate( ref SystemState state )
    {
        //NativeArray < ComponentType > componentTypes = new NativeArray < ComponentType >( 2, Allocator.Persistent );
        //componentTypes[0] = ComponentType.ReadWrite < AnimatorComponent >();
        //componentTypes[1] = ComponentType.ReadWrite<AnimatorBlendStateComponent>();
        //m_Group = state.GetEntityQuery(componentTypes);
    }

    [BurstCompile]
    public void OnDestroy( ref SystemState state )
    {
    }

    [BurstCompile]
    public void OnUpdate( ref SystemState state )
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        EntityCommandBuffer ecb = SystemAPI.GetSingleton < BeginSimulationEntityCommandBufferSystem.Singleton >().
                                            CreateCommandBuffer( state.WorldUnmanaged );

        UpdateAnimatorJob job =
            new UpdateAnimatorJob { DeltaTime = deltaTime, StartIndex = 0, Ecb = ecb.AsParallelWriter() };

        job.Run();

        //handle.Complete();
        // state.Dependency = JobHandle.CombineDependencies( ecb., state.Dependency );
        //state.Dependency.Complete();

        // int i = 0;
        // for ( int j = 0; j < jobHandles.Length; j++ )
        // {
        //     jobHandles2.Add(new UpdateAnimatedMeshMaterialParametersJob { Ecb = ecb.AsParallelWriter(), StartIndex = i }.ScheduleParallel(jobHandles[j] ));
        //     i += 2;
        // }
        //
        // for ( int j = 0; j < jobHandles2.Length; j++ )
        // {
        //     jobHandles2[j].Complete();
        // }
        // //ecb.Playback( state.EntityManager );
    }
}

[BurstCompile]
public partial struct UpdateAnimatorJob : IJobEntity
{
    public float DeltaTime;
    public int StartIndex;
    public EntityCommandBuffer.ParallelWriter Ecb;

    [BurstCompile]
    public void Execute(
        ref AnimatorComponent animator,
        ref AnimatorBlendStateComponent animatorBlendState,
        in DynamicBuffer < SkinnedMeshEntity > buffer )
    {
        if ( animator.Enabled )
        {
            // Get the animation lib data.
            ref VA_AnimationLibraryData animationsRef = ref animator.AnimationLibrary.Value;

            //if ( animator.AnimationName != vaAnimatorStateComponent.CurrentAnimationName )
            //{
            //	// Set the animation index on the AnimatorComponent to play this animation.
            //	animator.AnimationIndexNext = VA_AnimationLibraryUtils.GetAnimation(ref animationsRef, vaAnimatorStateComponent.CurrentAnimationName);
            //	animator.AnimationName = vaAnimatorStateComponent.CurrentAnimationName;
            //}

            // 'Play' the actual animation.
            animator.AnimationTime += DeltaTime * animationsRef.animations[animator.AnimationIndex].frameTime;

            if ( animator.AnimationTime > animationsRef.animations[animator.AnimationIndex].duration )
            {
                // Set time. Using the difference to smoothen out animations when looping.
                animator.AnimationTime -= animationsRef.animations[animator.AnimationIndex].duration;

                //animator.animationIndexNext = vaAnimatorStateComponent.Rand.NextInt( 20 );
            }

            // Lerp animations.
            // Set animation for lerp.
            int animationIndexNext = animator.AnimationIndexNext;

            if ( animationIndexNext < 0 )
            {
                animationIndexNext = animator.AnimationIndex;

                //animator.animationIndexNext = animationIndexNext + 1;
            }

            // Calculate next frame time for lerp.
            float animationTimeNext = animator.AnimationTime +
                                      ( 1.0f / animationsRef.animations[animationIndexNext].maxFrames );

            if ( animationTimeNext > animationsRef.animations[animationIndexNext].duration )
            {
                // Set time. Using the difference to smooth out animations when looping.
                animationTimeNext -= animator.AnimationTime;
            }

            int animationIndexNextBlend = 0;
            float animationTimeNextBlend = 0.0f;

            if ( animatorBlendState.BlendingEnabled )
            {
                // 'Play' the actual animation.
                animatorBlendState.AnimationTime += DeltaTime *
                                                    animationsRef.
                                                        animations[animatorBlendState.AnimationIndex].
                                                        frameTime;

                if ( animatorBlendState.AnimationTime >
                     animationsRef.animations[animatorBlendState.AnimationIndex].duration )
                {
                    // Set time. Using the difference to smoothen out animations when looping.
                    animatorBlendState.AnimationTime -=
                        animationsRef.animations[animatorBlendState.AnimationIndex].duration;

                    //animator.animationIndexNext = vaAnimatorStateComponent.Rand.NextInt( 20 );
                }

                // Lerp animations.
                // Set animation for lerp.
                animationIndexNextBlend = animatorBlendState.AnimationIndexNext;

                if ( animationIndexNextBlend < 0 )
                {
                    animationIndexNextBlend = animatorBlendState.AnimationIndex;

                    //animator.animationIndexNext = animationIndexNext + 1;
                }

                // Calculate next frame time for lerp.
                animationTimeNextBlend = animatorBlendState.AnimationTime +
                                         ( 1.0f / animationsRef.animations[animationIndexNextBlend].maxFrames );

                if ( animationTimeNextBlend > animationsRef.animations[animationIndexNextBlend].duration )
                {
                    // Set time. Using the difference to smooth out animations when looping.
                    animationTimeNextBlend -= animatorBlendState.AnimationTime;
                }
            }

            for ( int i = 0; i < buffer.Length; i++ )
            {
                FirstAnimationDataComponent vaAnimationDataComponent = new FirstAnimationDataComponent();

                vaAnimationDataComponent.Value = new float4
                {
                    x = animator.AnimationTime,
                    y = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animator.AnimationIndex ),
                    z = animationTimeNext,
                    w = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animationIndexNext )
                };

                SystemAPI.SetComponent < FirstAnimationDataComponent >( buffer[i].Value, vaAnimationDataComponent );

                if ( animatorBlendState.BlendingEnabled )
                {
                    SecondAnimationDataComponent vaAnimationDataComponent2 = new SecondAnimationDataComponent();

                    vaAnimationDataComponent2.Value = new float4
                    {
                        x = animatorBlendState.AnimationTime,
                        y = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                            ref animationsRef,
                            animatorBlendState.AnimationIndex ),
                        z = animationTimeNextBlend,
                        w = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                            ref animationsRef,
                            animationIndexNextBlend )
                    };

                    SystemAPI.SetComponent < SecondAnimationDataComponent >(
                        buffer[i].Value,
                        vaAnimationDataComponent2 );
                }
            }

            if ( animator.AnimationIndexNext >= 0 )
            {
                animator.AnimationIndex = animationIndexNext;
                animator.AnimationIndexNext = -1;
            }
        }
    }
}

}
