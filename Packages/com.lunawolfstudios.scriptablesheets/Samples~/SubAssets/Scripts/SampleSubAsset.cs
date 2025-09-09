using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class SampleSubAsset : ScriptableObject
	{
		[SerializeField]
		private string m_Description;
		public string Description { get => m_Description; set => m_Description = value; }

		[SerializeField]
		private int m_Index;
		public int Index { get => m_Index; set => m_Index = value; }

		[SerializeField]
		private bool m_SomeBoolValue;
		public bool SomeBoolValue { get => m_SomeBoolValue; set => m_SomeBoolValue = value; }
	}
}