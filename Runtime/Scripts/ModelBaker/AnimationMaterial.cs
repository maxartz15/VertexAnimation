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

		public static Material Create(string name, Shader shader, Texture2DArray positionMap, bool useNormalA, bool useInterpolation, int maxFrames)
		{
			Material material = Create(name, shader);

			material.Update(name, shader, positionMap, useNormalA, useInterpolation, maxFrames);

			return material;
		}

		public static void Update(this Material material, string name, Shader shader, Texture2DArray positionMap, bool useNormalA, bool useInterpolation, int maxFrames)
		{
			material.name = name;

			if (material.shader != shader)
			{
				material.shader = shader;
			}

			material.SetTexture("_PositionMap", positionMap);
			material.SetInt("_MaxFrames", maxFrames);

			if (useNormalA)
			{
				material.EnableKeyword("USE_NORMALA_ON");
			}
			else
			{
				material.DisableKeyword("USE_NORMALA_ON");
			}

			if (useInterpolation)
			{
				material.EnableKeyword("USE_INTERPOLATION_ON");
			}
			else
			{
				material.DisableKeyword("USE_INTERPOLATION_ON");
			}
		}
	}
}