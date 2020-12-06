using Unity.Entities;
using Unity.Rendering;

namespace TAO.VertexAnimation
{
	[MaterialProperty("_AnimationIndex", MaterialPropertyFormat.Float)]
	public struct VA_AnimationIndexComponent : IComponentData
	{
		public float Value;
	}
	
	[MaterialProperty("_AnimationTime", MaterialPropertyFormat.Float)]
	public struct VA_AnimationTimeComponent : IComponentData
	{
		public float Value;
	}
}