using LunaWolfStudiosEditor.ScriptableSheets.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Popups
{
	public class ColumnVisibilityPopupWindowContent : AnchoredPopupWindowContent
	{
		private readonly MultiColumnHeaderState m_MultiColumnHeaderState;
		private readonly int m_VisibleColumnLimit;
		private readonly Action<int[]> m_VisibilityChanged;

		private readonly bool[] m_ColumnVisibility;
		private readonly string[] m_ColumnNames;
		private readonly HashSet<int> m_VisibleColumns = new HashSet<int>();
		private readonly Paginator m_Paginator = new Paginator();
		private readonly List<int> m_Indices = new List<int>();
		private readonly List<int> m_PagedIndices = new List<int>();

		private Vector2 m_SizeOfBestFit = Vector2.zero;
		private Vector2 m_ScrollPos;
		private int m_TotalPages;
		private int m_PreviousSelectedPage = 1;

		public ColumnVisibilityPopupWindowContent(Rect anchoredRect, MultiColumnHeaderState multiColumnHeaderState, int visibleColumnLimit, Action<int[]> visibilityChanged) : base(anchoredRect)
		{
			m_MultiColumnHeaderState = multiColumnHeaderState;
			m_VisibleColumnLimit = visibleColumnLimit;
			m_VisibilityChanged = visibilityChanged;

			m_ColumnVisibility = new bool[m_MultiColumnHeaderState.columns.Length];
			m_ColumnNames = new string[m_MultiColumnHeaderState.columns.Length];

			var columnCount = m_MultiColumnHeaderState.columns.Length;
			for (var i = 0; i < columnCount; i++)
			{
				m_ColumnVisibility[i] = m_MultiColumnHeaderState.visibleColumns.Contains(i);
				m_ColumnNames[i] = string.IsNullOrEmpty(m_MultiColumnHeaderState.columns[i].headerContent?.text) ? i.ToString() : m_MultiColumnHeaderState.columns[i].headerContent.text;
				if (m_ColumnVisibility[i])
				{
					m_VisibleColumns.Add(i);
				}
			}

			m_Paginator.SetObjectsPerPage(PopupContent.Window.ColumnVisibilityRowsPerPage);
			m_Paginator.SetTotalObjects(columnCount);
			m_TotalPages = m_Paginator.GetTotalPages();
			m_Indices = Enumerable.Range(0, m_ColumnVisibility.Length).ToList();
			m_PagedIndices = m_Paginator.GetPageObjects(m_Indices);
		}

		public override Vector2 GetWindowSize()
		{
			// Can't call GUI.skin if opening via context menu.
			if (Event.current == null || m_ColumnNames.Length <= 0)
			{
				return m_AnchoredRect.size;
			}
			// We need to calculate the best fit from inside OnGUI in case the user opens the popup from the context window.
			if (m_SizeOfBestFit == Vector2.zero)
			{
				var widestLabel = m_ColumnNames.Max(n => GUI.skin.toggle.CalcSize(new GUIContent(n)).x);
				// Add extra padding when we display the pagination buttons.
				if (m_TotalPages > 1)
				{
					widestLabel = Mathf.Max(PopupContent.Window.ColumnVisibilityMinWidth, widestLabel);
				}
				m_SizeOfBestFit.x = Mathf.Min(widestLabel + PopupContent.Window.ColumnVisibilityPadding, m_AnchoredRect.size.x);
				var contentHeight = (m_PagedIndices.Count + 1) * EditorGUIUtility.singleLineHeight;
				m_SizeOfBestFit.y = Mathf.Min(contentHeight + PopupContent.Window.ColumnVisibilityPadding, m_AnchoredRect.size.y);
			}
			return m_SizeOfBestFit;
		}

		public override void OnGUI()
		{
			if (m_ColumnVisibility.Length <= 0)
			{
				return;
			}

			EditorGUILayout.BeginHorizontal();

			var columnVisibilityHeaderContent = SheetsContent.Label.GetColumnContent(m_ColumnVisibility.Count(c => c == true), m_VisibleColumnLimit, m_ColumnVisibility.Count());
			columnVisibilityHeaderContent.text = $"{PopupContent.Label.ColumnVisibility} {columnVisibilityHeaderContent.text}";
			var headerLabelWidth = GUI.skin.label.CalcSize(columnVisibilityHeaderContent).x;
			EditorGUILayout.LabelField(columnVisibilityHeaderContent, GUILayout.Width(headerLabelWidth));

			if (m_TotalPages > 1)
			{
				GUILayout.FlexibleSpace();

				var showFirstAndLastPageButtons = m_TotalPages > SheetLayout.FirstAndLastPageThreshold;
				if (showFirstAndLastPageButtons)
				{
					EditorGUI.BeginDisabledGroup(m_Paginator.IsOnFirstPage());
					if (GUILayout.Button(SheetsContent.Button.FirstPage, SheetLayout.InlineButton))
					{
						m_Paginator.GoToFirstPage();
					}
					EditorGUI.EndDisabledGroup();
				}

				if (GUILayout.Button(SheetsContent.Button.PreviousPage, SheetLayout.InlineButton))
				{
					m_Paginator.PreviousPage();
				}

				var pageLabelContent = SheetsContent.Label.GetColumnVisibilityPageContent(m_Paginator.CurrentPage, m_TotalPages, m_Paginator.TotalObjects);
				var pageLabelWidth = GUI.skin.label.CalcSize(pageLabelContent).x;
				EditorGUILayout.LabelField(pageLabelContent, SheetLayout.CenterLabelStyle, GUILayout.Width(pageLabelWidth));

				if (GUILayout.Button(SheetsContent.Button.NextPage, SheetLayout.InlineButton))
				{
					m_Paginator.NextPage();
				}

				if (showFirstAndLastPageButtons)
				{
					EditorGUI.BeginDisabledGroup(m_Paginator.IsOnLastPage());
					if (GUILayout.Button(SheetsContent.Button.LastPage, SheetLayout.InlineButton))
					{
						m_Paginator.GoToLastPage();
					}
					EditorGUI.EndDisabledGroup();
				}

				if (m_PreviousSelectedPage != m_Paginator.CurrentPage)
				{
					m_PagedIndices.Clear();
					m_PagedIndices.AddRange(m_Paginator.GetPageObjects(m_Indices));
					m_PreviousSelectedPage = m_Paginator.CurrentPage;
				}
			}

			EditorGUILayout.EndHorizontal();

			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);

			EditorGUI.BeginChangeCheck();

			foreach (var i in m_PagedIndices)
			{
				EditorGUI.BeginDisabledGroup((m_MultiColumnHeaderState.visibleColumns.Length >= m_VisibleColumnLimit && !m_ColumnVisibility[i]) || i == 0);
				var newValue = EditorGUILayout.ToggleLeft(m_ColumnNames[i], m_ColumnVisibility[i]);
				if (newValue != m_ColumnVisibility[i])
				{
					if (newValue)
					{
						m_VisibleColumns.Add(i);
					}
					else
					{
						m_VisibleColumns.Remove(i);
					}
					m_ColumnVisibility[i] = newValue;
				}
				EditorGUI.EndDisabledGroup();
			}

			if (EditorGUI.EndChangeCheck())
			{
				m_VisibilityChanged?.Invoke(m_VisibleColumns.OrderBy(i => i).ToArray());
			}

			EditorGUILayout.EndScrollView();
		}
	}
}