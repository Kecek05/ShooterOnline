using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Layout
{
	public class PopupContent : Content
	{
		public static class Button
		{
			public static readonly GUIContent Cancel = GetIconContent(EditorIcon.Cancel, "Cancel (ESC)");
			public static readonly GUIContent Confirm = GetIconContent(EditorIcon.Confirm, "Confirm (Enter)");
		}

		public static class Label
		{
			public static readonly GUIContent Rename = new GUIContent("New Name:");
			public static readonly GUIContent ColumnVisibility = new GUIContent("Columns");
		}

		public static class Window
		{
			public static readonly int ColumnVisibilityRowsPerPage = 100;
			public static readonly float ColumnVisibilityMinWidth = 250;
			public static readonly float ColumnVisibilityPadding = 50;
			public static readonly Vector2 ColumnVisibilityMaxSize = new Vector2(400, 600);
			public static readonly Vector2 RenameSize = new Vector2(200, 100);
		}
	}
}
