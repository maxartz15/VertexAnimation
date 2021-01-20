using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class MeshUtils
	{
		public static Mesh Copy(this Mesh mesh)
		{
			Mesh copy = new Mesh
			{
				name = mesh.name,
				indexFormat = mesh.indexFormat,
				vertices = mesh.vertices,
				triangles = mesh.triangles,
				normals = mesh.normals,
				tangents = mesh.tangents,
				colors = mesh.colors,
				uv = mesh.uv,
				uv2 = mesh.uv2,
				uv3 = mesh.uv3,
				uv4 = mesh.uv4,
				uv5 = mesh.uv5,
				uv6 = mesh.uv6,
				uv7 = mesh.uv7,
				uv8 = mesh.uv8
			};

			copy.RecalculateBounds();

			return copy;
		}
	}
}
