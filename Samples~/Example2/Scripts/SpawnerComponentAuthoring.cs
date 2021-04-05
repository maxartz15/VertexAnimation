using Unity.Entities;

[UnityEngine.RequireComponent(typeof(ConvertToEntity))]
public class SpawnerComponentAuthoring : UnityEngine.MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	public UnityEngine.GameObject prefab;
	public int countX = 1;
	public int countY = 1;
	public int countZ = 1;
	public int spaceX = 1;
	public int spaceY = 1;
	public int spaceZ = 1;

	// Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
	public void DeclareReferencedPrefabs(System.Collections.Generic.List<UnityEngine.GameObject> gameObjects)
	{
		gameObjects.Add(prefab);
	}

	// Lets you convert the editor data representation to the entity optimal runtime representation
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		var spawnerData = new SpawnerComponent
		{
			// The referenced prefab will be converted due to DeclareReferencedPrefabs.
			// So here we simply map the game object to an entity reference to that prefab.
			entity = conversionSystem.GetPrimaryEntity(prefab),
			countX = countX,
			countY = countY,
			countZ = countZ,
			spaceX = spaceX,
			spaceY = spaceY,
			spaceZ = spaceZ
		};
		dstManager.AddComponentData(entity, spawnerData);
	}
}

public struct SpawnerComponent : IComponentData
{
	public Entity entity;
	public int countX;
	public int countY;
	public int countZ;
	public int spaceX;
	public int spaceY;
	public int spaceZ;
}