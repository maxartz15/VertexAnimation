
#ifndef VERTEXANIMATIONUTILS_INCLUDED
#define VERTEX_ANIMATION_INCLUDED


#include "SampleTexture2DArrayLOD.hlsl"

void CALC_VA_UV_float(float2 uv, int maxFrames, float time, out float2 uvPosition)
{
	float timeInFrames = frac(time);
	timeInFrames = ceil(timeInFrames * maxFrames);
	timeInFrames /= maxFrames;
	timeInFrames += round(1.0f / maxFrames);

	uvPosition.x = uv.x;

	#ifdef VA_FLIP_UVS_ON
	uvPosition.y = (1.0f - (timeInFrames)) + (1.0f - (1.0f - uv.y));
	#else
	uvPosition.y = (1.0f - (1.0f - uv.y) - (1.0f - (timeInFrames)));
	#endif
}

float2 VA_UV_float(float2 uv, int maxFrames, float time)
{
	float2 uvPosition;
	float timeInFrames = frac(time);
	timeInFrames = ceil(timeInFrames * maxFrames);
	timeInFrames /= maxFrames;
	timeInFrames += round(1.0f / maxFrames);

	uvPosition.x = uv.x;

#ifdef VA_FLIP_UVS_ON
	uvPosition.y = (1.0f - (timeInFrames)) + (1.0f - (1.0f - uv.y));
#else
	uvPosition.y = (1.0f - (1.0f - uv.y) - (1.0f - (timeInFrames)));
#endif

	return uvPosition;
}

void VA_float(float2 uv, SamplerState texSampler, Texture2D positionMap, float time, int maxFrames,
	out float3 position, out float alpha)
{
	float2 uvPosition = VA_UV_float(uv, maxFrames, time);

	// Position.
	float4 texturePos = positionMap.SampleLevel(texSampler, uvPosition, 0);
	position = texturePos.xyz;

	// Normal.
	//FloatToFloat3_float(texturePos.w, outNormal);
	alpha = texturePos.w;
}

void VA_ARRAY_float(float2 uv, SamplerState texSampler, Texture2DArray positionMap, float positionMapIndex, float time, int maxFrames,
	out float3 position, out float alpha)
{
	float2 uvPosition = VA_UV_float(uv, maxFrames, time);

	// Position.
	float4 texturePos;
	SampleTexture2DArrayLOD_float(positionMap, uvPosition, texSampler, positionMapIndex, 0, texturePos);
	position = texturePos.xyz;

	// Normal.
	//FloatToFloat3_float(texturePos.w, outNormal);
	alpha = texturePos.w;
}

#endif