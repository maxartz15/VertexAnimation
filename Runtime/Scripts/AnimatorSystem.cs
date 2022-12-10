using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace TAO.VertexAnimation
{

// System to update all the animations.
public partial struct AnimatorSystem : ISystem
{
    public void OnCreate( ref SystemState state )
    {
    }

    public void OnDestroy( ref SystemState state )
    {
    }

    public void OnUpdate( ref SystemState state )
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer ecb = new EntityCommandBuffer( Allocator.Temp );
        int i = 0;
        foreach ( var animatorAspect in SystemAPI.
                     Query < AnimatorAspect >() )
        {
           animatorAspect.UpdateAnimator( deltaTime, ecb.AsParallelWriter(), ref i );
        }
        
        ecb.Playback( state.EntityManager );
    }
}

}
