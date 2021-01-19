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
		public List<VA_AnimationData> animationData = null;

		public void Init()
		{
			animationData = new List<VA_AnimationData>();

			foreach (VA_AnimationBook book in animationBooks)
			{
				book.SetMaterials();

				foreach (VA_Animation animation in book.animations)
				{
					animationData.Add(animation.Data);
				}
			}
		}

		private void OnValidate()
		{
			// TODO: Check for naming conflicts in AnimationBooks.
		}
	}
}