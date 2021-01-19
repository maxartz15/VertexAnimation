using Unity.Collections;
using UnityEngine;

namespace TAO.VertexAnimation
{
    public class VA_Animation : ScriptableObject
    {
		public VA_AnimationData Data;

		// data.name will be overwritten by this.name.
		public void SetData(VA_AnimationData a_data)
		{
			Data = a_data;
		}

		public VA_AnimationData GetData()
		{
			// TODO: Fix data name, FixedString32 doesn't transfer from editor?
			Data.name = new FixedString32(name);
			return Data;
		}

		public FixedString32 GetName()
		{
			return new FixedString32(this.name);
		}
	}
}