using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation
{
	[System.Serializable]
	public struct VA_AnimationData
	{
		public VA_AnimationData(FixedString64Bytes a_name, int a_frames, int a_maxFrames, int a_fps, int a_positionMapIndex, int a_colorMapIndex = -1)
		{
			name = a_name;
			frames = a_frames;
			maxFrames = a_maxFrames;
			animationMapIndex = a_positionMapIndex;
			colorMapIndex = a_colorMapIndex;
			frameTime = 1.0f / a_maxFrames * a_fps;
			duration = 1.0f / a_maxFrames * (a_frames - 1);
		}

		// The name of the animation.
		public FixedString64Bytes name;
		// The frames in this animation.
		public int frames;
		// The maximum of frames the texture holds.
		public int maxFrames;
		// The index of the related animation texture.
		public int animationMapIndex;
		// The index of the related color textures if/when added.
		public int colorMapIndex;
		// Time of a single frame.
		public float frameTime;
		// Total time of the animation.
		public float duration;
	}
	
	public struct VA_AnimationLibraryData
	{
		public BlobArray<VA_AnimationData> animations;
	}

	public static class VA_AnimationLibraryUtils
	{
		public const string AnimationLibraryAssetStoreName = "VA_AnimationLibrary";

		public static int GetAnimation(ref VA_AnimationLibraryData animationsRef, FixedString64Bytes animationName)
		{
			for (int i = 0; i < animationsRef.animations.Length; i++)
			{
				if (animationsRef.animations[i].name == animationName)
				{
					return i;
				}
			}

			return -1;
		}

		public static int GetAnimationMapIndex(ref VA_AnimationLibraryData animationsRef, int animation)
		{
			return animationsRef.animations[animation].animationMapIndex;
		}

		public static int GetColorMapIndex(ref VA_AnimationLibraryData animationsRef, int animation)
		{
			return animationsRef.animations[animation].colorMapIndex;
		}
	}
}