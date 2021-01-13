using UnityEngine;
using UnityEditor;

namespace TAO.VertexAnimation.Editor
{
	public static class AssetDatabaseUtils
	{
		public static bool HasChildAsset(Object parent, Object child)
		{
			var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parent));

			foreach (var a in assets)
			{
				if (a == child)
				{
					return true;
				}
			}

			return false;
		}

		public static void RemoveChildAssets(Object parent, Object[] filter = null)
		{
			var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parent));

			foreach (var a in assets)
			{
				bool filterSkip = false;

				foreach (var f in filter)
				{
					if (a == f)
					{
						filterSkip = true;
						break;
					}
				}

				if (!filterSkip && a != parent)
				{
					AssetDatabase.RemoveObjectFromAsset(a);
				}
			}
		}
	}
}