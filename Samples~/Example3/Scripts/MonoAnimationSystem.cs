using TAO.VertexAnimation;
using UnityEngine;

public class MonoAnimationSystem : MonoBehaviour
{
	[SerializeField]
	private VA_Animation[] animations = null;

	private VA_Animation curAnimation = null;
	private float animationTime;

	private MeshRenderer[] meshRenderers = null;

	private void Awake()
	{
		if (animations == null)
		{
			enabled = false;
			Debug.LogWarning("No animations added!");
			return;
		}

		curAnimation = animations[0];
		meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
	}

	private void Update()
	{
		PlayAnimation();
		InterpolationData interpolationData = GetInterpolationData(curAnimation, animationTime);
		UpdateMaterials(interpolationData);
	}

	// This is playing the animation.
	private void PlayAnimation()
	{
		animationTime += Time.deltaTime * curAnimation.Data.frameTime;

		if (animationTime > curAnimation.Data.duration)
		{
			animationTime = 0;

			int newAnimation = Random.Range(0, animations.Length);
			curAnimation = animations[newAnimation];
		}
	}

	// This is something like what the hybrid renderer does with the VA_AnimationDataComponent.
	private void UpdateMaterials(InterpolationData interpolationData)
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetVector("_AnimationData", new Vector4(animationTime, curAnimation.Data.animationMapIndex, interpolationData.animationTime, interpolationData.animationMapIndex));

		foreach (var mr in meshRenderers)
		{
			if (mr.enabled && mr.gameObject.activeSelf)
			{
				mr.SetPropertyBlock(materialPropertyBlock);
			}
		}
	}

	// This is resembles the VA_AnimatorSystem.
	private static InterpolationData GetInterpolationData(VA_Animation animation, float animationTime)
	{
		InterpolationData data = new InterpolationData();

		// Calculate next frame time for lerp.
		float animationTimeNext = animationTime + (1.0f / animation.Data.maxFrames);
		if (animationTimeNext > animation.Data.duration)
		{
			// Set time. Using the difference to smooth out animations when looping.
			animationTimeNext -= animationTime;
		}

		data.animationTime = animationTimeNext;
		data.animationMapIndex = animation.Data.animationMapIndex;

		return data;
	}

	public struct InterpolationData
	{
		public float animationTime;
		public int animationMapIndex;
	}
}