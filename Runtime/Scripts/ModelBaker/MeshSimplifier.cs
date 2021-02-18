using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class MeshSimplifier
	{
		// Convert mesh into Triangles.
		// Change triangles.
		// Generate new mesh data based on Triangles.
	
		// Everything is basically done through the triangle.
		// When something changes in the triangle all correlated sub data changes as well (uv, normals, verts, etc).
	
		public class Triangle
		{
			// Vertices (Vector3)
			// Normals (Vector3)
			// UVs (UV0, UV1, ..., UV7)
			// Other...
	
			public Vector3[] vertices = new Vector3[3];
			public Vector3[] normals = new Vector3[3];
			public Dictionary<int, List<Vector2>> uvs = new Dictionary<int, List<Vector2>>();
	
			public float Perimeter()
			{
				return Vector3.Distance(vertices[0], vertices[1]) + Vector3.Distance(vertices[1], vertices[2]) + Vector3.Distance(vertices[2], vertices[0]);
			}
	
			// If two or more points have the same values the triangle has no surface area and will be 'zero'.
			public bool IsZero()
			{
				if (vertices[0] == vertices[1] || vertices[0] == vertices[2] || vertices[1] == vertices[2])
				{
					return true;
				}
	
				return false;
			}
	
			// Returns the closest vertex index of a vertex within this triangle.
			public int GetClosestVertexIndex(Vector3 vertex)
			{
				float distance = Mathf.Infinity;
				int closestVertex = -1;
	
				for (int v = 0; v < vertices.Length; v++)
				{
					if (vertices[v] != vertex)
					{
						float dist = Vector3.Distance(vertices[v], vertex);
	
						if (dist < distance)
						{
							distance = dist;
							closestVertex = v;
						}
					}
				}
	
				return closestVertex;
			}
	
			// Update triangle by copying data from a point in this triangle.
			public bool UpdateVertex(int curVertexIndex, int newVertexIndex)
			{
				vertices[curVertexIndex] = vertices[newVertexIndex];
				normals[curVertexIndex] = normals[newVertexIndex];
	
				foreach (var uv in uvs)
				{
					uv.Value[curVertexIndex] = uv.Value[newVertexIndex];
				}
	
				return true;
			}
	
			// Update triangle by copying data from an other triangle.
			public bool UpdateVertex(Triangle sourceTriangle, int sourceVertexIndex, int newSourceVertexIndex)
			{
				if (sourceTriangle != this)
				{
					Vector3 sourceVertex = sourceTriangle.vertices[sourceVertexIndex];
					int index = System.Array.IndexOf(vertices, sourceVertex);
	
					if (index != -1)
					{
						// Set all the new data.
						vertices[index] = sourceTriangle.vertices[newSourceVertexIndex];
						normals[index] = sourceTriangle.normals[newSourceVertexIndex];
	
						foreach (var uv in uvs)
						{
							uv.Value[index] = sourceTriangle.uvs[uv.Key][newSourceVertexIndex];
						}
	
						return true;
					}
				}
	
				return false;
			}
		}
	
		public static Mesh Simplify(this Mesh mesh, float quality)
		{
			string name = mesh.name;

			List<Triangle> triangles = mesh.ToTriangles();
	
			int targetCount = Mathf.FloorToInt(triangles.Count * quality);
			int loopCount = 0;

			while (triangles.Count > targetCount)
			{
				// Sort by perimeter.
				// TODO: Better priority system. Maybe allow user to pass in method.
				if (loopCount % triangles.Count == 0)
				{
					triangles.SortByPerimeter();
				}
	
				// Select tri/vert to simplify.
				int curTriIndex = 0;
				// TODO: Select vert by shortest total distance to the two other verts in the triangle.
				int curVertIndex = 0;
				Vector3 curVert = triangles[curTriIndex].vertices[curVertIndex];
	
				// Select closest vert within triangle to merge into.
				int newVertIndex = triangles[curTriIndex].GetClosestVertexIndex(curVert);
	
				// Update all triangles.
				// TODO: Apply only to connected triangles.
				for (int t = 0; t < triangles.Count; t++)
				{
					triangles[t].UpdateVertex(triangles[curTriIndex], curVertIndex, newVertIndex);
				}
				triangles[curTriIndex].UpdateVertex(curVertIndex, newVertIndex);
	
				// Remove all zero triangles.
				triangles.RemoveAll(t => t.IsZero());
	
				loopCount++;
			}
	
			mesh.Clear();
			mesh = triangles.ToMesh();
			mesh.name = name;
	
			return mesh;
		}
	
		public static List<Triangle> ToTriangles(this Mesh mesh)
		{
			List<Triangle> triangles = new List<Triangle>();

			Vector3[] verts = mesh.vertices;
			Vector3[] normals = mesh.normals;
			int[] tris = mesh.triangles;
	
			Dictionary<int, List<Vector2>> uvs = new Dictionary<int, List<Vector2>>();
			for (int u = 0; u < 8; u++)
			{
				List<Vector2> coordinates = new List<Vector2>();
				mesh.GetUVs(u, coordinates);
	
				if (coordinates != null && coordinates.Any())
				{
					uvs.Add(u, coordinates);
				}
			}
	
			for (int t = 0; t < tris.Length; t += 3)
			{
				Triangle tri = new Triangle();
	
				tri.vertices[0] = verts[tris[t + 0]];
				tri.vertices[1] = verts[tris[t + 1]];
				tri.vertices[2] = verts[tris[t + 2]];
	
				tri.normals[0] = normals[tris[t + 0]];
				tri.normals[1] = normals[tris[t + 1]];
				tri.normals[2] = normals[tris[t + 2]];

				foreach (var uv in uvs)
				{
					if (tri.uvs.TryGetValue(uv.Key, out List<Vector2> coordinates))
					{
						coordinates.Add(uv.Value[tris[t + 0]]);
						coordinates.Add(uv.Value[tris[t + 1]]);
						coordinates.Add(uv.Value[tris[t + 2]]);
					}
					else
					{
						tri.uvs.Add(uv.Key, new List<Vector2>
						{ 
							uv.Value[tris[t + 0]], 
							uv.Value[tris[t + 1]], 
							uv.Value[tris[t + 2]]
						});
					}
				}
	
				triangles.Add(tri);
			}
	
			return triangles;
		}
	
		public static Mesh ToMesh(this List<Triangle> triangles)
		{
			Mesh mesh = new Mesh();
			mesh.Clear();
	
			List<Vector3> vertices = new List<Vector3>(triangles.Count * 3);
			List<Vector3> normals = new List<Vector3>(triangles.Count * 3);
			List<int> tris = new List<int>(triangles.Count * 3);
			Dictionary<int, List<Vector2>> uvs = new Dictionary<int, List<Vector2>>();

			int skipped = 0;
			for (int t = 0; t < triangles.Count; t++)
			{
				for (int v = 0; v < triangles[t].vertices.Length; v++)
				{
					// Check for existing matching vert.
					int vIndex = vertices.IndexOf(triangles[t].vertices[v]);
					if (vIndex != -1)
					{
						// Check for existing matching normal.
						if (normals[vIndex] == triangles[t].normals[v])
						{
							// We have a duplicate.
							// Don't add the data and instead point to existing.
							tris.Add(vIndex);
							skipped++;
							continue;
						}
					}
	
					// Add data when it doesn't exist.
					vertices.Add(triangles[t].vertices[v]);
					normals.Add(triangles[t].normals[v]);
	
					foreach (var uv in triangles[t].uvs)
					{
						if (uvs.TryGetValue(uv.Key, out List<Vector2> coordinates))
						{
							coordinates.Add(uv.Value[v]);
						}
						else
						{
							uvs.Add(uv.Key, new List<Vector2>
							{
								uv.Value[v],
							});
						}
					}
	
					tris.Add(t * 3 + v - skipped);
				}
			}

			// Large mesh support.
			if (vertices.Count > 65535)
			{
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}

			mesh.vertices = vertices.ToArray();
			mesh.normals = normals.ToArray();
	
			foreach (var uv in uvs)
			{
				mesh.SetUVs(uv.Key, uv.Value);
			}

			mesh.triangles = tris.ToArray();
	
			mesh.Optimize();
			mesh.RecalculateBounds();
			mesh.RecalculateTangents();
	
			return mesh;
		}
	
		public static List<Triangle> SortByPerimeter(this List<Triangle> triangles)
		{
			triangles.Sort((x, y) => x.Perimeter().CompareTo(y.Perimeter()));
	
			return triangles;
		}
	}
}