using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace TAO.VertexAnimation
{
	//public class VA_AnimatorSystem : SystemBase
	//{
	//    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;
	
	//    protected override void OnCreate()
	//    {
	//        base.OnCreate();
	
	//        endSimulationEntityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
	//    }
	
	//    protected override void OnUpdate()
	//    {
	//        var ecb = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
	
	//        Entities.ForEach((Entity entity, int entityInQueryIndex, in VA_AnimatorComponent ac, in DynamicBuffer<Child> children) =>
	//        {
	//            for (int i = 0; i < children.Length; i++)
	//            {
	//                // Get child.
	//                Entity child = children[i].Value;
	
	//                // Overwrite existing component.
	//                ecb.AddComponent(entityInQueryIndex, child, new VA_AnimationTimeComponent { Value = ac.animationTime });
	//                ecb.AddComponent(entityInQueryIndex, child, new VA_AnimationIndexComponent { Value = ac.animationIndex });
	//            }
	//        })
	//        .ScheduleParallel();
	
	//        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
	//    }
	//}
	
	//[UpdateBefore(typeof(HybridRendererSystem))]
	public class VA_AnimatorSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			var atc = GetComponentDataFromEntity<VA_AnimationTimeComponent>(false);
	        var aic = GetComponentDataFromEntity<VA_AnimationIndexComponent>(false);
	
			Entities.ForEach((ref VA_AnimatorComponent ac, in DynamicBuffer<Child> children) =>
	        {
	            for (int i = 0; i < children.Length; i++)
	            {
	                // Get child.
	                Entity child = children[i].Value;
	                
			        // Get a copy of the time Component of the child.
			        VA_AnimationTimeComponent atcCopy = atc[child];
			        // Set new value.
			        atcCopy.Value = ac.animationTime;
			        // Update original.
			        atc[child] = atcCopy;
	                
			        VA_AnimationIndexComponent aicCopy = aic[child];
			        aicCopy.Value = ac.animationIndex;
			        aic[child] = aicCopy;
	            }
	        })
	        .Run();
	    }
	}
	
	public class VA_AnimatorSystem2 : SystemBase
	{
	    protected override void OnCreate()
	    {
	        base.OnCreate();
	
			Enabled = false;
	    }
	
	    protected override void OnUpdate()
		{
			Entities.ForEach((ref VA_AnimatorComponent ac, in DynamicBuffer<Child> children) =>
			{
				for (int i = 0; i < children.Length; i++)
				{
					// Get child.
					Entity child = children[i].Value;
					
					//if(HasComponent<VA_AnimationTimeComponent>(child))
					//{
	                    var atc = GetComponent<VA_AnimationTimeComponent>(child);
	                    atc.Value = ac.animationTime;
						SetComponent(child, atc);
	                //}
					
					//if(HasComponent<VA_AnimationIndexComponent>(child))
					//{
						var aic = GetComponent<VA_AnimationIndexComponent>(child);
						aic.Value = ac.animationIndex;
						SetComponent(child, aic);
					//}
				}
			})
			.Run();
		}
	}
	
	[UpdateBefore(typeof(VA_AnimatorSystem))]
	public class VA_AnimationTimeSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			float time = UnityEngine.Time.deltaTime;
			
			Entities.ForEach((ref VA_AnimatorComponent ac) =>
			{
				ac.animationTime += time;
			}).ScheduleParallel();
		}
	}
	
	[UpdateBefore(typeof(VA_AnimatorSystem))]
	public class VA_AnimationIndexSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			Entities.ForEach((Entity entity, ref VA_AnimatorComponent ac) =>
			{
				int index = entity.Index % 2;
				ac.animationIndex = index;
			}).ScheduleParallel();
		}
	}
}