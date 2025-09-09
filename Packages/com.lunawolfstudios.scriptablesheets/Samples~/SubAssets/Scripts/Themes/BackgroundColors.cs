using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class BackgroundColors : BaseColor
	{
		[SerializeField]
		private Color m_BaseBackgroundColor = Color.white;
		public Color BaseBackgroundColor { get => m_BaseBackgroundColor; set => m_BaseBackgroundColor = value; }

		[SerializeField]
		private Color m_BackgroundGlowColor = Color.white;
		public Color BackgroundGlowColor { get => m_BackgroundGlowColor; set => m_BackgroundGlowColor = value; }

		[SerializeField]
		private int m_BackgroundGlowIntensity;
		public int BackgroundGlowIntensity { get => m_BackgroundGlowIntensity; set => m_BackgroundGlowIntensity = value; }
	}
}
