using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace TAO.VertexAnimation
{
	[MaterialProperty("_AnimationData", MaterialPropertyFormat.Float4)]
	public struct VA_AnimationDataComponent : IComponentData
	{
		// animationTime, animationIndex, colorIndex, nan.
		public float4 Value;
	}
}