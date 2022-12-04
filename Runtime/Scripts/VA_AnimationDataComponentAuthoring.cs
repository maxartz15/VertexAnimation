using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TAO.VertexAnimation
{

public class VA_AnimationDataComponentAuthoring : MonoBehaviour
{
    public float4 Color;
}

public class VA_AnimationDataBaker : Baker < VA_AnimationDataComponentAuthoring >
{
    public override void Bake( VA_AnimationDataComponentAuthoring authoring )
    {
        //Entity parent = GetEntity( authoring.RootParent );
        AddComponent( new VaAnimationDataComponent{ Value = authoring.Color} );
    }
}
}
