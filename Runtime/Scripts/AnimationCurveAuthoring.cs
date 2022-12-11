using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TAO.VertexAnimation
{

public class AnimationCurveAuthoring : MonoBehaviour
{
    public List < AnimationCurve > CurvesToConvert;
    public int NumberOfSamples = 256;
}


public class AnimationCurveAuthoringBaker : Baker <AnimationCurveAuthoring>
{
    public override void Bake( AnimationCurveAuthoring authoring )
    {
        EntitiesAnimationCurveLibrary curveLibrary = new EntitiesAnimationCurveLibrary();
        curveLibrary.CurveReferences = new NativeList < EntitiesAnimationCurveReference >( 5, Allocator.Persistent );
        foreach ( AnimationCurve curve in authoring.CurvesToConvert )
        {
            curveLibrary.CurveReferences.Add( curve.LoadUnityAnimationCurveIntoEntitiesAnimationCurve( authoring.NumberOfSamples ) );
        }
        
        AddComponent( curveLibrary );
    }
}
}
