using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation.Example4
{

	public class ZombieAnimationComponentAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
	{
		[UnityEngine.SerializeField]
		private VA_Animation idle;
		[UnityEngine.SerializeField]
		private VA_Animation run;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			ZombieAnimationComponent sampleAnimationComponent = new ZombieAnimationComponent
			{
				curAnimation = idle.GetName(),
				idle = idle.GetName(),
				run = run.GetName()
			};
			dstManager.AddComponentData(entity, sampleAnimationComponent);
		}
	}

	public struct ZombieAnimationComponent : IComponentData
	{
		public FixedString64 curAnimation;
		public FixedString64 idle;
		public FixedString64 run;
	}
}