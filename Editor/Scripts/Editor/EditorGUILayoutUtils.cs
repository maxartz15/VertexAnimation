using UnityEditor;
using UnityEngine;

namespace TAO.VertexAnimation.Editor
{
	public static class EditorGUILayoutUtils
	{
		public static readonly Color horizontalLineColor = Color.white;

		public static void HorizontalLine(Color color)
		{
			Color prev = GUI.color;
			GUI.color = color;
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUI.color = prev;
		}

		public static void HorizontalLine() => HorizontalLine(horizontalLineColor);
	}
}
