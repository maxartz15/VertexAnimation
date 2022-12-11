using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TAO.VertexAnimation
{
public static class AnimationCurveExtention
{
    public static float[] GenerateCurveArray(this UnityEngine.AnimationCurve self, int numberOfSamples)
    {
        float[] returnArray = new float[numberOfSamples];
        for (int j = 0; j < numberOfSamples; j++)
        {
            returnArray[j] = self.Evaluate((float)j / numberOfSamples);   
            //Debug.Log( returnArray[j] );
        }
        
        return returnArray;
    }
    
    public static EntitiesAnimationCurveReference LoadUnityAnimationCurveIntoEntitiesAnimationCurve(this UnityEngine.AnimationCurve animationCurve, int numberOfSamples)
    {
        float[] samplePoints = animationCurve.GenerateCurveArray(numberOfSamples);
        EntitiesAnimationCurveReference component = new EntitiesAnimationCurveReference();
        using ( BlobBuilder blobBuilder = new BlobBuilder( Allocator.Temp ) )
        {
            ref EntitiesAnimationCurve animationCurveEcs = ref blobBuilder.ConstructRoot < EntitiesAnimationCurve >();
            BlobBuilderArray<float> curveSamples = blobBuilder.Allocate( ref animationCurveEcs.SampledPoints, numberOfSamples );
            for (int i = 0; i < numberOfSamples; i++)
            {
                // Copy data.
                curveSamples[i] = samplePoints[i];
            }

            component.EntitiesCurve = blobBuilder.CreateBlobAssetReference < EntitiesAnimationCurve >( Allocator.Persistent );
        }

        component.EntitiesCurve.Value.NumberOfSamples = numberOfSamples;
        return component;
    }
}
[Serializable]
public struct EntitiesAnimationCurve
{
    public BlobArray < float > SampledPoints;
    public int NumberOfSamples;
    
    public float GetValueAtTime(float time)
    {
        var approxSampleIndex = (NumberOfSamples - 1) * time;
        var sampleIndexBelow = (int)math.floor(approxSampleIndex);
        if (sampleIndexBelow >= NumberOfSamples - 1)
        {
            return SampledPoints[NumberOfSamples - 1];
        }
        var indexRemainder = approxSampleIndex - sampleIndexBelow;
        return math.lerp(SampledPoints[sampleIndexBelow], SampledPoints[sampleIndexBelow + 1], indexRemainder);
    }
    

}
[Serializable]
public struct EntitiesAnimationCurveReference
{
    public BlobAssetReference<EntitiesAnimationCurve> EntitiesCurve;
    public readonly float GetValueAtTime(float time) => EntitiesCurve.Value.GetValueAtTime(time);
}

[Serializable]
public struct EntitiesAnimationCurveLibrary : IComponentData
{
    public NativeList < EntitiesAnimationCurveReference > CurveReferences;
}


public struct SampledAnimationCurve : System.IDisposable
{
    NativeArray<float> sampledFloat;
    /// <param name="samples">Must be 2 or higher</param>
    public SampledAnimationCurve(AnimationCurve ac, int samples)
    {
        sampledFloat = new NativeArray<float>(samples, Allocator.Persistent);
        float timeFrom = ac.keys[0].time;
        float timeTo = ac.keys[ac.keys.Length - 1].time;
        float timeStep = (timeTo - timeFrom) / (samples - 1);
 
        for (int i = 0; i < samples; i++)
        {
            sampledFloat[i] = ac.Evaluate(timeFrom + (i * timeStep));
        }
    }
 
    public void Dispose()
    {
        sampledFloat.Dispose();
    }
 
    /// <param name="time">Must be from 0 to 1</param>
    public float EvaluateLerp(float time)
    {
        int len = sampledFloat.Length - 1;
        float clamp01 = time < 0 ? 0 : (time > 1 ? 1 : time);
        float floatIndex = (clamp01 * len);
        int floorIndex = (int)math.floor(floatIndex);
        if (floorIndex == len)
        {
            return sampledFloat[len];
        }
 
        float lowerValue = sampledFloat[floorIndex];
        float higherValue = sampledFloat[floorIndex + 1];
        return math.lerp(lowerValue, higherValue, math.frac(floatIndex));
    }
}
}
