using System.Collections.Generic;

namespace LunaWolfStudiosEditor.ScriptableSheets.Tables
{
	public enum WrapOption
	{
		None = 0,
		DoubleQuotes = 1,
		SingleQuotes = 2,
		CurlyBraces = 3,
		SquareBrackets = 4,
		Parentheses = 5,
		AngleBrackets = 6,
	}

	public static class WrapOptionExtensions
	{
		private static readonly HashSet<WrapOption> JsonSafeOptions = new HashSet<WrapOption>()
		{
			WrapOption.None,
			WrapOption.SingleQuotes,
			WrapOption.Parentheses,
			WrapOption.AngleBrackets,
		};

		public static bool IsJsonUnsafe(this WrapOption wrapOption)
		{
			return !JsonSafeOptions.Contains(wrapOption);
		}
	}
}
