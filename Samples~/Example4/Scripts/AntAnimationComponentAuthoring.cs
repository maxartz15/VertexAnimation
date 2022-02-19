using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation.Example4
{
	public class AntAnimationComponentAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
	{
		[UnityEngine.SerializeField]
		private VA_Animation idle;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			AntAnimationComponent sampleAnimationComponent = new AntAnimationComponent
			{
				curAnimation = idle.GetName(),
				idle = idle.GetName(),




			};
			dstManager.AddComponentData(entity, sampleAnimationComponent);
		}
	}

	public struct AntAnimationComponent : IComponentData
	{
		public FixedString64 curAnimation;
		public FixedString64 idle;
    }
}