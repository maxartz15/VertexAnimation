using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
	[CreateAssetMenu(fileName = "new AnimationLibrary", menuName = "TAO/VertexAnimation/AnimationLibrary", order = 400)]
	public class VA_AnimationLibrary : ScriptableObject
	{
		[SerializeField]
		private List<VA_AnimationBook> animationBooks = new List<VA_AnimationBook>();

		[HideInInspector]
		public List<VA_AnimationData> animations = null;

		public void Init()
		{
			animations = new List<VA_AnimationData>();

			foreach (VA_AnimationBook ab in animationBooks)
			{
				ab.SetMaterials();
				animations.AddRange(ab.playData.GetAnimations);
			}
		}

		private void OnValidate()
		{
			// TODO: Check for naming conflicts in AnimationBooks.
		}
	}
}