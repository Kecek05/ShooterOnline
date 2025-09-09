using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Popups
{
	public abstract class AnchoredPopupWindowContent : PopupWindowContent
	{
		protected readonly Rect m_AnchoredRect;

		public AnchoredPopupWindowContent(Rect anchoredRect)
		{
			m_AnchoredRect = anchoredRect;
		}

		public override Vector2 GetWindowSize()
		{
			return m_AnchoredRect.size;
		}

		public override void OnGUI(Rect rect)
		{
			editorWindow.position = m_AnchoredRect;
			OnGUI();
		}

		public abstract void OnGUI();
	}
}
