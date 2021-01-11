using UnityEngine;

namespace TAO.VertexAnimation
{
	[CreateAssetMenu(fileName = "new ModelBaker", menuName = "VA_ModelBaker/ModelBaker", order = 400)]
	public class VA_ModelBaker : ScriptableObject
	{
		public GameObject model;
		public AnimationClip[] animationClips;
		[Range(1, 60)]
		public int fps = 24;
		public int textureWidth = 512;

#if UNITY_EDITOR
		public bool saveBakedDataToAsset = true;
		public bool generateAnimationBook = true;
		public bool generatePrefab = true;
		public Shader materialShader = null;

		public GameObject prefab = null;
		public Material material = null;
		public VA_AnimationBook book = null;
#endif

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