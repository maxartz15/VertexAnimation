using UnityEngine;

namespace tech_art_outsource.vertex_animation
{
    public class VA_MaterialSetup : MonoBehaviour
    {
        public TextAsset m_animationJson = null;
        [HideInInspector]
        public AnimationData m_animationData = null;

        private Material m_material = null;

        // Start is called before the first frame update
        void Start()
        {
            m_animationData = JsonUtility.FromJson<AnimationData>(m_animationJson.text);

            m_material = GetComponent<MeshRenderer>().material;

            m_material.SetInt("_numOfFrames", int.Parse(m_animationData.vertex_animation_textures1[0]._numOfFrames));
            m_material.SetFloat("_paddedX", float.Parse(m_animationData.vertex_animation_textures1[0]._paddedX));
            m_material.SetFloat("_paddedY", float.Parse(m_animationData.vertex_animation_textures1[0]._paddedY));
            m_material.SetFloat("_pivMax", float.Parse(m_animationData.vertex_animation_textures1[0]._pivMax));
            m_material.SetFloat("_pivMin", float.Parse(m_animationData.vertex_animation_textures1[0]._pivMin));
            m_material.SetFloat("_posMax", float.Parse(m_animationData.vertex_animation_textures1[0]._posMax));
            m_material.SetFloat("_posMin", float.Parse(m_animationData.vertex_animation_textures1[0]._posMin));
            m_material.SetFloat("_speed", float.Parse(m_animationData.vertex_animation_textures1[0]._speed));
        }
    }

    [System.Serializable]
    public class AnimationData
    {
        public AnimData[] vertex_animation_textures1;
    }

    [System.Serializable]
    public class AnimData
    {
        public string _doubleTex;
        public string _height;
        public string _normData;
        public string _numOfFrames;
        public string _packNorm;
        public string _packPscale;
        public string _paddedX;
        public string _paddedY;
        public string _pivMax;
        public string _pivMin;
        public string _posMax;
        public string _posMin;
        public string _scaleMax;
        public string _scaleMin;
        public string _speed;
        public string _width;
    }
}