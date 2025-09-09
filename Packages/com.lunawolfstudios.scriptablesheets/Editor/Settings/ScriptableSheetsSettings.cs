using LunaWolfStudiosEditor.ScriptableSheets.Importers;
using LunaWolfStudiosEditor.ScriptableSheets.Layout;
using LunaWolfStudiosEditor.ScriptableSheets.Scanning;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets.Settings
{
#if UNITY_2020_1_OR_NEWER

	// We cannot persist a ScriptableSingleton in Unity versions prior to 2020 because FilePathAttribute is internal.
	// https://forum.unity.com/threads/missing-documentation-for-scriptable-singleton.292754/
	[FilePath("UserSettings/ScriptableSheetsSettings.asset", FilePathAttribute.Location.ProjectFolder)]
#endif
	public class ScriptableSheetsSettings : ScriptableSingleton<ScriptableSheetsSettings>
	{
		[SerializeField]
		private DataTransferSettings m_DataTransfer;
		public DataTransferSettings DataTransfer => m_DataTransfer;

		[SerializeField]
		private ObjectManagementSettings m_ObjectManagement;
		public ObjectManagementSettings ObjectManagement => m_ObjectManagement;

		[SerializeField]
		private UserInterfaceSettings m_UserInterface;
		public UserInterfaceSettings UserInterface => m_UserInterface;

		[SerializeField]
		private WorkloadSettings m_Workload;
		public WorkloadSettings Workload => m_Workload;

		[SerializeField]
		private ExperimentalSettings m_Experimental;
		public ExperimentalSettings Experimental => m_Experimental;

		[SerializeField]
		private List<GoogleSheetsImporter> m_GoogleSheetsImporters = new List<GoogleSheetsImporter>();
		public List<GoogleSheetsImporter> GoogleSheetsImporters => m_GoogleSheetsImporters;

		[SerializeField]
		private string m_WindowSessionStates;

		private bool m_IsQuitting;
		private bool m_WasWindowDestroyedThisUpdate;

		private ScanOption m_PreviousScanOption;
		private string[] m_PreviousScanPaths;
		private bool m_PreviousRootPrefabsOnly;
		private bool m_PreviousCaseSensitive;
		private bool m_PreviousStartsWith;

		private float m_PreviousHiglightAlpha;
		private bool m_PreviousHighlightSelectedRow;
		private bool m_PreviousHighlightSelectedColumn;
		private HeaderFormat m_PreviousHeaderFormat;
		private bool m_PreviousLockNames;
		private int m_PreviousRowLineHeight;
		private bool m_PreviousShowAssetPreviews;
		private ScaleMode m_PreviousPreviewScaleMode;
		private bool m_PreviousShowRowIndex;
		private bool m_PreviousShowColumnIndex;
		private bool m_PreviousShowChildren;
		private bool m_PreviousShowArrays;
		private bool m_PreviousOverrideArraySize;
		private int m_PreviousArraySize;
		private bool m_PreviousShowAssetPath;
		private bool m_PreviousShowGuids;
		private bool m_PreviousShowReadOnly;
		private bool m_PreviousSubAssetFilters;

		private int m_PreviousMaxIterations;
		private int m_PreviousRowsPerPage;
		private int m_PreviousVisibleColumnLimit;

#if UNITY_2020_1_OR_NEWER
		private string m_FilePath;
		private string m_FolderPath;

		private void Awake()
		{
			if (!System.IO.File.Exists(GetFilePath()))
			{
				ResetDefaultsAndSave();
			}
			m_FilePath = System.IO.Path.Combine(Application.dataPath, "..", GetFilePath());
			m_FolderPath = System.IO.Path.GetDirectoryName(m_FilePath);
		}

#endif

		private void OnEnable()
		{
			CacheReactiveSettings();
			EditorApplication.wantsToQuit += OnEditorApplicationWantsToQuit;
			EditorApplication.update += OnEditorApplicationUpdate;
		}

		private void OnDisable()
		{
			// Save window session state when assembly reloads.
			if (!m_IsQuitting)
			{
				SaveWindowSessions();
			}
		}

		private bool OnEditorApplicationWantsToQuit()
		{
			try
			{
				m_IsQuitting = true;
				SaveWindowSessions();
#if UNITY_2020_1_OR_NEWER
				Save(true);
#endif
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"An error occured while trying to save {nameof(ScriptableSheetsSettings)}.\n{ex.Message}");
			}
			return true;
		}

		private void OnEditorApplicationUpdate()
		{
			m_WasWindowDestroyedThisUpdate = false;
		}

		private void SaveWindowSessions()
		{
			if (ScriptableSheetsEditorWindow.Instances.Count > 0)
			{
				var windowSessionStates = GetWindowSessionStates();
				if (windowSessionStates == null)
				{
					windowSessionStates = new HashSet<WindowSessionState>();
				}
				foreach (var window in ScriptableSheetsEditorWindow.Instances)
				{
					windowSessionStates.Add(window.GetWindowSessionState());
				}
				// Remove duplicate session caches that have the same instance id. Take the last group which should be the newest if renamed.
				windowSessionStates = new HashSet<WindowSessionState>(windowSessionStates.GroupBy(s => s.InstanceId).Select(g => g.Last()));
				m_WindowSessionStates = JsonConvert.SerializeObject(windowSessionStates);
			}
		}

		public void WindowDestroyed()
		{
			// When the user maximizes an editor window then other docked editor windows are destroyed.
			// https://forum.unity.com/threads/how-do-i-prevent-docked-editorwindows-being-destroyed-when-pressing-play-with-maximize-on-play-on.276970/
			if (!m_IsQuitting && !m_WasWindowDestroyedThisUpdate)
			{
				// Save the session states for each window that was destroyed this update so they can be restored when unmaximized.
				SaveWindowSessions();
				m_WasWindowDestroyedThisUpdate = true;
			}
		}

		public HashSet<WindowSessionState> GetWindowSessionStates()
		{
			if (!string.IsNullOrWhiteSpace(m_WindowSessionStates))
			{
				try
				{
					var windowSessionStates = JsonConvert.DeserializeObject<HashSet<WindowSessionState>>(m_WindowSessionStates);
					return windowSessionStates;
				}
				catch (System.Exception ex)
				{
					Debug.LogWarning($"Failed to deserialize {nameof(WindowSessionState)} JSON '{m_WindowSessionStates}'. {ex.Message}.");
				}
			}
			return null;
		}

		public HashSet<WindowSessionState> SaveAndGetWindowSessionStates()
		{
			SaveWindowSessions();
			return GetWindowSessionStates();
		}

		public void DeleteWindowSessionState(WindowSessionState windowSessionState)
		{
			var windowSessionStates = GetWindowSessionStates();
			if (windowSessionStates == null || windowSessionStates.Count <= 0)
			{
				Debug.LogWarning($"Cannot delete {nameof(WindowSessionState)} {windowSessionState.Title}. {nameof(windowSessionStates)} was null or empty.");
				return;
			}
			windowSessionStates.RemoveWhere(s => s.InstanceId == windowSessionState.InstanceId);
			m_WindowSessionStates = JsonConvert.SerializeObject(windowSessionStates);
		}

		public WindowSessionState LoadWindowSessionStateFromWindow(ScriptableSheetsEditorWindow window)
		{
			var windowSessionStates = GetWindowSessionStates();
			WindowSessionState windowSessionState = null;
			if (windowSessionStates != null && windowSessionStates.Count > 0)
			{
				var windowPosition = window.position.ToString();
				var instanceId = window.GetInstanceID();
				// First search by instance id and position. We include position here as well because Unity's instance id can be unreliable and change.
				windowSessionState = windowSessionStates.FirstOrDefault(s => s.InstanceId == instanceId && s.Position == windowPosition);
				if (windowSessionState == null)
				{
					// If no valid instance ids were found then look for a matching position.
					windowSessionState = windowSessionStates.FirstOrDefault(s => s.Position == windowPosition);
					if (windowSessionState == null)
					{
						// Find the first window session state that isn't already in use.
						var activeInstanceIds = new HashSet<int>(ScriptableSheetsEditorWindow.Instances.Select(i => i.GetInstanceID()));
						windowSessionState = windowSessionStates.FirstOrDefault(s => !activeInstanceIds.Contains(s.InstanceId));
					}
				}
			}
			return LoadWindowSessionState(windowSessionState, windowSessionStates);
		}

		public WindowSessionState LoadWindowSessionState(WindowSessionState windowSessionState, HashSet<WindowSessionState> windowSessionStates = null)
		{
			if (windowSessionStates == null)
			{
				windowSessionStates = GetWindowSessionStates();
			}
			// Use default state if window session state is null.
			if (windowSessionState == null)
			{
				windowSessionState = new WindowSessionState
				{
					SelectableSheetAssets = SheetAsset.Default,
					SelectedSheetAsset = SheetAsset.ScriptableObject,
					SelectedTypeIndex = 0,
					NewAmount = 1,
					SearchInput = string.Empty
				};
			}

			if (windowSessionState.PinnedIndexSets == null)
			{
				windowSessionState.PinnedIndexSets = new Dictionary<SheetAsset, HashSet<int>>();
			}

			// Initialize any new or null SheetAsset sets.
			foreach (SheetAsset sheetAsset in System.Enum.GetValues(typeof(SheetAsset)))
			{
				// The set can become null if the file was edited manually.
				if (!windowSessionState.PinnedIndexSets.TryGetValue(sheetAsset, out var set) || set == null)
				{
					windowSessionState.PinnedIndexSets[sheetAsset] = new HashSet<int>();
				}
			}

			// Create TableLayout Dictionary for previous Window Session States.
			if (windowSessionState.TableLayouts == null)
			{
				windowSessionState.TableLayouts = new Dictionary<string, TableLayout>();
			}

			// Remove the found window session state so it's not loaded by another docked window at the same position and so the instance id is overwritten next save.
			if (windowSessionStates != null && windowSessionStates.Count > 0)
			{
				windowSessionStates.Remove(windowSessionState);
				m_WindowSessionStates = JsonConvert.SerializeObject(windowSessionStates);
			}

			return windowSessionState;
		}

		public void DrawGUI(bool isSeparateWindow)
		{
			Undo.RecordObject(this, $"{nameof(ScriptableSheetsSettings)}");

			var serializedObject = new SerializedObject(this);
			m_DataTransfer.DrawGUI(serializedObject);
			m_ObjectManagement.DrawGUI(serializedObject);
			m_UserInterface.DrawGUI(serializedObject);
			m_Workload.DrawGUI(serializedObject);
			m_Experimental.DrawGUI(serializedObject);

			SheetLayout.DrawHorizontalLine();

			EditorGUILayout.BeginHorizontal();

			var googleSheetsImportersProperty = serializedObject.FindProperty(nameof(m_GoogleSheetsImporters));
			EditorGUILayout.PropertyField(googleSheetsImportersProperty, SettingsContent.Label.GoogleSheetsImporters, true);
			if (GUILayout.Button(SettingsContent.Button.ScanImporters, SheetLayout.InlineButton))
			{
				m_GoogleSheetsImporters.Clear();
				var guids = AssetDatabase.FindAssets($"t:{nameof(GoogleSheetsImporter)}");
				foreach (var guid in guids)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					var asset = AssetDatabase.LoadAssetAtPath<GoogleSheetsImporter>(path);
					if (asset != null)
					{
						m_GoogleSheetsImporters.Add(asset);
					}
				}
			}

			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();

			SheetLayout.DrawHorizontalLine();

#if UNITY_2020_1_OR_NEWER
			if (m_Workload.AutoSave || GUILayout.Button(SettingsContent.Button.SaveChangesToDisk))
			{
				Save(true);
			}
			if (GUILayout.Button(SettingsContent.Button.OpenFile))
			{
				Application.OpenURL(m_FilePath);
			}
			if (GUILayout.Button(SettingsContent.Button.OpenFolder))
			{
				Application.OpenURL(m_FolderPath);
			}
#endif
			if (GUILayout.Button(SettingsContent.Button.ResetDefaults))
			{
				if (EditorUtility.DisplayDialog("Scriptable Sheets", "Reset settings to default values?", "Confirm", "Cancel"))
				{
					ResetDefaultsAndSave();
				}
			}
			if (!isSeparateWindow && GUILayout.Button(SettingsContent.Button.OpenWindow))
			{
				ScriptableSheetsSettingsEditorWindow.ShowWindow();
			}

			// Repaint and refresh column layouts immediately as reactive settings change.
			var hasScanOptionChanged = m_PreviousScanOption != m_ObjectManagement.Scan.Option;
			var hasScanPathChanged = !m_PreviousScanPaths.SequenceEqual(m_ObjectManagement.Scan.GetScanPaths());
			var hasRootPrefabsOnlyChanged = m_PreviousRootPrefabsOnly != m_ObjectManagement.Scan.RootPrefabsOnly;
			var hasHeaderFormatChanged = m_PreviousHeaderFormat != m_UserInterface.HeaderFormat;
			var hasShowRowIndexChanged = m_PreviousShowRowIndex != m_UserInterface.ShowRowIndex;
			var hasShowColumnIndexChanged = m_PreviousShowColumnIndex != m_UserInterface.ShowColumnIndex;
			var hasShowChildrenChanged = m_PreviousShowChildren != m_UserInterface.ShowChildren;
			var hasShowArraysChanged = m_PreviousShowArrays != m_UserInterface.ShowArrays;
			var hasOverrideArraySizeChanged = m_PreviousOverrideArraySize != m_UserInterface.OverrideArraySize;
			var hasArraySizeChanged = m_PreviousArraySize != m_UserInterface.ArraySize;
			var hasShowAssetPathChanged = m_PreviousShowAssetPath != m_UserInterface.ShowAssetPath;
			var hasShowGuidChanged = m_PreviousShowGuids != m_UserInterface.ShowGuid;
			var hasShowReadOnlyChanged = m_PreviousShowReadOnly != m_UserInterface.ShowReadOnly;
			var hasMaxIterationsChanged = m_PreviousMaxIterations != m_Workload.MaxIterations;
			var hasVisibleColumnLimitChanged = m_PreviousVisibleColumnLimit != m_Workload.VisibleColumnLimit;

			var needsColumnRefresh = hasScanOptionChanged || hasScanPathChanged
				|| hasHeaderFormatChanged || hasShowRowIndexChanged
				|| hasShowColumnIndexChanged || hasShowChildrenChanged
				|| hasShowArraysChanged || hasOverrideArraySizeChanged || hasArraySizeChanged
				|| hasShowGuidChanged || hasShowAssetPathChanged
				|| hasShowReadOnlyChanged || hasMaxIterationsChanged || hasVisibleColumnLimitChanged;

			var hasCaseSensitiveChanged = m_PreviousCaseSensitive != m_ObjectManagement.Search.CaseSensitive;
			var hasStartsWithChanged = m_PreviousStartsWith != m_ObjectManagement.Search.StartsWith;
			var hasHighlightAlphaChanged = m_PreviousHiglightAlpha != m_UserInterface.TableNav.HighlightAlpha;
			var hasHighlightSelectedRowChanged = m_PreviousHighlightSelectedRow != m_UserInterface.TableNav.HighlightSelectedRow;
			var hasHighlightSelectedColumnChanged = m_PreviousHighlightSelectedColumn != m_UserInterface.TableNav.HighlightSelectedColumn;
			var hasLockNamesChanged = m_PreviousLockNames != m_UserInterface.LockNames;
			var hasRowLineHeightChanged = m_PreviousRowLineHeight != m_UserInterface.RowLineHeight;
			var hasShowAssetPreviewsChanged = m_PreviousShowAssetPreviews != m_UserInterface.AssetPreview.Show;
			var hasPreviewScaleModeChanged = m_PreviousPreviewScaleMode != m_UserInterface.AssetPreview.ScaleMode;
			var hasRowsPerPageChanged = m_PreviousRowsPerPage != m_Workload.RowsPerPage;
			var hasSubAssetFiltersChanged = m_PreviousSubAssetFilters != m_UserInterface.SubAssetFilters;

			var needsRepaint = needsColumnRefresh || hasRootPrefabsOnlyChanged || hasCaseSensitiveChanged
				|| hasStartsWithChanged || hasHighlightAlphaChanged
				|| hasHighlightSelectedRowChanged || hasHighlightSelectedColumnChanged || hasLockNamesChanged
				|| hasRowLineHeightChanged || hasShowAssetPreviewsChanged || hasPreviewScaleModeChanged
				|| hasRowsPerPageChanged || hasSubAssetFiltersChanged;

			if (needsRepaint)
			{
				foreach (var window in ScriptableSheetsEditorWindow.Instances)
				{
					if (hasScanPathChanged)
					{
						if (m_ObjectManagement.Scan.Option != ScanOption.Assembly)
						{
							window.ResetSelectedType();
						}
						window.ScanObjects();
					}
					else if (hasScanOptionChanged || hasRootPrefabsOnlyChanged)
					{
						window.ScanObjects();
					}
					if (needsColumnRefresh)
					{
						window.ForceRefreshColumnLayout();
					}
					window.Repaint();
				}
			}

			CacheReactiveSettings();
		}

		private void CacheReactiveSettings()
		{
			m_PreviousScanOption = m_ObjectManagement.Scan.Option;
			m_PreviousScanPaths = m_ObjectManagement.Scan.GetScanPaths();
			m_PreviousRootPrefabsOnly = m_ObjectManagement.Scan.RootPrefabsOnly;
			m_PreviousCaseSensitive = m_ObjectManagement.Search.CaseSensitive;
			m_PreviousStartsWith = m_ObjectManagement.Search.StartsWith;

			m_PreviousHiglightAlpha = m_UserInterface.TableNav.HighlightAlpha;
			m_PreviousHighlightSelectedRow = m_UserInterface.TableNav.HighlightSelectedRow;
			m_PreviousHighlightSelectedColumn = m_UserInterface.TableNav.HighlightSelectedColumn;
			m_PreviousHeaderFormat = m_UserInterface.HeaderFormat;
			m_PreviousLockNames = m_UserInterface.LockNames;
			m_PreviousRowLineHeight = m_UserInterface.RowLineHeight;
			m_PreviousShowAssetPreviews = m_UserInterface.AssetPreview.Show;
			m_PreviousPreviewScaleMode = m_UserInterface.AssetPreview.ScaleMode;
			m_PreviousShowRowIndex = m_UserInterface.ShowRowIndex;
			m_PreviousShowColumnIndex = m_UserInterface.ShowColumnIndex;
			m_PreviousShowChildren = m_UserInterface.ShowChildren;
			m_PreviousShowArrays = m_UserInterface.ShowArrays;
			m_PreviousOverrideArraySize = m_UserInterface.OverrideArraySize;
			m_PreviousArraySize = m_UserInterface.ArraySize;
			m_PreviousShowAssetPath = m_UserInterface.ShowAssetPath;
			m_PreviousShowGuids = m_UserInterface.ShowGuid;
			m_PreviousShowReadOnly = m_UserInterface.ShowReadOnly;

			m_PreviousMaxIterations = m_Workload.MaxIterations;
			m_PreviousRowsPerPage = m_Workload.RowsPerPage;
			m_PreviousVisibleColumnLimit = m_Workload.VisibleColumnLimit;
		}

		private void ResetDefaultsAndSave()
		{
			m_DataTransfer = new DataTransferSettings();
			m_ObjectManagement = new ObjectManagementSettings();
			m_UserInterface = new UserInterfaceSettings();
			m_Workload = new WorkloadSettings();
			m_Experimental = new ExperimentalSettings();
			m_GoogleSheetsImporters = new List<GoogleSheetsImporter>();

#if UNITY_2020_1_OR_NEWER
			Save(true);
#endif
		}
	}
}
