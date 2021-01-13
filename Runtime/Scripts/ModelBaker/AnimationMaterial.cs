using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class AnimationMaterial
	{
		public static Material Create(string name, Shader shader)
		{
			Material material = new Material(shader)
			{
				name = name,
				enableInstancing = true
			};

			return material;
		}
	}
}