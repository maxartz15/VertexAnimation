using System.Collections.Generic;
using Unity.Collections;
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

#if UNITY_EDITOR
		[SerializeField]
		private List<VA_Animation> loadedAnimationsPreview = null;
#endif

		public Unity.Entities.Hash128 key;
		public string guid;

        private void Awake()
        {
			 

		}
        public void Init()
		{
			// generate guid
			guid = System.Guid.NewGuid().ToString("N");
			key = new Unity.Entities.Hash128(guid);

			animationData = new List<VA_AnimationData>();
			

			foreach (VA_AnimationBook book in animationBooks)
			{
				book.UpdateMaterials();

				if (book != null)
				{
					foreach (VA_Animation animation in book.animations)
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
			Dictionary<string, VA_Animation> usedNames = new Dictionary<string, VA_Animation>();

			foreach (VA_AnimationBook book in animationBooks)
			{
				if (book != null)
				{
					foreach (VA_Animation animation in book.animations)
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
			loadedAnimationsPreview = new List<VA_Animation>();
			foreach (var un in usedNames)
			{
				loadedAnimationsPreview.Add(un.Value);
			}
#endif
		}
	}
}