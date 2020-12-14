using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation
{
	[System.Serializable]
	public struct VA_AnimationData
	{
		// The name of the animation.
		public FixedString32 name;
		// The frames in this animation.
		public int frames;
		// The maximum of frames the texture holds.
		public int maxFrames;
		// 1.0f / maxFrames.
		public float frameTime;
		// FrameTime * frames.
		public float duration;
		// The index of the related animation texture.
		public int animationMapIndex;
		// The index of the related color textures if/when added.
		public int colorMapIndex;
	}
	
	public struct VA_AnimationLibraryData
	{
		public BlobArray<VA_AnimationData> animations;
	}

    public static class VA_AnimationLibraryUtils
	{
        public static int GetAnimation(ref VA_AnimationLibraryData animationsRef, FixedString32 animationName)
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