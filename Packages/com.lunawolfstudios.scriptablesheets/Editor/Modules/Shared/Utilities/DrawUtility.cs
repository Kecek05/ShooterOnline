using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Shared
{
	public static class DrawUtility
	{
		public static void TableAssetPreview(Object obj, Rect propertyRect, AssetPreviewSettings assetPreviewSettings)
		{
			if (!assetPreviewSettings.Show || obj == null)
			{
				return;
			}

			var preview = AssetPreview.GetAssetPreview(obj) ?? AssetPreview.GetMiniThumbnail(obj);
			if (preview == null)
			{
				return;
			}

			var previewRect = propertyRect;
			previewRect.y += EditorGUIUtility.singleLineHeight;
			previewRect.height -= EditorGUIUtility.singleLineHeight;
			UnityEngine.GUI.DrawTexture(previewRect, preview, assetPreviewSettings.ScaleMode);
		}

		public static class GUI
		{
			public static bool ToggleCenter(Rect propertyRect, bool value)
			{
				var centerPoint = (propertyRect.width - 10) / 2;
				propertyRect.x += centerPoint;
				propertyRect.width -= centerPoint;
				return EditorGUI.Toggle(propertyRect, value);
			}

			public static uint UIntField(Rect propertyRect, uint value)
			{
				var textValue = EditorGUI.TextField(propertyRect, value.ToString());
				return uint.TryParse(textValue, out uint newValue) ? newValue : value;
			}

			public static ulong ULongField(Rect propertyRect, ulong value)
			{
				var textValue = EditorGUI.TextField(propertyRect, value.ToString());
				return ulong.TryParse(textValue, out ulong newValue) ? newValue : value;
			}
		}
	}
}
