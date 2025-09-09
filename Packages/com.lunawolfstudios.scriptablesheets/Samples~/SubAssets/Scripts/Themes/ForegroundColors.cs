using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class ForegroundColors : BaseColor
	{
		[SerializeField]
		private Color m_BaseForegroundColor = Color.white;
		public Color BaseForegroundColor { get => m_BaseForegroundColor; set => m_BaseForegroundColor = value; }

		[SerializeField]
		private Color m_ForegroundGlowColor = Color.white;
		public Color ForegroundGlowColor { get => m_ForegroundGlowColor; set => m_ForegroundGlowColor = value; }

		[SerializeField]
		private int m_ForegroundGlowIntensity;
		public int ForegroundGlowIntensity { get => m_ForegroundGlowIntensity; set => m_ForegroundGlowIntensity = value; }
	}
}
