using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Scanning
{
	[System.Serializable]
	public class ScanSettings
	{
		[SerializeField]
		private ScanOption m_Option = ScanOption.Default;
		public ScanOption Option { get => m_Option; set => m_Option = value; }

		[SerializeField]
		private ScanPathOption m_PathOption = ScanPathOption.Default;
		public ScanPathOption PathOption { get => m_PathOption; set => m_PathOption = value; }

		[SerializeField]
		private string m_Path = UnityConstants.DefaultAssetPath;
		public string Path { get => m_Path; set => m_Path = value; }

		[SerializeField]
		private bool m_ShowProgressBar = true;
		public bool ShowProgressBar { get => m_ShowProgressBar; set => m_ShowProgressBar = value; }

		[SerializeField]
		private bool m_RootPrefabsOnly = true;
		public bool RootPrefabsOnly { get => m_RootPrefabsOnly; set => m_RootPrefabsOnly = value; }

		public string[] GetScanPaths()
		{
			switch (m_PathOption)
			{
				case ScanPathOption.Default:
					return new string[] { m_Path };
				case ScanPathOption.Assets:
					return new string[] { UnityConstants.DefaultAssetPath };
				case ScanPathOption.Packages:
					return new string[] { UnityConstants.Packages };
				case ScanPathOption.All:
					return new string[] { UnityConstants.DefaultAssetPath, UnityConstants.Packages };
				default:
					Debug.LogWarning($"Selected {nameof(ScanPathOption)} {m_PathOption} is not defined. Using default scan path {UnityConstants.DefaultAssetPath}.");
					return new string[] { UnityConstants.DefaultAssetPath };
			}
		}

		public string GetFirstScanPath()
		{
			return GetScanPaths()[0];
		}

		public string GetJoinedScanPaths()
		{
			var scanPaths = GetScanPaths();
			return string.Join("\n", scanPaths);
		}

		public string GetJoinedScanPaths(string[] scanPaths)
		{
			return string.Join("\n", scanPaths);
		}
	}
}
