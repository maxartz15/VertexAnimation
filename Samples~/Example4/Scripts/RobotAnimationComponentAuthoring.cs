using Unity.Entities;
using Unity.Collections;
using TAO.VertexAnimation;

namespace TAO.VertexAnimation.Example4
{
	public class RobotAnimationComponentAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
	{

		[UnityEngine.SerializeField]
		private VA_Animation anim;

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			RobotAnimationComponent sampleAnimationComponent = new RobotAnimationComponent
			{
				curAnimation = anim.GetName(),
				anim = anim.GetName(),
			};
			dstManager.AddComponentData(entity, sampleAnimationComponent);
		}
	}

	public struct RobotAnimationComponent : IComponentData
	{
		public FixedString64 curAnimation;
		public FixedString64 anim;
	}
}