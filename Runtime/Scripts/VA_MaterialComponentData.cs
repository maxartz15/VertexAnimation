using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace TAO.VertexAnimation
{
[MaterialProperty("_AnimationData")] //, MaterialPropertyFormat.Float4
public struct VaAnimationDataComponent : IComponentData
{
	// animationTime, animationIndex, colorIndex, nan.
	public float4 Value;
}


}