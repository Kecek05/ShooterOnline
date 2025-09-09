using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Layout
{
	public class Content
	{
		// Workaround because Unity's default implementation for IconContent with a tooltip does not work.
		public static GUIContent GetIconContent(string iconName, string tooltip)
		{
			var image = EditorGUIUtility.IconContent(iconName).image;
			return new GUIContent(image, tooltip);
		}

		public static GUIContent GetIconContent(string iconName, string text, string tooltip)
		{
			var image = EditorGUIUtility.IconContent(iconName).image;
			return new GUIContent(text, image, tooltip);
		}
	}
}
