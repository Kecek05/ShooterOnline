using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Shared
{
	[System.Serializable]
	public class AssetPreviewSettings
	{
		[SerializeField]
		private bool m_Show = true;
		public bool Show { get => m_Show; set => m_Show = value; }

		[SerializeField]
		private ScaleMode m_ScaleMode = ScaleMode.ScaleToFit;
		public ScaleMode ScaleMode { get => m_ScaleMode; set => m_ScaleMode = value; }
	}
}
