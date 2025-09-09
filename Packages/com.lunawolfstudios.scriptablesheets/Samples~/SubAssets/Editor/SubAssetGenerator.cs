using LunaWolfStudios.ScriptableSheets.Samples.SubAssets;
using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Samples.SubAssets
{
	public class SubAssetGenerator
	{
		[MenuItem("Assets/Create/Scriptable Sheets/Color Theme")]
		public static void CreateColorTheme()
		{
			var colorTheme = ScriptableObject.CreateInstance<ColorTheme>();
			colorTheme.Description = "This is a color theme.";

			var path = "Assets/ColorTheme.asset";
			AssetDatabase.CreateAsset(colorTheme, path);

			var backgroundColor = ScriptableObject.CreateInstance<BackgroundColors>();
			backgroundColor.name = "BackgroundColors";
			colorTheme.BackgroundColor = backgroundColor;
			AssetDatabase.AddObjectToAsset(backgroundColor, colorTheme);

			var foregroundColor = ScriptableObject.CreateInstance<ForegroundColors>();
			foregroundColor.name = "ForegroundColors";
			colorTheme.ForegroundColor = foregroundColor;
			AssetDatabase.AddObjectToAsset(foregroundColor, colorTheme);

			var textColor = ScriptableObject.CreateInstance<TextColors>();
			textColor.name = "TextColors";
			colorTheme.TextColor = textColor;
			AssetDatabase.AddObjectToAsset(textColor, colorTheme);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		[MenuItem("Assets/Create/Scriptable Sheets/Sample Main Asset with Sub Assets")]
		public static void CreateSampleMainAssetWithSubAssets()
		{
			var sampleMainAsset = ScriptableObject.CreateInstance<SampleMainAsset>();
			sampleMainAsset.Description = "This is a sample Main Asset.";

			var path = "Assets/MainAsset.asset";
			AssetDatabase.CreateAsset(sampleMainAsset, path);

			sampleMainAsset.SubAssets = new SampleSubAsset[3];
			for (var i = 0; i < 3; i++)
			{
				var sampleSubAsset = ScriptableObject.CreateInstance<SampleSubAsset>();
				sampleSubAsset.name = $"SubAsset{i}";
				sampleSubAsset.Description = $"This is sample Sub Asset.";
				sampleSubAsset.Index = i;
				sampleSubAsset.SomeBoolValue = i % 2 == 0;
				sampleMainAsset.SubAssets[i] = sampleSubAsset;
				AssetDatabase.AddObjectToAsset(sampleSubAsset, sampleMainAsset);
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}