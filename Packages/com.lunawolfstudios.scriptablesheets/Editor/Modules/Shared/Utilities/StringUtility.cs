using System;
using System.Text;

namespace LunaWolfStudiosEditor.ScriptableSheets.Shared
{
	public static class StringUtility
	{
		public static string DecodeBase64(this string text)
		{
			try
			{
				var bytes = Convert.FromBase64String(text);
				var decodedText = Encoding.UTF8.GetString(bytes);
				return decodedText;
			}
			catch (FormatException ex)
			{
				UnityEngine.Debug.LogWarning($"Error decoding '{text}'.\n{ex.Message}");
			}
			return text;
		}

		public static string EncodeBase64(this string text)
		{
			var bytes = Encoding.UTF8.GetBytes(text);
			var encodedText = Convert.ToBase64String(bytes);
			return encodedText;
		}

		public static string ExpandAll(this string text, int index, string type, int padding = 0)
		{
			return text.ExpandIndex(index, padding).ExpandType(type);
		}

		public static string ExpandIndex(this string text, int index, int padding = 0)
		{
			return text.Replace("{i}", index.ToString(new string('0', padding)));
		}

		public static string ExpandType(this string text, string type)
		{
			return text.Replace("{t}", type);
		}

		public static string GetEscapedText(this string text)
		{
			text = text.Replace("\\", "\\\\");
			text = text.Replace("\r", "\\r");
			text = text.Replace("\n", "\\n");
			text = text.Replace("\t", "\\t");
			return text;
		}

		public static string GetUnescapedText(this string text)
		{
			text = text.Replace("\\\\", "\\");
			text = text.Replace("\\r", "\r");
			text = text.Replace("\\n", "\n");
			text = text.Replace("\\t", "\t");
			return text;
		}

		public static bool MatchesSearch(this string text, string searchTerm, SearchSettings settings)
		{
			var stringComparison = settings.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
			if (settings.StartsWith)
			{
				return text.StartsWith(searchTerm, stringComparison);
			}
			else
			{
				return text.IndexOf(searchTerm, stringComparison) >= 0;
			}
		}

		public static string UnwrapLayerMask(this string text)
		{
			// Unity wraps LayerMask values with 'LayerMask(#)' when copying from the Inspector.
			return text.Replace("LayerMask(", string.Empty).Replace(")", string.Empty);
		}
	}
}
