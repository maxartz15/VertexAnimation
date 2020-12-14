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
				foreach (var ab in animationBooks)
				{
					animations.AddRange(ab.GetAnimationData());
				}
			}
		}
	}
}