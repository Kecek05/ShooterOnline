using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class SampleMainAsset : ScriptableObject
	{
		[SerializeField]
		private string m_Description;
		public string Description { get => m_Description; set => m_Description = value; }

		[SerializeField]
		private SampleSubAsset[] m_SubAssets;
		public SampleSubAsset[] SubAssets { get => m_SubAssets; set => m_SubAssets = value; }
	}
}