using LunaWolfStudiosEditor.ScriptableSheets.Scanning;
using System.Collections.Generic;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets
{
	[System.Serializable]
	public class WindowSessionState
	{
		[SerializeField]
		private int m_InstanceId;
		public int InstanceId { get => m_InstanceId; set => m_InstanceId = value; }

		[SerializeField]
		private string m_Title;
		public string Title { get => m_Title; set => m_Title = value; }

		[SerializeField]
		private string m_Position;
		public string Position { get => m_Position; set => m_Position = value; }

		[SerializeField]
		private SheetAsset m_SelectableSheetAssets;
		public SheetAsset SelectableSheetAssets { get => m_SelectableSheetAssets; set => m_SelectableSheetAssets = value; }

		[SerializeField]
		private SheetAsset m_SelectedSheetAsset;
		public SheetAsset SelectedSheetAsset { get => m_SelectedSheetAsset; set => m_SelectedSheetAsset = value; }

		[SerializeField]
		private int m_SelectedTypeIndex;
		public int SelectedTypeIndex { get => m_SelectedTypeIndex; set => m_SelectedTypeIndex = value; }

		[SerializeField]
		private Dictionary<SheetAsset, HashSet<int>> m_PinnedIndexSets;
		public Dictionary<SheetAsset, HashSet<int>> PinnedIndexSets { get => m_PinnedIndexSets; set => m_PinnedIndexSets = value; }

		[SerializeField]
		private int m_NewAmount;
		public int NewAmount { get => m_NewAmount; set => m_NewAmount = value; }

		[SerializeField]
		private string m_SearchInput;
		public string SearchInput { get => m_SearchInput; set => m_SearchInput = value; }

		[SerializeField]
		private Dictionary<string, TableLayout> m_TableLayouts;
		public Dictionary<string, TableLayout> TableLayouts { get => m_TableLayouts; set => m_TableLayouts = value; }
	}

	[System.Serializable]
	public class TableLayout
	{
		[SerializeField]
		private int m_SortedColumnIndex = 1;
		public int SortedColumnIndex { get => m_SortedColumnIndex; set => m_SortedColumnIndex = value; }

		[SerializeField]
		private bool m_IsSortedAscending;
		public bool IsSortedAscending { get => m_IsSortedAscending; set => m_IsSortedAscending = value; }

		[SerializeField]
		private int m_ColumnCount;
		public int ColumnCount { get => m_ColumnCount; set => m_ColumnCount = value; }

		[SerializeField]
		private float[] m_ColumnWidths;
		public float[] ColumnWidths { get => m_ColumnWidths; set => m_ColumnWidths = value; }

		[SerializeField]
		private int[] m_VisibleColumns;
		public int[] VisibleColumns { get => m_VisibleColumns; set => m_VisibleColumns = value; }

		[SerializeField]
		private int m_MainAssetIndex;
		public int MainAssetIndex { get => m_MainAssetIndex; set => m_MainAssetIndex = value; }
	}
}
