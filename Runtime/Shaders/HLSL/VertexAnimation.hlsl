
#ifndef VERTEXANIMATIONUTILS_INCLUDED
#define VERTEX_ANIMATION_INCLUDED

#include "VectorEncodingDecoding.hlsl"
#include "SampleTexture2DArrayLOD.hlsl"

float2 VA_UV_float(float2 uv, int maxFrames, float time)
{
	float2 uvPosition;
	
	float timeInFrames = frac(time);
	timeInFrames = ceil(timeInFrames * maxFrames);
	timeInFrames /= maxFrames;
	timeInFrames += (1 / maxFrames);

	uvPosition.x = uv.x;
	uvPosition.y = (1 - (timeInFrames)) + (1 - (1 - uv.y));

	return uvPosition;
}

void VA_float(float2 uv, SamplerState texSampler, Texture2D positionMap, float time, int maxFrames,
	out float3 outPosition, out float3 outNormal)
{
	float2 uvPosition = VA_UV_float(uv, maxFrames, time);

	// Position.
	float4 texturePos = positionMap.SampleLevel(texSampler, uvPosition, 0);
	outPosition = texturePos.xyz;

	// Normal.
	DecodeFloat1ToFloat3_float(texturePos.w, outNormal);
}

void VA_ARRAY_float(float2 uv, SamplerState texSampler, Texture2DArray positionMap, float positionMapIndex, float time, int maxFrames,
	out float3 outPosition, out float3 outNormal)
{
	float2 uvPosition = VA_UV_float(uv, maxFrames, time);

	// Position.
	float4 texturePos;
	SampleTexture2DArrayLOD_float(positionMap, uvPosition, texSampler, positionMapIndex, 0, texturePos);
	outPosition = texturePos.xyz;

	// Normal.
	DecodeFloat1ToFloat3_float(texturePos.w, outNormal);
}

#endif