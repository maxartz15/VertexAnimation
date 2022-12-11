using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TAO.VertexAnimation
{

public class AnimationDataComponentAuthoring : MonoBehaviour
{
    public float4 AnimationDataOne;
    public float4 AnimationDataTwo;
    public float BlendFactor;
}

public class AnimationDataComponentBaker : Baker < AnimationDataComponentAuthoring >
{
    public override void Bake( AnimationDataComponentAuthoring authoring )
    {
        //Entity parent = GetEntity( authoring.RootParent );
        AddComponent( new FirstAnimationDataComponent{ Value = authoring.AnimationDataOne} );
        AddComponent( new SecondAnimationDataComponent{ Value = authoring.AnimationDataTwo} );
        AddComponent( new BlendingAnimationDataComponent{ Value = authoring.BlendFactor} );
    }
}
}
