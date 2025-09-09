using UnityEngine;

namespace LunaWolfStudios.ScriptableSheets.Samples.SubAssets
{
	[System.Serializable]
	public class BaseColor : ScriptableObject
	{
		[SerializeField]
		private Color m_DefaultColor = Color.black;
		public Color DefaultColor { get => m_DefaultColor; set => m_DefaultColor = value; }
	}
}
