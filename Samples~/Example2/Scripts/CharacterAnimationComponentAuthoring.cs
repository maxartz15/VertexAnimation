using Unity.Entities;
using Unity.Collections;
using TAO.VertexAnimation;

public class CharacterAnimationComponentAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
	[UnityEngine.SerializeField]
	private VA_Animation idle;
	[UnityEngine.SerializeField]
	private VA_Animation run;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		CharacterAnimationComponent sampleAnimationComponent = new CharacterAnimationComponent
		{
			curAnimation = idle.GetName(),
			idle = idle.GetName(),
			run = run.GetName()
		};
		dstManager.AddComponentData(entity, sampleAnimationComponent);
	}
}

public struct CharacterAnimationComponent : IComponentData
{
	public FixedString64 curAnimation;
	public FixedString64 idle;
	public FixedString64 run;
}