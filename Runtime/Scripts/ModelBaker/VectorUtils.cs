using UnityEngine;

namespace TAO.VertexAnimation
{
	public static class VectorUtils
	{
		#region Custom Packing
		// Encode.
		public static Vector2 Float3ToFloat2(this Vector3 f3)
		{
			Vector3 rotation = Vector3.Normalize(new Vector3(f3.x, 0, f3.y));
	
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

		// Decode.
		public static Vector2 FloatToFloat2(float f1)
		{
			f1 *= 256;

			Vector2 f2;
			f2.x = (f1 % 16) / 15;
			f2.y = ((f1 / 256) * 16);
			f2.y = Mathf.Floor(f2.y) / 15;

			return f2;
		}

		public static Vector3 Float2ToFloat3(Vector2 f2)
		{
			float dist = 1 - Mathf.Abs((f2.y - 0.5f) * 2);
			float temp = (f2.x * (Mathf.PI * 2)) - Mathf.PI;

			Vector3 f3;
			f3.x = Mathf.Sin(temp + (Mathf.PI * 2)) * dist;
			f3.z = Mathf.Cos((temp - Mathf.PI) + (Mathf.PI * 2)) * dist;

			f3.y = (f2.y - 0.5f) * -2;

			f3 = f3.normalized;

			return f3;
		}

		public static Vector3 FloatToFloat3(float f1)
		{
			return Float2ToFloat3(FloatToFloat2(f1));
		}
		#endregion

		#region Houdini Style Packing
		// Encode.
		public static float EncodeFloat3ToFloat1(Vector3 f3)
		{
			float f1;

			float z = Mathf.Sqrt(f3.z * 8 + 8);
			float y = (f3.y / z + 0.5f) * 31;
			float x = Mathf.Floor((f3.x / z + 0.5f) * 31) * 32;
			f1 = (x + y) / 1023;

			return f1;
		}

		// Decode.
		public static Vector2 DecodeFloat1ToFloat2(float f1)
		{
			Vector2 f2;

			f1 *= 1024;
			f2.x = Mathf.Floor(f1 / 32.0f) / 31.5f;
			f2.y = (f1 - (Mathf.Floor(f1 / 32.0f) * 32.0f)) / 31.5f;

			return f2;
		}

		public static Vector3 DecodeFloat2ToFloat3(Vector2 f2)
		{
			Vector3 f3;

			f2 *= 4;
			f2.x -= 2;
			f2.y -= 2;

			float f2dot = Vector3.Dot(f2, f2);

			f3.x = Mathf.Sqrt(1 - (f2dot / 4.0f)) * f2.x;
			f3.y = Mathf.Sqrt(1 - (f2dot / 4.0f)) * f2.y;
			f3.z = 1 - (f2dot / 2.0f);

			//f3.x = Mathf.Clamp(f3.x, -1.0f, 1.0f);
			//f3.y = Mathf.Clamp(f3.x, -1.0f, 1.0f);
			//f3.z = Mathf.Clamp(f3.x, -1.0f, 1.0f);
			f3 = Vector3.ClampMagnitude(f3, 1);

			return f3;
		}

		public static Vector3 DecodeFloat1ToFloat3(float f1)
		{
			return DecodeFloat2ToFloat3(DecodeFloat1ToFloat2(f1));
		}
		#endregion
	}
}