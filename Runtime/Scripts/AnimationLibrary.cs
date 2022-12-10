using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace TAO.VertexAnimation
{
	[CreateAssetMenu(fileName = "new AnimationLibrary", menuName = "TAO/VertexAnimation/AnimationLibrary", order = 400)]
	public class AnimationLibrary : ScriptableObject
	{
		[SerializeField]
		private List<AnimationBook> animationBooks = new List<AnimationBook>();

		[HideInInspector]
		public List<VA_AnimationData> animationData = null;

#if UNITY_EDITOR
		[SerializeField]
		private List<Animation> loadedAnimationsPreview = null;
#endif

		public void Init()
		{
			animationData = new List<VA_AnimationData>();

			foreach (AnimationBook book in animationBooks)
			{
				book.UpdateMaterials();

				if (book != null)
				{
					foreach (Animation animation in book.animations)
					{
						// TODO: Fix data name, FixedString32 doesn't transfer from editor?
						//animation.Data.name = new FixedString32(animation.name);
						animationData.Add(animation.GetData());
					}
				}
			}
		}

		public void OnValidate()
		{
			Dictionary<string, Animation> usedNames = new Dictionary<string, Animation>();

			foreach (AnimationBook book in animationBooks)
			{
				if (book != null)
				{
					foreach (Animation animation in book.animations)
					{
						if (!usedNames.ContainsKey(animation.name))
						{
							usedNames.Add(animation.name, animation);
						}
						else
						{
							Debug.LogWarning(string.Format("Naming conflict found in {0} - Animation {1} and {2} have the same name!", name, usedNames[animation.name].name, animation.name));
						}
					}
				}
			}

#if UNITY_EDITOR
			loadedAnimationsPreview = new List<Animation>();
			foreach (var un in usedNames)
			{
				loadedAnimationsPreview.Add(un.Value);
			}
#endif
		}
	}
}