
#ifndef VECTOR_ENCODING_DECODING_INCLUDED
#define VECTOR_ENCODING_DECODING_INCLUDED

// Ref: SideFX.
void DecodeFloat1ToFloat3_float(float f1, out float3 f3)
{
	//decode float to float2
	f1 *= 1024;
	float2 f2;
	f2.x = floor(f1 / 32.0) / 31.5;
	f2.y = (f1 - (floor(f1 / 32.0) * 32.0)) / 31.5;

	//decode float2 to float3
	f2 *= 4;
	f2 -= 2;
	float f2dot = dot(f2, f2);
	f3.xy = sqrt(1 - (f2dot / 4.0)) * f2;
	f3.z = 1 - (f2dot / 2.0);
	f3 = clamp(f3, -1.0, 1.0);
}

void EncodeFloat3ToFloat1_float(float3 f3, out float f1)
{
	float z = sqrt(f3.z * 8 + 8);
	float y = (f3.y / z + 0.5f) * 31;
	float x = floor((f3.x / z + 0.5f) * 31) * 32;
	
	float o = (x + y) / 1023;
	
	f1 = o;
	
    //float       fval1 = f3.x;
    //float       fval2 = f3.y;
    //float       fval3 = f3.z;
    //float       scaled0;
    //float       added0;
    //float       sqrt0;
    //float       div0;
    //float       added1;
    //float       scaled1;
    //float       floor0;
    //float       scaled2;
    //float       div1;
    //float       added2;
    //float       scaled3;
    //float       sum0;
    //float       scaled4;

	//// Code produced by: mulconst1
    //scaled0 = fval3 * 8;
    
    //// Code produced by: addconst1
    //added0 = scaled0 + 8;
    
    //// Code produced by: sqrt1
    //sqrt0 = sqrt(added0);
    
    //// Code produced by: divide1
    //div0 = fval1 / sqrt0;
    
    //// Code produced by: addconst2
    //added1 = div0 + 0.5;
    
    //// Code produced by: mulconst2
    //scaled1 = added1 * 31;
    
    //// Code produced by: floor1
    //floor0 = floor(scaled1);
    
    //// Code produced by: mulconst3
    //scaled2 = floor0 * 32;
    
    //// Code produced by: divide2
    //div1 = fval2 / sqrt0;
    
    //// Code produced by: addconst3
    //added2 = div1 + 0.5;
    
    //// Code produced by: mulconst4
    //scaled3 = added2 * 31;
    
    //// Code produced by: add1
    //sum0 = scaled2 + scaled3;
    
    //// Code produced by: divconst1
    //scaled4 = sum0 * (1.0 / 1023);
    
    //f1 = scaled4;
}

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