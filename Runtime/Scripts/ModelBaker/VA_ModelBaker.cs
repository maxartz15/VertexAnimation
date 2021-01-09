using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
	[CreateAssetMenu(fileName = "new ModelBaker", menuName = "VA_ModelBaker/ModelBaker", order = 400)]
	public class VA_ModelBaker : ScriptableObject
	{
		public GameObject model;
		public AnimationClip[] animationClips;
		public int fps = 24;
		public int textureWidth = 512;
		public bool saveBakedDataToAsset = true;
		public bool generateAnimationBook = false;

		[SerializeField]
		private AnimationBaker.BakedData bakedData;
		public AnimationBaker.BakedData BakedData
        {
			get
			{
				return bakedData;
			}
        }

        public void Bake()
		{
			var target = Instantiate(model);

			target.ConbineAndConvertGameObject();
			bakedData = target.Bake(animationClips, fps, textureWidth);

			DestroyImmediate(target);
		}
	}
}