using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
	[CreateAssetMenu(fileName = "new AnimationLibrary", menuName = "AnimationLibrary", order = 0)]
	public class VA_AnimationLibrary : ScriptableObject
	{
		[SerializeField]
		private VA_AnimationBook[] animationBooks;

		[HideInInspector]
		public List<VA_AnimationData> animations = null;

		public void Create()
		{
			foreach (VA_AnimationBook book in animationBooks)
			{
				book.Create();
			}

			ConvertAnimations();
		}

		private void OnValidate()
		{
			// TODO: Check for naming conflicts in AnimationBooks.
		}

		private void ConvertAnimations()
		{
			animations = new List<VA_AnimationData>();

			if (animationBooks != null)
			{
				for (int b = 0; b < animationBooks.Length; b++)
				{
					if(animationBooks[b].animationPages != null)
					{
                        for (int p = 0; p < animationBooks[b].animationPages.Count; p++)
                        {
							animations.Add(new VA_AnimationData
							{
								name = new Unity.Collections.FixedString32(animationBooks[b].animationPages[p].name),
								maxFrames = animationBooks[b].maxFrames,
								frames = animationBooks[b].animationPages[p].frames,
								frameTime = 1.0f / animationBooks[b].maxFrames,
								duration = 1.0f / animationBooks[b].maxFrames * animationBooks[b].animationPages[p].frames
							});
                        }
                    }
				}
			}
		}
	}
}