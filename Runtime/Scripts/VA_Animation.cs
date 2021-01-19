using UnityEngine;

namespace TAO.VertexAnimation
{
    public class VA_Animation : ScriptableObject
    {
		public VA_Animation(int a_maxFrames, int a_frames, int a_fps, int a_positionMapIndex, int a_colorMapIndex = -1)
		{
			Data = new VA_AnimationData(this.name, a_frames, a_maxFrames, a_fps, a_positionMapIndex, a_colorMapIndex);
		}

		public VA_Animation(VA_AnimationData a_data)
		{
			this.name = a_data.name.ToString();
			Data = a_data;
		}

		public VA_AnimationData Data
		{
			get; private set;
		}

		// data.name will be overwritten by this.name.
		public void SetData(VA_AnimationData a_data)
		{
			a_data.name = this.name;
			Data = a_data;
		}
	}
}