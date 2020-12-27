using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using TAO.VertexAnimation;

public class SampleAnimationSystemsAuthoring : UnityEngine.MonoBehaviour
{
    [UnityEngine.SerializeField]
    private SampleSystem sampleSystem = SampleSystem.PlayRandomAnimation;

    [UnityEngine.SerializeField]
    private string animationName = "Character1Shoot";

    private void Awake()
    {
        switch (sampleSystem)
        {
            case SampleSystem.PlayRandomAnimation:
                {
                    var system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PlayRandomAnimationSystem>();
                    system.Enabled = true;
                }
                break;
            case SampleSystem.PlayAnimationByName:
                {
                    var system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<PlayAnimationByNameSystem>();
                    system.animationName = animationName;
                    system.Enabled = true;
                }
                break;
            default:
                break;
        }
    }

    public enum SampleSystem
    {
        PlayRandomAnimation,
        PlayAnimationByName
    }
}

// Example system to update animation parameters.
[UpdateBefore(typeof(VA_AnimatorSystem))]
public class PlayRandomAnimationSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();

        Enabled = false;
    }

    protected override void OnUpdate()
    {
        Random random = Random.CreateFromIndex((uint)UnityEngine.Time.time);
        float deltaTime = UnityEngine.Time.deltaTime;

        Entities.ForEach((ref VA_AnimatorComponent ac) =>
        {
            // Get the animation lib data.
            ref VA_AnimationLibraryData animationsRef = ref ac.animationLibrary.Value;

            // 'Play' the actual animation.
            ac.animationTime += deltaTime;

            if (ac.animationTime > animationsRef.animations[ac.animationIndex].duration)
            {
                // Set time. Using the difference to smoothen out animations when looping.
                ac.animationTime -= animationsRef.animations[ac.animationIndex].duration;

                // New random animation.
                ac.animationIndex = random.NextInt(0, animationsRef.animations.Length);
            }

        }).ScheduleParallel();
    }
}


// Example system to set the animation by name.
[UpdateBefore(typeof(VA_AnimatorSystem))]
public class PlayAnimationByNameSystem : SystemBase
{
    public FixedString32 animationName;

    protected override void OnCreate()
    {
        base.OnCreate();

        Enabled = false;
    }

    protected override void OnUpdate()
    {
        float deltaTime = UnityEngine.Time.deltaTime;
        FixedString32 an = animationName;

        Entities.ForEach((Entity entity, ref VA_AnimatorComponent ac) =>
        {
            // Get the animation lib data.
            ref VA_AnimationLibraryData animationsRef = ref ac.animationLibrary.Value;

            // Set the animation index on the AnimatorComponent to play this animation.
            ac.animationIndex = VA_AnimationLibraryUtils.GetAnimation(ref animationsRef, an);

            // 'Play' the actual animation.
            ac.animationTime += deltaTime;

            if (ac.animationTime > animationsRef.animations[ac.animationIndex].duration)
            {
                // Set time. Using the difference to smoothen out animations when looping.
                ac.animationTime -= animationsRef.animations[ac.animationIndex].duration;
            }
        }).ScheduleParallel();
    }
}