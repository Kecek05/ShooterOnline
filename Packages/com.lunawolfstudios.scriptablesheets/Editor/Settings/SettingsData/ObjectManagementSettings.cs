using LunaWolfStudiosEditor.ScriptableSheets.Layout;
using LunaWolfStudiosEditor.ScriptableSheets.Scanning;
using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets
{
	[System.Serializable]
	public class ObjectManagementSettings : AbstractBaseSettings, IScriptableSettings
	{
		[SerializeField]
		private bool m_UseExpansion;
		public bool UseExpansion { get => m_UseExpansion; set => m_UseExpansion = value; }

		[SerializeField]
		private int m_StartingIndex;
		public int StartingIndex { get => m_StartingIndex; set => m_StartingIndex = value; }

		[SerializeField]
		private int m_IndexPadding;
		public int IndexPadding { get => m_IndexPadding; set => m_IndexPadding = value; }

		[SerializeField]
		private string m_NewObjectName;
		public string NewObjectName { get => m_NewObjectName; set => m_NewObjectName = value; }

		[SerializeField]
		private string m_NewObjectPrefix;
		public string NewObjectPrefix { get => m_NewObjectPrefix; set => m_NewObjectPrefix = value; }

		[SerializeField]
		private string m_NewObjectSuffix;
		public string NewObjectSuffix { get => m_NewObjectSuffix; set => m_NewObjectSuffix = value; }

		[SerializeField]
		private Object m_DefaultMainAsset;
		public Object DefaultMainAsset { get => m_DefaultMainAsset; set => m_DefaultMainAsset = value; }

		[SerializeField]
		private ScanSettings m_Scan;
		public ScanSettings Scan { get => m_Scan; set => m_Scan = value; }

		[SerializeField]
		private SearchSettings m_Search;
		public SearchSettings Search { get => m_Search; set => m_Search = value; }

		public override GUIContent FoldoutContent => SettingsContent.Foldouts.ObjectManagement;

		public ObjectManagementSettings()
		{
			Foldout = true;
			m_UseExpansion = true;
			m_StartingIndex = 0;
			m_IndexPadding = 1;
			m_NewObjectName = string.Empty;
			m_NewObjectPrefix = "New";
			m_NewObjectSuffix = string.Empty;
			m_DefaultMainAsset = null;
			m_Scan = new ScanSettings();
			m_Search = new SearchSettings();
		}

		protected override void DrawProperties(SerializedObject target)
		{
			m_UseExpansion = EditorGUILayout.Toggle(SettingsContent.Toggle.UseExpansion, m_UseExpansion);
			if (m_UseExpansion)
			{
				SheetLayout.Indent();
				m_StartingIndex = EditorGUILayout.IntField(SettingsContent.DigitField.StartingIndex, m_StartingIndex);
				m_IndexPadding = EditorGUILayout.IntSlider(SettingsContent.DigitField.IndexPadding, m_IndexPadding, 1, 10);
				SheetLayout.Unindent();
			}
			m_NewObjectName = EditorGUILayout.TextField(SettingsContent.TextField.NewObjectName, m_NewObjectName);
			m_NewObjectPrefix = EditorGUILayout.TextField(SettingsContent.TextField.NewObjectPrefix, m_NewObjectPrefix);
			m_NewObjectSuffix = EditorGUILayout.TextField(SettingsContent.TextField.NewObjectSuffix, m_NewObjectSuffix);
			var newMainAsset = EditorGUILayout.ObjectField(SettingsContent.ObjectField.DefaultMainAsset, m_DefaultMainAsset, typeof(Object), false);
			if (IsSupportedMainAsset(newMainAsset))
			{
				m_DefaultMainAsset = newMainAsset;
			}
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(SettingsContent.Label.Scanning, EditorStyles.boldLabel);
			m_Scan.Option = (ScanOption) EditorGUILayout.EnumPopup(SettingsContent.Dropdown.ScanOption, m_Scan.Option);
			m_Scan.PathOption = (ScanPathOption) EditorGUILayout.EnumPopup(SettingsContent.Dropdown.ScanPathOption, m_Scan.PathOption);
			if (m_Scan.PathOption == ScanPathOption.Default)
			{
				m_Scan.Path = SheetLayout.DrawAssetPathSettingGUI(SettingsContent.TextField.ScanPath, SettingsContent.Button.EditScanPath, m_Scan.Path, SheetLayout.DefaultLabel);
			}
			m_Scan.ShowProgressBar = EditorGUILayout.Toggle(SettingsContent.Toggle.ShowScanProgressBar, m_Scan.ShowProgressBar);
			m_Scan.RootPrefabsOnly = EditorGUILayout.Toggle(SettingsContent.Toggle.RootPrefabsOnly, m_Scan.RootPrefabsOnly);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField(SettingsContent.Label.Searching, EditorStyles.boldLabel);
			m_Search.CaseSensitive = EditorGUILayout.Toggle(SettingsContent.Toggle.CaseSensitive, m_Search.CaseSensitive);
			m_Search.StartsWith = EditorGUILayout.Toggle(SettingsContent.Toggle.StartsWith, m_Search.StartsWith);
		}

		private bool IsSupportedMainAsset(Object obj)
		{
			if (obj == null)
			{
				return true;
			}

			var assetPath = AssetDatabase.GetAssetPath(obj);
			if (string.IsNullOrEmpty(assetPath))
			{
				Debug.LogWarning($"Found invalid asset path for '{obj.name}'.");
				return false;
			}

			// Sub Assets cannot be Main Assets.
			var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
			if (obj != mainAsset)
			{
				Debug.LogWarning($"{obj.name} is not a valid Main Asset.");
				return false;
			}

			if (obj is ScriptableObject || obj is GameObject || obj is Material)
			{
				return true;
			}

			Debug.LogWarning($"Object {obj.name} of type {obj.GetType()} is not a supported Main Asset type. Only ScriptableObjects, Prefabs, and Materials are supported.");
			return false;
		}
	}
}
