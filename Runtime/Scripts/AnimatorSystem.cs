using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

        RefRW < EntitiesAnimationCurveLibrary > curveLibrary =
            SystemAPI.GetSingletonRW < EntitiesAnimationCurveLibrary >();

        UpdateAnimatorJob job =
            new UpdateAnimatorJob
            {
                DeltaTime = deltaTime,
                StartIndex = 0,
                Ecb = ecb.AsParallelWriter(),
                EntitiesAnimationCurveLibrary = curveLibrary
            };

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
    [NativeDisableUnsafePtrRestriction]
    public RefRW < EntitiesAnimationCurveLibrary > EntitiesAnimationCurveLibrary;

    [BurstCompile]
    public void Execute(
        ref AnimatorComponent animator,
        ref AnimatorBlendStateComponent animatorBlendState )
    {
        if ( animator.Enabled )
        {
            // Get the animation lib data.
            ref VA_AnimationLibraryData animationsRef = ref animator.AnimationLibrary.Value;

            int animationIndexNextBlend = 0;
            float animationTimeNextBlend = 0.0f;
            float blendValue = 0.0f;

            if ( animatorBlendState.BlendingEnabled )
            {
                animatorBlendState.BlendingCurrentTime += DeltaTime;

                if ( animatorBlendState.BlendingCurrentTime >
                     animatorBlendState.BlendingDuration )
                {
                    animator.AnimationIndex = animatorBlendState.ToAnimationIndex;
                    animator.AnimationIndexNext = -1;
                    animator.AnimationTime = animatorBlendState.AnimationTime;

                    animatorBlendState.BlendingEnabled = false;
                    animatorBlendState.ToAnimationIndex = -1;
                    animatorBlendState.AnimationTime = 0;
                    animatorBlendState.BlendingDuration = 0.0f;
                    animatorBlendState.BlendingCurrentTime = 0.0f;
                    animatorBlendState.AnimationBlendingCurveIndex = -1;

                    for ( int i = 0; i < animator.SkinnedMeshes.Length; i++ )
                    {
                        BlendingAnimationDataComponent blendingAnimationDataComponent =
                            new BlendingAnimationDataComponent { Value = 0.0f };

                        Ecb.SetComponent < BlendingAnimationDataComponent >(
                            StartIndex,
                            animator.SkinnedMeshes[i],
                            blendingAnimationDataComponent );

                        StartIndex++;
                        SecondAnimationDataComponent vaAnimationDataComponent2 = new SecondAnimationDataComponent();

                        vaAnimationDataComponent2.Value = new float4
                        {
                            x = 0.0f,
                            y = animatorBlendState.ToAnimationIndex,
                            z = 0.0f,
                            w = animatorBlendState.ToAnimationIndex
                        };

                        Ecb.SetComponent < SecondAnimationDataComponent >(
                            StartIndex,
                            animator.SkinnedMeshes[i],
                            vaAnimationDataComponent2 );

                        StartIndex++;
                    }
                   
                }
                else
                {
                    float blendTime =
                        1.0f / animatorBlendState.BlendingDuration * animatorBlendState.BlendingCurrentTime;

                    blendValue = EntitiesAnimationCurveLibrary.ValueRW.
                                                               CurveReferences[
                                                                   animatorBlendState.AnimationBlendingCurveIndex].
                                                               GetValueAtTime( blendTime );

                    animatorBlendState.AnimationTime += DeltaTime *
                                                        animationsRef.
                                                            animations[animatorBlendState.ToAnimationIndex].
                                                            frameTime;

                    if ( animatorBlendState.AnimationTime >
                         animationsRef.animations[animatorBlendState.ToAnimationIndex].duration )
                    {
                        // Set time. Using the difference to smoothen out animations when looping.
                        animatorBlendState.AnimationTime -=
                            animationsRef.animations[animatorBlendState.ToAnimationIndex].duration;

                        //animator.animationIndexNext = vaAnimatorStateComponent.Rand.NextInt( 20 );
                    }

                    // Lerp animations.
                    // Set animation for lerp.
                    animationIndexNextBlend = animatorBlendState.ToAnimationIndex;

                    // Calculate next frame time for lerp.
                    animationTimeNextBlend = animatorBlendState.AnimationTime +
                                             ( 1.0f / animationsRef.animations[animationIndexNextBlend].maxFrames );

                    if ( animationTimeNextBlend > animationsRef.animations[animationIndexNextBlend].duration )
                    {
                        // Set time. Using the difference to smooth out animations when looping.
                        animationTimeNextBlend -= animatorBlendState.AnimationTime;
                    }
                }
            }

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

            for ( int i = 0; i < animator.SkinnedMeshes.Length; i++ )
            {
                FirstAnimationDataComponent vaAnimationDataComponent = new FirstAnimationDataComponent();

                vaAnimationDataComponent.Value = new float4
                {
                    x = animator.AnimationTime,
                    y = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animator.AnimationIndex ),
                    z = animationTimeNext,
                    w = VA_AnimationLibraryUtils.GetAnimationMapIndex( ref animationsRef, animationIndexNext )
                };

                Ecb.SetComponent < FirstAnimationDataComponent >(
                    StartIndex,
                    animator.SkinnedMeshes[i],
                    vaAnimationDataComponent );

                StartIndex++;

                if ( animatorBlendState.BlendingEnabled )
                {
                    BlendingAnimationDataComponent blendingAnimationDataComponent =
                        new BlendingAnimationDataComponent { Value = blendValue };

                    Ecb.SetComponent < BlendingAnimationDataComponent >(
                        StartIndex,
                        animator.SkinnedMeshes[i],
                        blendingAnimationDataComponent );

                    StartIndex++;
                    SecondAnimationDataComponent vaAnimationDataComponent2 = new SecondAnimationDataComponent();

                    vaAnimationDataComponent2.Value = new float4
                    {
                        x = animatorBlendState.AnimationTime,
                        y = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                            ref animationsRef,
                            animatorBlendState.ToAnimationIndex ),
                        z = animationTimeNextBlend,
                        w = VA_AnimationLibraryUtils.GetAnimationMapIndex(
                            ref animationsRef,
                            animationIndexNextBlend )
                    };

                    Ecb.SetComponent < SecondAnimationDataComponent >(
                        StartIndex,
                        animator.SkinnedMeshes[i],
                        vaAnimationDataComponent2 );

                    StartIndex++;
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
