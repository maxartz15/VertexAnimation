
#ifndef VECTOR_ENCODING_DECODING_INCLUDED
#define VECTOR_ENCODING_DECODING_INCLUDED

#define V_PI		3.14159265359f
#define V_TWO_PI	6.28318530718f
#define V_HALF_PI   1.57079632679f

// Custom Packing.
// Encode float3 to 0..1 float.
void Float3ToFloat2_float(float3 f3, out float2 f2)
{
	//float3 rotation = normalize(float3(f3.x, 0, f3.y));
	float3 rotation = normalize(float3(f3.x, 0, f3.z));

	f2.x = acos(dot(rotation, float3(1, 0, 0))) * sign(f3.z);
	f2.x = ((f2.x / V_PI) + 1) * 0.5f;

	f2.y = acos(f3.y) / V_PI;

	f2 *= 15;
	f2.x = round(f2.x);
	f2.y = round(f2.y);
}

void Float2ToFloat_float(float2 f2, out float f1)
{
	f1 = (f2.x + (16 * f2.y)) / 255;
}

void Float3ToFloat_float(float3 f3, out float f1)
{
	float2 f2;
	Float3ToFloat2_float(f3, f2);
	Float2ToFloat_float(f2, f1);
}

// Decode 0..1 float.
void FloatToFloat2_float(float f1, out float2 f2)
{
	f1 *= 256;

	f2.x = (f1 % 16) / 15;
	f2.y = ((f1 / 256) * 16);
	f2.y = floor(f2.y) / 15;
}

void Float2ToFloat3_float(float2 f2, out float3 f3)
{
	float dist = 1 - abs((f2.y - 0.5f) * 2);
	float temp = (f2.x * V_TWO_PI) - V_PI;

	f3.x = sin(temp + V_TWO_PI) * dist;
	f3.z = cos((temp - V_PI) + V_TWO_PI) * dist;

	f3.y = (f2.y - 0.5f) * -2;

	f3 = normalize(f3);
}

void FloatToFloat3_float(float f1, out float3 f3)
{
	float2 f2;
	FloatToFloat2_float(f1, f2);
	Float2ToFloat3_float(f2, f3);
}

// Houdini Style Packing.
// Encode.
void EncodeFloat3ToFloat1_float(float3 f3, out float f1)
{
	float z = sqrt(f3.z * 8 + 8);
	float y = (f3.y / z + 0.5f) * 31;
	float x = floor((f3.x / z + 0.5f) * 31) * 32;
	f1 = (x + y) / 1023;
}

// Decode.
void DecodeFloat1ToFloat2_float(float f1, out float2 f2)
{
	f1 *= 1024;
	f2.x = floor(f1 / 32.0) / 31.5;
	f2.y = (f1 - (floor(f1 / 32.0) * 32.0)) / 31.5;
}

void DecodeFloat2ToFloat3_float(float2 f2, out float3 f3)
{
	f2 *= 4;
	f2 -= 2;
	float f2dot = dot(f2, f2);
	f3.xy = sqrt(1 - (f2dot / 4.0)) * f2;
	f3.z = 1 - (f2dot / 2.0);
	f3 = clamp(f3, -1.0, 1.0);
}

void DecodeFloat1ToFloat3_float(float f1, out float3 f3)
{
	float2 f2;
	DecodeFloat1ToFloat2_float(f1, f2);
	DecodeFloat2ToFloat3_float(f2, f3);
}

// Test Packing.
// Ref:
// https://answers.unity.com/questions/733677/cg-shader-float3-to-float-packunpack-functions.html
// http://aras-p.info/blog/2009/07/30/encoding-floats-to-rgba-the-final/
void Encode2Float3ToFloat1_float(float3 f3, out float f1)
{
	f1 = (dot(round((f3) * 255), float3(65536, 256, 1)));
}

void Decode2Float1ToFloat3_float(float f1, out float3 f3)
{
	f3 = (frac((f1) / float3(16777216, 65536, 256)));
}

#endif