using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial class SpawnerSystem : SystemBase
{
	private EntityCommandBufferSystem enityCommandBufferSystem;

	protected override void OnCreate()
	{
		enityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected override void OnUpdate()
	{
		var commandBuffer = enityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

		Entities.ForEach((Entity entity, int entityInQueryIndex, ref SpawnerComponent spawner, ref LocalToWorld location) =>
		{
			for (var x = 0; x < spawner.countX; x++)
			{
				for (var y = 0; y < spawner.countY; y++)
				{
					for (int z = 0; z < spawner.countZ; z++)
					{
						var instance = commandBuffer.Instantiate(entityInQueryIndex, spawner.entity);

						// Place the instantiated in a grid with some noise
						var position = math.transform(location.Value, new float3(x * spawner.spaceX, y * spawner.spaceY, z * spawner.spaceZ));
						commandBuffer.SetComponent(entityInQueryIndex, instance, new Translation { Value = position });
					}
				}
			}

			commandBuffer.DestroyEntity(entityInQueryIndex, entity);
		})
		.WithName("SpawnerSystem")
		.ScheduleParallel();

		enityCommandBufferSystem.AddJobHandleForProducer(Dependency);
	}
}
