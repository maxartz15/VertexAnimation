using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TAO.VertexAnimation
{
    [CreateAssetMenu(fileName = "new AnimationBook", menuName = "AnimationBook", order = 0)]
    public class VA_AnimationBookSO : ScriptableObject
    {
        public int maxFrames;
        public Material[] materials;
        public VA_AnimationPage[] animationPages;

        private void Setup()
        {
            // TODO: ...
            // GenerateTextures.
            // SetupMaterials.
        }
    }

    [System.Serializable]
    public struct VA_AnimationPage
    {
        public string name;
        public int frames;
        public Texture2D texture2D;
    }

}