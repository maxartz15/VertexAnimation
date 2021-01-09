using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class VectorUtils
	{
		public static Vector2 Float3ToFloat2(this Vector3 f3)
		{
			Vector3 rotation = Vector3.Normalize(new Vector3(f3.x, 0, f3.z));
	
			Vector2 f2 = new Vector2();
			f2.x = Mathf.Acos(Vector3.Dot(rotation, new Vector3(1, 0, 0))) * Mathf.Sign(f3.z);
			f2.x = ((f2.x / Mathf.PI) + 1) * 0.5f;
	
			f2.y = Mathf.Acos(f3.y) / Mathf.PI;
	
			f2 *= 15;
			f2.x = Mathf.Round(f2.x);
			f2.y = Mathf.Round(f2.y);
	
			return f2;
		}
	
		public static float Float2ToFloat(this Vector2 f2)
		{
			return (f2.x + (16 * f2.y)) / 255;
		}
	
		public static float Float3ToFloat(this Vector3 f3)
		{
			return Float2ToFloat(Float3ToFloat2(f3));
		}
	}
}