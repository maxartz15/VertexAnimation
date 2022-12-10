using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class MeshUtils
	{
		// Copy a mesh and it's properties.
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
				bounds = mesh.bounds,
				uv = mesh.uv,
				uv2 = mesh.uv2,
				uv3 = mesh.uv3,
				uv4 = mesh.uv4,
				uv5 = mesh.uv5,
				uv6 = mesh.uv6,
				uv7 = mesh.uv7,
				uv8 = mesh.uv8,
			};

			return copy;
		}

		// Optimize the mesh and upload the mesh data, makes the mesh no longer readable.
		public static void Finalize(this Mesh mesh)
		{
			mesh.Optimize();
			mesh.UploadMeshData(true);
			
		}
	}
}
