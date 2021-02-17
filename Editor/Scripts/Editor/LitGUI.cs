using UnityEditor;
using UnityEngine;

namespace TAO.VertexAnimation.Editor
{
    public class LitGUI : ShaderGUI
    {
		private MaterialEditor materialEditor;
		private MaterialProperty[] properties;
		private bool foldoutBase = true;
		private bool foldoutAnimation = true;
		private bool foldoutOther = true;

		override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			this.materialEditor = materialEditor;
			this.properties = properties;

			if (foldoutBase = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutBase, "Base"))
			{
				BaseMapGUI();
				NormalGUI();
				MaskGUI();
				EmissionGUI();
				MaterialProperty tilingAndOffset = FindProperty("_TilingAndOffset", properties);
				materialEditor.ShaderProperty(tilingAndOffset, MakeLabel(tilingAndOffset));
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			if (foldoutAnimation = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAnimation, "Vertex Animation"))
			{
				VertexAnimationGUI();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			if (foldoutOther = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOther, "Other"))
			{
				OtherGUI();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private void BaseMapGUI()
		{
			MaterialProperty map = FindProperty("_BaseMap", properties);
			materialEditor.TexturePropertySingleLine(MakeLabel(map, "(RGB) Albedo, (A) Alpha."), map, FindProperty("_BaseColor", properties));

			EditorGUI.indentLevel += 2;

			MaterialProperty slider = FindProperty("_AlphaClipThreshhold", properties);
			materialEditor.ShaderProperty(slider, MakeLabel(slider));

			EditorGUI.indentLevel -= 2;
		}

		private void NormalGUI()
		{
			MaterialProperty map = FindProperty("_NormalMap", properties);
			MaterialProperty strength = FindProperty("_NormalStrength", properties);

			EditorGUI.BeginChangeCheck();

			materialEditor.TexturePropertySingleLine(MakeLabel(map), map, map.textureValue ? strength : null);

			if (EditorGUI.EndChangeCheck())
			{
				SetKeyword("USE_NORMALMAP_ON", map.textureValue);
			}
		}

		private void MaskGUI()
		{
			MaterialProperty map = FindProperty("_MaskMap", properties);
			materialEditor.TexturePropertySingleLine(MakeLabel(map, "(R) Metallic, (G) Occlusion, (B) Detail mask, (A) Smoothness."), map);

			EditorGUI.indentLevel += 2;

			MaterialProperty metalness = FindProperty("_Metalness", properties);
			materialEditor.ShaderProperty(metalness, MakeLabel(metalness));
			MaterialProperty smoothness = FindProperty("_Smoothness", properties);
			materialEditor.ShaderProperty(smoothness, MakeLabel(smoothness));

			EditorGUI.indentLevel -= 2;
		}

		private void EmissionGUI()
		{
			MaterialProperty map = FindProperty("_EmissionMap", properties);

			EditorGUI.BeginChangeCheck();

			materialEditor.TexturePropertySingleLine(MakeLabel(map), map, FindProperty("_EmissionColor", properties));

			if (EditorGUI.EndChangeCheck())
			{
				SetKeyword("USE_EMISSIONMAP_ON", map.textureValue);
			}
		}

		private void VertexAnimationGUI()
		{
			MaterialProperty map = FindProperty("_PositionMap", properties);
			materialEditor.TexturePropertySingleLine(MakeLabel(map), map);

			var mat = materialEditor.target as Material;

			{
				bool value = mat.IsKeywordEnabled("USE_INTERPOLATION_ON");
				MaterialProperty useInterpolation = FindProperty("USE_INTERPOLATION", properties);

				EditorGUI.BeginChangeCheck();

				value = EditorGUILayout.Toggle(MakeLabel(useInterpolation, "For smooth animations."), mat.IsKeywordEnabled("USE_INTERPOLATION_ON"));
				//materialEditor.ShaderProperty(useInterpolation, MakeLabel(useInterpolation, "For smooth animations."));

				if (EditorGUI.EndChangeCheck())
				{
					SetKeyword("USE_INTERPOLATION_ON", value);
				}
			}

			{
				bool value = mat.IsKeywordEnabled("USE_NORMALA_ON");
				MaterialProperty useNormalA = FindProperty("USE_NORMALA", properties);

				EditorGUI.BeginChangeCheck();

				value = EditorGUILayout.Toggle(MakeLabel(useNormalA, "Apply vertex normals saved in the alpha channel of the position map."), mat.IsKeywordEnabled("USE_NORMALA_ON"));
				//materialEditor.ShaderProperty(normal, MakeLabel(useNormalA, "Apply vertex normals saved in the alpha channel of the position map."));

				if (EditorGUI.EndChangeCheck())
				{
					SetKeyword("USE_NORMALA_ON", value);
				}
			}

			{
				bool value = mat.IsKeywordEnabled("VA_FLIP_UVS");
				MaterialProperty flipUV = FindProperty("VA_FLIP_UVS", properties);

				EditorGUI.BeginChangeCheck();

				value = EditorGUILayout.Toggle(MakeLabel(flipUV, "Flip UVs."), mat.IsKeywordEnabled("VA_FLIP_UVS_ON"));
				//materialEditor.ShaderProperty(useInterpolation, MakeLabel(useInterpolation, "For smooth animations."));

				if (EditorGUI.EndChangeCheck())
				{
					SetKeyword("VA_FLIP_UVS_ON", value);
				}
			}

			MaterialProperty maxFrames = FindProperty("_MaxFrames", properties);
			materialEditor.ShaderProperty(maxFrames, MakeLabel(maxFrames, "This will be auto filled by the animation system."));
			MaterialProperty animationData = FindProperty("_AnimationData", properties);
			materialEditor.ShaderProperty(animationData, MakeLabel(animationData, "This will be auto filled by the animation system."));
		}

		private void OtherGUI()
		{
			materialEditor.RenderQueueField();
			materialEditor.EnableInstancingField();
			materialEditor.DoubleSidedGIField();
		}

		private void SetKeyword(string keyword, bool state)
		{
			if (state)
			{
				foreach (Material m in materialEditor.targets)
				{
					m.EnableKeyword(keyword);
				}
			}
			else
			{
				foreach (Material m in materialEditor.targets)
				{
					m.DisableKeyword(keyword);
				}
			}
		}

		static readonly GUIContent staticLabel = new GUIContent();
		static GUIContent MakeLabel(MaterialProperty property, string tooltip = null)
		{
			staticLabel.text = property.displayName;
			staticLabel.tooltip = tooltip;
			return staticLabel;
		}
	}
}