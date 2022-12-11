using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace TAO.VertexAnimation
{
[MaterialProperty("_AnimationDataOne")] //, MaterialPropertyFormat.Float4
public struct FirstAnimationDataComponent : IComponentData
{
	// animationTime, animationIndex, colorIndex, nan.
	public float4 Value;
}

[MaterialProperty("_AnimationDataTwo")] //, MaterialPropertyFormat.Float4
public struct SecondAnimationDataComponent : IComponentData
{
	// animationTime, animationIndex, colorIndex, nan.
	public float4 Value;
}

[MaterialProperty("_AnimationDataBlend")] //, MaterialPropertyFormat.Float4
public struct BlendingAnimationDataComponent : IComponentData
{
	// animationTime, animationIndex, colorIndex, nan.
	public float Value;
}

}