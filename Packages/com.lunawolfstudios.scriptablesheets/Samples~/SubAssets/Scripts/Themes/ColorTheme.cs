using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class ColorTheme : ScriptableObject
	{
		[SerializeField]
		private string m_Description;
		public string Description { get => m_Description; set => m_Description = value; }

		[SerializeField]
		private BackgroundColors m_BackgroundColor;
		public BackgroundColors BackgroundColor { get => m_BackgroundColor; set => m_BackgroundColor = value; }

		[SerializeField]
		private ForegroundColors m_ForegroundColor;
		public ForegroundColors ForegroundColor { get => m_ForegroundColor; set => m_ForegroundColor = value; }

		[SerializeField]
		private TextColors m_TextColor;
		public TextColors TextColor { get => m_TextColor; set => m_TextColor = value; }
	}
}
