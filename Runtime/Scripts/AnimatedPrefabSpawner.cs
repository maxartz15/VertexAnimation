using System;
using UnityEngine;

namespace TAO.VertexAnimation
{

public class AnimatedPrefabSpawner : MonoBehaviour
{
    public GameObject Prefab;

    public Transform BottomLeftCorner;

    public Transform Parent;
    
    public int Width;
    public int Height;
    
    public float Distance;
    
    [ContextMenu("Test")]
    public void SetAllSeeds()
    {
     
        Vector3 currentPosition = BottomLeftCorner.position;
        Vector3 startPosition = currentPosition;
        for ( int i = 0; i < Width; i++ )
        {
            for ( int j = 0; j < Height; j++ )
            {
                GameObject instance = Instantiate( Prefab, Parent, true );
                instance.transform.position = currentPosition;
                currentPosition = new Vector3( currentPosition.x + Distance, currentPosition.y, currentPosition.z );
            }
            currentPosition = new Vector3( startPosition.x , currentPosition.y, currentPosition.z + Distance );
        }
        AnimationLibraryComponentAuthoring[] vaAnimationLibraryComponentAuthorings = Parent.GetComponentsInChildren < AnimationLibraryComponentAuthoring >();

        foreach ( AnimationLibraryComponentAuthoring authoring in vaAnimationLibraryComponentAuthorings )
        {
            var test = Guid.NewGuid().GetHashCode().ToString();
            Debug.Log( test );
            test = test.Substring( test.Length - 4 );
            Debug.Log( UInt32.Parse( test ) );
            authoring.Seed = UInt32.Parse( test );
        }
    }
}

}
