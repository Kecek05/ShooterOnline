using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets.Importers
{
	[Serializable]
	[HelpURL("https://github.com/LunaWolfStudios/ScriptableSheetsDocs/blob/main/DOCUMENTATION.md#google-sheets-importers")]
	[CreateAssetMenu(fileName = "NewGoogleSheetsImporter", menuName = "Scriptable Sheets/Google Sheets Importer")]
	public class GoogleSheetsImporter : ScriptableObject
	{
		[Tooltip("The fully qualified type name that this importer targets. Required for non-ScriptableObject types like UnityEngine.GameObject for prefabs.")]
		[SerializeField]
		private string m_FullTypeName;
		public string FullTypeName { get => m_FullTypeName; set => m_FullTypeName = value; }

		[Tooltip("The MonoScript that defines the Object type this importer targets. Required if the full type name is not set.")]
		[SerializeField]
		private MonoScript m_MonoScript;
		public MonoScript MonoScript { get => m_MonoScript; set => m_MonoScript = value; }

		[Tooltip("Optional main asset to group this importer under. Useful for organizing Sub Assets.")]
		[SerializeField]
		private Object m_MainAsset;
		public Object MainAsset { get => m_MainAsset; set => m_MainAsset = value; }

		[Tooltip("Optional Scriptable Sheets window name that this importer targets. If set, this importer is only used when the window name matches.")]
		[SerializeField]
		private string m_WindowName;
		public string WindowName { get => m_WindowName; set => m_WindowName = value; }

		[Tooltip("The Sheet ID from the Google Sheets URL. The Google Sheets URL must be accessible via a shared link.")]
		[SerializeField]
		private string m_SheetId;
		public string SheetId { get => m_SheetId; set => m_SheetId = value; }

		[Tooltip("The name of the sheet tab inside the Google Sheet. This is case-sensitive.")]
		[SerializeField]
		private string m_SheetName;
		public string SheetName { get => m_SheetName; set => m_SheetName = value; }

		public string Url => $"https://docs.google.com/spreadsheets/d/{m_SheetId}/gviz/tq?tqx=out:csv&sheet={m_SheetName}";

		public async Task<string> GetCsvDataAsync()
		{
			var sheetName = m_SheetName;
			using var httpClient = new HttpClient();
			try
			{
				Debug.Log($"Getting '{sheetName}' CSV data from '{Url}'. Using {nameof(GoogleSheetsImporter)} {name}.");
				var csvData = await httpClient.GetStringAsync(Url);
				return csvData;
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to get '{sheetName}' CSV data from '{Url}'. Using {nameof(GoogleSheetsImporter)} {name}.\n{e.Message}");
			}
			return string.Empty;
		}

		public bool IsTypeMatch(Type type, MonoScript monoScript)
		{
			return type.FullName == FullTypeName || (monoScript != null && monoScript == m_MonoScript);
		}

		public bool IsValidSheetId()
		{
			return !string.IsNullOrWhiteSpace(SheetId);
		}

		public bool IsValidSheetName()
		{
			return !string.IsNullOrWhiteSpace(SheetName);
		}

		public string GetInvalidSheetIdWarning()
		{
			return $"Please set the {nameof(SheetId)} field. This is the unique identifier found in your Google Sheets URL.";
		}

		public string GetInvalidSheetNameWarning()
		{
			return $"Please set the {nameof(SheetName)} field. This should match a sheet tab name within your Google Sheet.";
		}
	}
}