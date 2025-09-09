using LunaWolfStudiosEditor.ScriptableSheets.Layout;
using System;
using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Popups
{
	public class InputPopupWindowContent : AnchoredPopupWindowContent
	{
		private static readonly string InputPopupControlName = $"{nameof(InputPopupWindowContent)}";

		private readonly Action<string> m_Confirmed;
		private readonly GUIContent m_LabelContent;

		private string m_Input;

		public InputPopupWindowContent(Rect anchoredRect, GUIContent labelContent, string defaultInput, Action<string> confirmed) : base(anchoredRect)
		{
			m_LabelContent = labelContent;
			m_Input = defaultInput;
			m_Confirmed = confirmed;
		}

		public override void OnGUI()
		{
			EditorGUILayout.Space();

			EditorGUILayout.LabelField(m_LabelContent);

			EditorGUILayout.Space();

			GUI.SetNextControlName(InputPopupControlName);
			m_Input = EditorGUILayout.TextField(m_Input);
			if (Event.current.type == EventType.Repaint)
			{
				EditorGUI.FocusTextInControl(InputPopupControlName);
			}
			GUI.SetNextControlName(string.Empty);

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(PopupContent.Button.Confirm))
			{
				ConfirmInput();
			}

			SheetLayout.DrawVerticalLine();

			if (GUILayout.Button(PopupContent.Button.Cancel))
			{
				CancelInput();
			}
			EditorGUILayout.EndHorizontal();

			var e = Event.current;
			if (e.type == EventType.KeyUp && e.modifiers == EventModifiers.None)
			{
				switch (e.keyCode)
				{
					case KeyCode.KeypadEnter:
					case KeyCode.Return:
						ConfirmInput();
						e.Use();
						break;
					case KeyCode.Escape:
						CancelInput();
						e.Use();
						break;
				}
			}
		}

		private void ConfirmInput()
		{
			m_Confirmed?.Invoke(m_Input);
			editorWindow.Close();
		}

		private void CancelInput()
		{
			editorWindow.Close();
		}
	}
}
