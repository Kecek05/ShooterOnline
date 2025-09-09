namespace LunaWolfStudiosEditor.ScriptableSheets.Shared
{
	/// <summary>
	/// Constant field names and paths found within the Unity Editor.
	/// </summary>
	public static class UnityConstants
	{
		public const string DefaultAssetPath = "Assets";
		public const string Packages = "Packages";

		public static readonly string ArrayPropertyPath = $".{Field.Array}.{Field.ArrayData}[";
		public const string EnumPrefix = "Enum:";
		public const string ObjectWrapperJSON = "UnityEditor.ObjectWrapperJSON:";

		public static class Extensions
		{
			public const string Asset = ".asset";
		}

		public static class Field
		{
			public const string Array = "Array";
			public const string ArrayData = "data";
			public const string ArraySize = "size";
			public const string AssetRefGuid = "m_AssetGUID";
			public const string AssetRefSubObjectName = "m_SubObjectName";
			public const string AssetRefSubObjectType = "m_SubObjectType";
			public const string Direction = "m_Direction";
			public const string HorizontalAlignment = "m_HorizontalAlignment";
			public const string Layer = "m_Layer";
			public const string Name = "m_Name";
			public const string Script = "m_Script";
			public const string StaticEditorFlags = "m_StaticEditorFlags";
			public const string Tag = "m_TagString";
			public const string TextAlignment = "m_textAlignment";
			public const string VerticalAlignment = "m_VerticalAlignment";
		}

		public static class Path
		{
			public const string BuiltInExtra = "Resources/unity_builtin_extra";
		}

		public static class Type
		{
			public const string AssetReference = "AssetReference";
			public const string Double = "double";
			public const string Float = "float";
			public const string Int = "int";
			public const string Long = "long";
			public const string TMPro = "TMPro";
			public const string UInt = "uint";
			public const string ULong = "ulong";
			public const string UnityEngine = "UnityEngine";
			public const string UnityEditorInternal = "UnityEditorInternal";
			public const string UnityEngineUISlider = "UnityEngine.UI.Slider";
		}
	}
}
