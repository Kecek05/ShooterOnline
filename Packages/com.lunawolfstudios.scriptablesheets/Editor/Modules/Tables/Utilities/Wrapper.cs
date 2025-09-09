using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Tables
{
	[System.Serializable]
	public class Wrapper
	{
		[SerializeField]
		private char m_Open;
		public char Open { get => m_Open; set => m_Open = value; }

		[SerializeField]
		private char m_Close;
		public char Close { get => m_Close; set => m_Close = value; }

		public Wrapper(char openAndClose)
		{
			m_Open = openAndClose;
			m_Close = openAndClose;
		}

		public Wrapper(char open, char close)
		{
			m_Open = open;
			m_Close = close;
		}

		public string Wrap(string value)
		{
			return $"{m_Open}{value}{m_Close}";
		}

		public string WrapInverse(string value)
		{
			return $"{m_Close}{value}{m_Open}";
		}

		public string EscapeContent(string content, string escapedOpen, string escapedClose)
		{
			if (string.IsNullOrEmpty(content))
			{
				return content;
			}
			var escapedContent = content.Replace(m_Open.ToString(), escapedOpen);
			if (m_Open != m_Close)
			{
				escapedContent = escapedContent.Replace(m_Close.ToString(), escapedClose);
			}
			return escapedContent;
		}

		public string UnescapeContent(string content, string escapedOpen, string escapedClose)
		{
			if (string.IsNullOrEmpty(content))
			{
				return content;
			}
			var unescapedContent = content.Replace(escapedOpen, m_Open.ToString());
			if (escapedOpen != escapedClose)
			{
				unescapedContent = unescapedContent.Replace(escapedClose, m_Close.ToString());
			}
			return unescapedContent;
		}
	}
}
