// References: 
// https://forum.unity.com/threads/hdrp-shader-graph-hybrid-instanced-instancing-now-working.886063/

#ifndef SAMPLE_TEXTURE2D_ARRAY_INCLUDED
#define SAMPLE_TEXTURE2D_ARRAY_INCLUDED

void SampleTexture2DArrayLOD_float(Texture2DArray TextureArray, float2 UV, SamplerState Sampler, uint Index, uint LOD, out float4 RGBA)
{
	RGBA = SAMPLE_TEXTURE2D_ARRAY_LOD(TextureArray, Sampler, UV, Index, LOD);
}

#endif