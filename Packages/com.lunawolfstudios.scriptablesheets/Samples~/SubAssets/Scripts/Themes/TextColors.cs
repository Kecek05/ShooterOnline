using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class TextColors : BaseColor
	{
		[SerializeField]
		private Color m_InnerTextColor = Color.white;
		public Color InnerTextColor { get => m_InnerTextColor; set => m_InnerTextColor = value; }

		[SerializeField]
		private Color m_OutlineTextColor = Color.white;
		public Color OutlineTextColor { get => m_OutlineTextColor; set => m_OutlineTextColor = value; }
	}
}
