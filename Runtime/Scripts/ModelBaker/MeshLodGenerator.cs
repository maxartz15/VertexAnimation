using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class MeshLodGenerator
	{
		public static Mesh[] GenerateLOD(this Mesh mesh, int lods, float[] quality)
		{
			Mesh[] lodMeshes = new Mesh[lods];
			
			for (int lm = 0; lm < lodMeshes.Length; lm++)
			{
				lodMeshes[lm] = mesh.Copy();
				// Only simplify when needed.
				if (quality[lm] < 1.0f)
				{
					lodMeshes[lm] = lodMeshes[lm].Simplify(quality[lm]);
				}

				lodMeshes[lm].name = string.Format("{0}_LOD{1}", lodMeshes[lm].name, lm);
			}

			return lodMeshes;
		}

		public static Mesh[] GenerateLOD(this Mesh mesh, int lods, AnimationCurve qualityCurve)
		{
			float[] quality = new float[lods];

			for (int q = 0; q < quality.Length; q++)
			{
				quality[q] = qualityCurve.Evaluate(1f / quality.Length * q);
			}

			return GenerateLOD(mesh, lods, quality);
		}
	}
}