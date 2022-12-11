using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TAO.VertexAnimation
{

[RequireMatchingQueriesForUpdate]
public partial class AnimationBakingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer( Allocator.Temp );
        foreach (var (animator, wait) in
                 SystemAPI.Query<RefRW<AnimatorComponent>, RefRW<AnimatorWaitingForBaking>>())
        {
            DynamicBuffer<Child> children = EntityManager.GetBuffer < Child >( wait.ValueRO.AnimatorEntity );
            Debug.Log( children.Length );
            animator.ValueRW.SkinnedMeshes = new NativeArray < Entity >( children.Length, Allocator.Persistent );
            int i = 0;
            foreach ( Child child in children )
            {
                animator.ValueRW.SkinnedMeshes[i] = child.Value;
                i++;
            }

            wait.ValueRW.IsInitialized = true;
            entityCommandBuffer.RemoveComponent<AnimatorWaitingForBaking>( wait.ValueRO.AnimatorEntity );
            //EntityManager.RemoveComponent < AnimatorWaitingForBaking >( wait.AnimatorEntity );
        }
        
        entityCommandBuffer.Playback( EntityManager );
    }
}

}
