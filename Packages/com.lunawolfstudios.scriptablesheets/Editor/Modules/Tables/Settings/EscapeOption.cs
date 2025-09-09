namespace LunaWolfStudiosEditor.ScriptableSheets.Tables
{
	public enum EscapeOption
	{
		None = 0,
		Backslash = 1,
		Repeat = 2,
		Custom = 3,
	}

	public static class EscapeOptionExtensions
	{
		public static string GetEscapedWrapper(this EscapeOption escapeOption, char wrapper, string custom)
		{
			switch (escapeOption)
			{
				case EscapeOption.Backslash:
					return "\\" + wrapper;

				case EscapeOption.Repeat:
					return new string(wrapper, 2);

				case EscapeOption.Custom:
					return custom + wrapper;

				case EscapeOption.None:
				default:
					return wrapper.ToString();
			}
		}
	}
}
