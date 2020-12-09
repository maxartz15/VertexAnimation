using Unity.Entities;
using Unity.Collections;

namespace TAO.VertexAnimation
{
	[System.Serializable]
	public struct VA_AnimationData
	{
		public FixedString32 name;
		public int frames;
		public int maxFrames;
		// 1.0f / maxFrames.
		public float frameTime;
		// frameTime * frames.
		public float duration;
	}
	
	public struct VA_AnimationLibrary
	{
		public BlobArray<VA_AnimationData> animations;
	}

    public static class VA_AnimationLibraryUtils
	{
		public static int GetAnimation(ref VA_AnimationLibrary animationsRef, FixedString32 animationName)
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
	}
}