using LunaWolfStudiosEditor.ScriptableSheets.Comparables;
using LunaWolfStudiosEditor.ScriptableSheets.Importers;
using LunaWolfStudiosEditor.ScriptableSheets.Layout;
using LunaWolfStudiosEditor.ScriptableSheets.PastePad;
using LunaWolfStudiosEditor.ScriptableSheets.Popups;
using LunaWolfStudiosEditor.ScriptableSheets.Scanning;
using LunaWolfStudiosEditor.ScriptableSheets.Settings;
using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using LunaWolfStudiosEditor.ScriptableSheets.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets
{
	public class ScriptableSheetsEditorWindow : EditorWindow, IHasCustomMenu
	{
		public static readonly List<ScriptableSheetsEditorWindow> Instances = new List<ScriptableSheetsEditorWindow>();

		private static readonly Dictionary<Type, MonoScript> s_MonoScriptCache = new Dictionary<Type, MonoScript>();

		private const string JsonExtension = "json";

		private static WindowSessionState s_NextWindowSessionStateToLoad;
		private static bool s_IsNextWindowSessionStateClone;
		private static bool s_IsNewWindowSessionState;

		private readonly ObjectScanner m_Scanner = new ObjectScanner();
		private readonly Paginator m_Paginator = new Paginator();
		private readonly TableNav<ITableProperty> m_TableNav = new TableNav<ITableProperty>();
		private readonly TableSmartPaste<ITableProperty> m_TableSmartPaste = new TableSmartPaste<ITableProperty>();

		private Object m_MainAsset;
		private int m_SelectedMainAssetIndex;
		private GoogleSheetsImporter m_GoogleSheetsImporter;

		private List<Object> m_SortedObjects = new List<Object>();
		private Type m_SelectedType;
		private Type m_PreviousSelectedType;
		private string m_NewAssetPath;
		private int m_PreviousSelectedPage = 1;

		private Table<ITableProperty> m_PropertyTable;
		private TableAction m_TableAction;
		private int[] m_CachedVisibleColumns;
		private string m_SelectedFilepath;
		private string m_ImportedFileContents;
		private bool m_IsImportJson;

		private SearchField m_SearchField;

		private MultiColumnHeaderState.Column[] m_Columns;
		private MultiColumnHeaderState m_MultiColumnHeaderState;
		private MultiColumnHeader m_MultiColumnHeader;
		private Dictionary<string, int> m_MultiColumnTooltipPaths;
		private bool m_SortingChanged;

		private Rect m_ScrollViewArea;
		private Rect m_TableScrollViewRect;
		private Vector2 m_ScrollPosition;
		private Vector2 m_SheetAssetScrollPosition;
		private Vector2 m_ObjectTypeScrollPosition;

		private ScriptableSheetsSettings m_Settings;
		private bool m_ForceRefreshColumnLayout;
		private bool m_Reinitialized;

		// Cached window session data.
		private SheetAsset m_SelectableSheetAssets;
		private SheetAsset m_SelectedSheetAsset;
		private int m_SelectedTypeIndex;
		private Dictionary<SheetAsset, HashSet<int>> m_PinnedIndexSets;
		private int m_NewAmount;
		private string m_SearchInput;
		private Dictionary<string, TableLayout> m_TableLayouts;

		private bool m_Initialized;

		[MenuItem("Window/Scriptable Sheets")]
		public static void ShowWindow()
		{
			var window = CreateInstance<ScriptableSheetsEditorWindow>();
			window.minSize = new Vector2(600, 400);
			window.Show();
		}

		private void OnEnable()
		{
			Instances.Add(this);
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			m_SearchField = new SearchField();
			m_Settings = ScriptableSheetsSettings.instance;

			WindowSessionState windowSessionState;
			if (s_IsNewWindowSessionState)
			{
				// Load a new window session state when selected from the context menu.
				// In this case null will use default settings for a new window.
				windowSessionState = m_Settings.LoadWindowSessionState(null);
				s_IsNewWindowSessionState = false;
			}
			else if (s_NextWindowSessionStateToLoad == null)
			{
				// Load this window based on its instance id and position.
				// If no matches are found then load any recently closed window.
				// If none are found then load a new window.
				windowSessionState = m_Settings.LoadWindowSessionStateFromWindow(this);
			}
			else
			{
				// Load a specific Window that was opened from the context menu.
				windowSessionState = m_Settings.LoadWindowSessionState(s_NextWindowSessionStateToLoad);
				if (!s_IsNextWindowSessionStateClone)
				{
					m_Settings.DeleteWindowSessionState(s_NextWindowSessionStateToLoad);
				}
				s_NextWindowSessionStateToLoad = null;
				s_IsNextWindowSessionStateClone = false;
			}

			m_SelectableSheetAssets = windowSessionState.SelectableSheetAssets;
			m_SelectedSheetAsset = windowSessionState.SelectedSheetAsset;
			m_SelectedTypeIndex = windowSessionState.SelectedTypeIndex;
			m_PinnedIndexSets = windowSessionState.PinnedIndexSets;
			m_NewAmount = windowSessionState.NewAmount;
			m_SearchInput = windowSessionState.SearchInput;
			m_TableLayouts = windowSessionState.TableLayouts;

			// Workaround because we cannot directly scan for objects within OnEnable.
			// This is due to a bug with calling AssetDatabase.Refresh within OnEnable when opening a window via custom Layout.
			m_Reinitialized = true;

			// Defer titleContent assignment to ensure it isn't overwritten by Unity's layout restoration system.
			// Without this, multiple windows with the same name may share the same GUIContent instance on startup, causing renaming issues.
			// But also initialize regardless incase the window is immediately destroyed from a layout change.
			InitializeTitleContent(windowSessionState.Title);
			m_Initialized = false;
			EditorApplication.delayCall += () => InitializeTitleContent(windowSessionState.Title);
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
		}

		private void OnDestroy()
		{
			// Workaround for when a single docked window is maximized then minimized, Unity briefly clones then destroys the window.
			// This ensures we're not saving each clone.
			if (m_Initialized || Instances.Count != 2 || Instances[0].titleContent.text != Instances[1].titleContent.text)
			{
				m_Settings.WindowDestroyed();
			}
			Instances.Remove(this);
		}

		private void OnInspectorUpdate()
		{
			if (m_Settings.Workload.AutoUpdate)
			{
				Repaint();
			}
		}

		private void InitializeTitleContent(string title)
		{
			titleContent = SheetsContent.Window.GetDefaultTitleContent();
			if (!string.IsNullOrWhiteSpace(title))
			{
				titleContent.text = title;
			}
			m_Initialized = true;
		}

		public void ForceRefreshColumnLayout()
		{
			m_ForceRefreshColumnLayout = true;
		}

		public WindowSessionState GetWindowSessionState()
		{
			var windowSession = new WindowSessionState()
			{
				InstanceId = GetInstanceID(),
				Title = titleContent.text,
				Position = position.ToString(),
				SelectableSheetAssets = m_SelectableSheetAssets,
				SelectedSheetAsset = m_SelectedSheetAsset,
				SelectedTypeIndex = m_SelectedTypeIndex,
				PinnedIndexSets = m_PinnedIndexSets,
				NewAmount = m_NewAmount,
				SearchInput = m_SearchInput,
				TableLayouts = m_TableLayouts,
			};
			return windowSession;
		}

		public void ResetSelectedType()
		{
			m_PreviousSelectedType = null;
			m_SelectedTypeIndex = m_PinnedIndexSets[m_SelectedSheetAsset].FirstOrDefault();
		}

		public void ScanObjects()
		{
			m_Scanner.ScanObjects(m_Settings.ObjectManagement.Scan, m_SelectedSheetAsset);
		}

		private bool IsScriptableObject()
		{
			return m_SelectedSheetAsset == SheetAsset.ScriptableObject && m_SelectedType != null && m_SelectedType.IsSubclassOf(typeof(ScriptableObject));
		}

		private void OnUndoRedoPerformed()
		{
			Repaint();
		}

		private void OnGUI()
		{
			if (m_Reinitialized || m_Settings.Workload.AutoScan)
			{
				m_Reinitialized = false;
				ScanObjects();
			}

			// Handle table nav and smart paste inputs up front to prevent the events being used.
			if (m_TableNav.UpdateFocusedCoordinate(m_PropertyTable, IsScriptableObject()))
			{
				// Repaint if there was keyboard navigation to force column highlighting for non text fields.
				if (m_TableNav.WasKeyboardNav)
				{
					Repaint();
				}
				if (!m_TableNav.IsEditingTextField)
				{
					if (m_Settings.DataTransfer.SmartPasteEnabled && m_TableSmartPaste.UpdatePasteContent())
					{
						SetTableAction(TableAction.SmartPaste);
					}
					else
					{
						m_TableSmartPaste.TryCopySingleCell(m_PropertyTable, m_TableNav.FocusedCoordinate, GetFlatFileFormatSettings());
					}
				}
			}

			if (m_Settings.Workload.Debug)
			{
				Debug.Log($"Focused cell coordinate '{m_TableNav.FocusedCoordinate}'.");
			}

			EditorGUILayout.BeginHorizontal();
			GUI.SetNextControlName(string.Empty);
			var previousSelectedSheetAsset = m_SelectedSheetAsset;
			m_SelectableSheetAssets = (SheetAsset) EditorGUILayout.EnumFlagsField(string.Empty, m_SelectableSheetAssets, SheetLayout.Property);
			if (m_SelectableSheetAssets == SheetAsset.Default)
			{
				m_SelectableSheetAssets = m_Settings.UserInterface.DefaultSheetAssets;
				m_SelectedSheetAsset = m_SelectableSheetAssets.FirstFlagOrDefault();
			}
			if (GUILayout.Button(SheetsContent.Button.Rescan, SheetLayout.InlineButton))
			{
				ScanObjects();
				EditorGUILayout.EndHorizontal();
				return;
			}
			m_SheetAssetScrollPosition = EditorGUILayout.BeginScrollView(m_SheetAssetScrollPosition, SheetLayout.DoubleLineHeight);
			EditorGUILayout.BeginHorizontal();
			foreach (SheetAsset sheetAsset in Enum.GetValues(typeof(SheetAsset)))
			{
				if (sheetAsset != SheetAsset.Default)
				{
					var isSelected = m_SelectedSheetAsset == sheetAsset;
					if (m_SelectableSheetAssets.HasFlag(sheetAsset))
					{
						var assetNameContent = SheetsContent.Label.GetAssetNameContent(sheetAsset.ToString());
						var width = GUI.skin.button.CalcSize(assetNameContent).x;
						if (GUILayout.Button(assetNameContent, SheetLayout.GetButtonStyle(isSelected), GUILayout.Width(width)))
						{
							m_SelectedSheetAsset = sheetAsset;
						}
					}
					else if (isSelected)
					{
						// If the selected sheet asset is disabled then default to the next selected sheet asset.
						m_SelectedSheetAsset = m_SelectableSheetAssets.FirstFlagOrDefault();
					}
					if (previousSelectedSheetAsset != m_SelectedSheetAsset)
					{
						ResetSelectedType();
						previousSelectedSheetAsset = m_SelectedSheetAsset;
						ScanObjects();

						// If there are no pins then use the default type when the Sheet Asset changes. Ignore for ScriptableObjects.
						if (m_SelectedSheetAsset != SheetAsset.ScriptableObject && m_PinnedIndexSets[m_SelectedSheetAsset].Count <= 0 && m_Scanner.ObjectTypes.Length > 0)
						{
							var defaultSheetAssetType = m_SelectedSheetAsset.GetDefaultType();
							for (var i = 0; i < m_Scanner.ObjectTypes.Length; i++)
							{
								if (m_Scanner.ObjectTypes[i].FullName == defaultSheetAssetType)
								{
									m_SelectedTypeIndex = i;
									break;
								}
							}
						}

						// Exit early after Scanning Objects.
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.EndScrollView();
						EditorGUILayout.EndHorizontal();
						return;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();

			var isScriptableObject = IsScriptableObject();
			if (m_Scanner.ObjectsByType.Keys.Count <= 0)
			{
				if (m_SelectedType == null && m_SelectedSheetAsset == SheetAsset.ScriptableObject && m_Scanner.ObjectTypes.Length > 0)
				{
					m_SelectedTypeIndex = 0;
					m_SelectedType = m_Scanner.ObjectTypes[m_SelectedTypeIndex];
					isScriptableObject = IsScriptableObject();
				}
				if (!isScriptableObject || m_Settings.ObjectManagement.Scan.Option != ScanOption.Assembly)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.HelpBox($"Did not find any objects of type {m_SelectedSheetAsset} under path(s):\n{m_Settings.ObjectManagement.Scan.GetJoinedScanPaths()}\nUpdate the scan path or create a new asset.", MessageType.Warning);
					m_NewAssetPath = m_Settings.ObjectManagement.Scan.GetFirstScanPath();
					m_Paginator.GoToFirstPage();
					m_SortedObjects.Clear();
					return;
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			var previousSelectedTypeIndex = m_SelectedTypeIndex;
			GUI.SetNextControlName(string.Empty);
			m_SelectedTypeIndex = EditorGUILayout.Popup(string.Empty, m_SelectedTypeIndex, m_Scanner.ObjectTypeNames);
			var activePinnedIndexSet = m_PinnedIndexSets[m_SelectedSheetAsset];
			if (m_Settings.UserInterface.AutoPin && previousSelectedTypeIndex != m_SelectedTypeIndex)
			{
				activePinnedIndexSet.Add(m_SelectedTypeIndex);
			}
			if (activePinnedIndexSet.Contains(m_SelectedTypeIndex))
			{
				if (GUILayout.Button(SheetsContent.Button.Unpin, SheetLayout.InlineButton))
				{
					activePinnedIndexSet.Remove(m_SelectedTypeIndex);
				}
			}
			else
			{
				if (GUILayout.Button(SheetsContent.Button.Pin, SheetLayout.InlineButton))
				{
					activePinnedIndexSet.Add(m_SelectedTypeIndex);
				}
			}
			if (activePinnedIndexSet.Count > 1 && GUILayout.Button(SheetsContent.Button.UnpinAll, SheetLayout.InlineButton))
			{
				activePinnedIndexSet.Clear();
			}
			EditorGUILayout.EndHorizontal();

			if (activePinnedIndexSet.Count > 0)
			{
				m_ObjectTypeScrollPosition = EditorGUILayout.BeginScrollView(m_ObjectTypeScrollPosition, SheetLayout.DoubleLineHeight);
				EditorGUILayout.BeginHorizontal();
				foreach (var index in activePinnedIndexSet)
				{
					if (index < m_Scanner.ObjectTypes.Length)
					{
						var isSelected = m_SelectedTypeIndex == index;
						var objectTypeContent = SheetsContent.Label.GetObjectTypeContent(m_Scanner.FriendlyObjectTypeNames[index], m_Scanner.ObjectTypeNames[index]);
						var width = GUI.skin.button.CalcSize(objectTypeContent).x;
						if (GUILayout.Button(objectTypeContent, SheetLayout.GetButtonStyle(isSelected), GUILayout.Width(width)))
						{
							m_SelectedTypeIndex = index;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndScrollView();
			}

			if (m_SelectedTypeIndex >= m_Scanner.ObjectTypes.Length)
			{
				m_SelectedTypeIndex = 0;
			}
			m_SelectedType = m_Scanner.ObjectTypes[m_SelectedTypeIndex];
			isScriptableObject = IsScriptableObject();
			if (!m_Scanner.ObjectsByType.TryGetValue(m_SelectedType, out List<Object> filteredObjects))
			{
				filteredObjects = new List<Object>();
				m_Scanner.ObjectsByType[m_SelectedType] = filteredObjects;
			}
			else
			{
				filteredObjects.RemoveAll(o => o == null);
			}
			MonoScript monoScript = null;
			var hasMainAssetIndexChanged = false;
			if (isScriptableObject)
			{
				if (m_Settings.UserInterface.SubAssetFilters && m_Scanner.SubAssetsByTypeAndMainAsset.TryGetValue(m_SelectedType, out var subAssetByMainAsset))
				{
					subAssetByMainAsset = subAssetByMainAsset.Where(kvp => kvp.Key != null).ToDictionary(kvp => kvp.Key, pair => pair.Value);
					if (subAssetByMainAsset.Count > 0)
					{
						var separator = subAssetByMainAsset.Count > SheetLayout.SubMenuThreshold ? '/' : '.';
						var subAssetMainAssetsAsString = subAssetByMainAsset.Keys.Select(mainAsset => mainAsset.GetType().Name + separator + mainAsset.name).ToArray();
						var previousMainAssetIndex = m_SelectedMainAssetIndex;
						GUI.SetNextControlName(string.Empty);
						m_SelectedMainAssetIndex = EditorGUILayout.Popup(string.Empty, m_SelectedMainAssetIndex, subAssetMainAssetsAsString);
						if (m_SelectedMainAssetIndex < 0 || m_SelectedMainAssetIndex >= subAssetByMainAsset.Keys.Count)
						{
							m_SelectedMainAssetIndex = 0;
						}
						hasMainAssetIndexChanged = previousMainAssetIndex != m_SelectedMainAssetIndex;
						if (hasMainAssetIndexChanged)
						{
							var tableLayout = GetTableLayout();
							tableLayout.MainAssetIndex = m_SelectedMainAssetIndex;
						}
						m_MainAsset = subAssetByMainAsset.Keys.ElementAt(m_SelectedMainAssetIndex);
						subAssetByMainAsset[m_MainAsset].RemoveAll(obj => obj == null);
						filteredObjects = subAssetByMainAsset[m_MainAsset];
					}
					else
					{
						m_Scanner.SubAssetsByTypeAndMainAsset.Remove(m_SelectedType);
						m_MainAsset = null;
						m_SelectedMainAssetIndex = 0;
					}
				}
				else
				{
					m_MainAsset = null;
					m_SelectedMainAssetIndex = 0;
				}
				if (!s_MonoScriptCache.TryGetValue(m_SelectedType, out monoScript))
				{
					// Do not allow instantiation of UnityEditorInternal types.
					if (!m_SelectedType.FullName.Contains(UnityConstants.Type.UnityEditorInternal))
					{
						var tempScriptableObject = CreateInstance(m_SelectedType);
						if (tempScriptableObject != null)
						{
							monoScript = MonoScript.FromScriptableObject(tempScriptableObject);
							DestroyImmediate(tempScriptableObject);
						}
					}
					s_MonoScriptCache.Add(m_SelectedType, monoScript);
				}
				if (monoScript != null)
				{
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.ObjectField(string.Empty, monoScript, typeof(MonoScript), false);
					EditorGUI.EndDisabledGroup();
				}
			}

			EditorGUILayout.BeginHorizontal();
			if (isScriptableObject && monoScript != null)
			{
				if (string.IsNullOrEmpty(m_NewAssetPath))
				{
					m_NewAssetPath = m_Settings.ObjectManagement.Scan.GetFirstScanPath();
				}
				if (GUILayout.Button(SheetsContent.Button.GetCreateContent(m_NewAmount), SheetLayout.InlineButton))
				{
					var confirmed = true;
					if (m_NewAmount > 9000)
					{
						confirmed = EditorUtility.DisplayDialog("It's Over 9000!!!", "What!? 9000!? There's no way that can be right!", "Send it", "Cancel");
					}
					if (confirmed)
					{
						AssetDatabase.StartAssetEditing();
						for (var i = 0; i < m_NewAmount; i++)
						{
							var selectedTypeName = m_SelectedType.Name;
							var newObjectName = m_Settings.ObjectManagement.NewObjectName;
							if (string.IsNullOrEmpty(newObjectName))
							{
								newObjectName = selectedTypeName;
							}
							var prefix = m_Settings.ObjectManagement.NewObjectPrefix;
							var suffix = m_Settings.ObjectManagement.NewObjectSuffix;
							if (m_Settings.ObjectManagement.UseExpansion)
							{
								var newObjectIndex = i + m_Settings.ObjectManagement.StartingIndex;
								var indexPadding = m_Settings.ObjectManagement.IndexPadding;
								newObjectName = newObjectName.ExpandAll(newObjectIndex, selectedTypeName, indexPadding);
								prefix = prefix.ExpandAll(newObjectIndex, selectedTypeName, indexPadding);
								suffix = suffix.ExpandAll(newObjectIndex, selectedTypeName, indexPadding);
							}
							newObjectName = $"{prefix}{newObjectName}{suffix}{UnityConstants.Extensions.Asset}";
							if (!AssetDatabase.IsValidFolder(m_NewAssetPath))
							{
								// If the path got deleted somehow, attempt to recreate it.
								Directory.CreateDirectory(m_NewAssetPath);
								AssetDatabase.Refresh();
								if (!AssetDatabase.IsValidFolder(m_NewAssetPath))
								{
									m_NewAssetPath = UnityConstants.DefaultAssetPath;
								}
							}
							var uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(m_NewAssetPath + "/" + newObjectName);
							if (m_Settings.Workload.Debug)
							{
								Debug.Log($"Creating new asset of type {m_SelectedType} with name {newObjectName} at path {uniqueAssetPath}");
							}
							var newScriptableObject = CreateInstance(m_SelectedType);
							if (m_Settings.ObjectManagement.DefaultMainAsset != null)
							{
								m_MainAsset = m_Settings.ObjectManagement.DefaultMainAsset;
							}
							if (m_MainAsset != null)
							{
								AssetDatabase.AddObjectToAsset(newScriptableObject, m_MainAsset);
								newScriptableObject.name = newObjectName.Substring(0, newObjectName.LastIndexOf('.'));

								if (!m_Scanner.SubAssetsByTypeAndMainAsset.TryGetValue(m_SelectedType, out var subAssetByMainAsset))
								{
									subAssetByMainAsset = new Dictionary<Object, List<Object>>();
									m_Scanner.SubAssetsByTypeAndMainAsset[m_SelectedType] = subAssetByMainAsset;
								}
								if (!m_Scanner.SubAssetsByTypeAndMainAsset[m_SelectedType].TryGetValue(m_MainAsset, out var subAssets))
								{
									subAssets = new List<Object>();
									m_Scanner.SubAssetsByTypeAndMainAsset[m_SelectedType][m_MainAsset] = subAssets;
									m_SelectedMainAssetIndex = m_Scanner.SubAssetsByTypeAndMainAsset[m_SelectedType].Count - 1;
								}
								subAssets.Add(newScriptableObject);
							}
							else
							{
								AssetDatabase.CreateAsset(newScriptableObject, uniqueAssetPath);
							}
							m_Scanner.ObjectsByType[m_SelectedType].Add(newScriptableObject);
						}
						AssetDatabase.StopAssetEditing();
						AssetDatabase.SaveAssets();
						AssetDatabase.Refresh();
						EditorGUILayout.EndHorizontal();
						GUIUtility.keyboardControl = 0;
						m_Paginator.SetObjectsPerPage(m_Settings.Workload.RowsPerPage);
						m_Paginator.SetTotalObjects(m_Paginator.TotalObjects + m_NewAmount);
						if (!m_Paginator.IsOnLastPage())
						{
							m_Paginator.GoToLastPage();
						}
						return;
					}
				}
				// Force reset the control name.
				GUI.SetNextControlName(string.Empty);
				m_NewAmount = EditorGUILayout.IntField(m_NewAmount, SheetLayout.PropertySmall);
				m_NewAmount = Mathf.Clamp(m_NewAmount, 1, 9999);
				try
				{
					var selectedNewAssetPath = SheetLayout.DrawAssetPathSettingGUI(GUIContent.none, SheetsContent.Button.EditNewAssetPath, m_NewAssetPath, SheetLayout.Empty);
					if (AssetDatabase.IsValidFolder(selectedNewAssetPath))
					{
						m_NewAssetPath = selectedNewAssetPath;
					}
					else
					{
						Debug.LogWarning($"'{selectedNewAssetPath}' is not a valid path for new assets.\nPlease select a folder under Assets or a mutable Package.");
					}
					// If the folder was deleted we need to reset the new asset path.
					if (!AssetDatabase.IsValidFolder(m_NewAssetPath))
					{
						m_NewAssetPath = m_Settings.ObjectManagement.Scan.GetFirstScanPath();
					}
				}
				catch (ArgumentException)
				{
					// Workaround for Unity GUI Error bug.
					GUIUtility.ExitGUI();
				}
			}

			if (filteredObjects.Count <= 0)
			{
				EditorGUILayout.EndHorizontal();
				try
				{
					if (m_MainAsset == null)
					{
						EditorGUILayout.HelpBox($"Did not find any objects of type {m_SelectedType} under path(s):\n{m_Settings.ObjectManagement.Scan.GetJoinedScanPaths()}\nUpdate the scan path or create a new asset.", MessageType.Warning);
					}
					else
					{
						var mainAssetPath = AssetDatabase.GetAssetPath(m_MainAsset);
						EditorGUILayout.HelpBox($"Did not find any objects of type {m_SelectedType} under main asset:\n{mainAssetPath}\nSelect a new main asset or create a new subasset.", MessageType.Warning);
					}
				}
				catch (ArgumentException)
				{
					// Workaround for Unity GUI Error bug.
					GUIUtility.ExitGUI();
				}
				m_Paginator.GoToFirstPage();
				m_SortedObjects.Clear();
				return;
			}
			if (filteredObjects[0] == null)
			{
				EditorGUILayout.EndHorizontal();
				// An SO was deleted externally. Repaint the window.
				Repaint();
				return;
			}
			var hasSelectedTypeChanged = m_PreviousSelectedType != m_SelectedType;
			m_PreviousSelectedType = m_SelectedType;

			if (hasSelectedTypeChanged || hasMainAssetIndexChanged)
			{
				m_TableNav.ResetTextFieldEditing();
			}

			if (string.IsNullOrEmpty(m_NewAssetPath) || hasSelectedTypeChanged || hasMainAssetIndexChanged)
			{
				var assetPath = AssetDatabase.GetAssetPath(filteredObjects[0]);
				var extension = Path.GetExtension(assetPath);
				var defaultNewAssetPath = assetPath.Replace($"{filteredObjects[0].name}{extension}", string.Empty);
				// Use default asset path for assets within immutable packages.
				if (PackageUtility.IsAssetImmutable(defaultNewAssetPath))
				{
					defaultNewAssetPath = UnityConstants.DefaultAssetPath;
				}
				else
				{
					// Check for subasset paths.
					if (defaultNewAssetPath.EndsWith(extension))
					{
						defaultNewAssetPath = defaultNewAssetPath.Substring(0, defaultNewAssetPath.LastIndexOf('/') + 1);
					}
					// In older versions of Unity IsValidFolder will return false if it ends in a forward slash.
					// https://issuetracker.unity3d.com/issues/assetdatabase-dot-isvalidfolder-returns-false-when-the-end-of-the-path-string-contains-a-directory-separator-slash
					defaultNewAssetPath = defaultNewAssetPath.TrimEnd('/');
					if (string.IsNullOrEmpty(defaultNewAssetPath) || !AssetDatabase.IsValidFolder(defaultNewAssetPath))
					{
						var firstScanPath = m_Settings.ObjectManagement.Scan.GetFirstScanPath();
						if (AssetDatabase.IsValidFolder(firstScanPath))
						{
							defaultNewAssetPath = firstScanPath;
						}
						else
						{
							defaultNewAssetPath = UnityConstants.DefaultAssetPath;
						}
					}
				}
				m_NewAssetPath = defaultNewAssetPath;
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (m_MultiColumnHeader == null || hasSelectedTypeChanged || m_ForceRefreshColumnLayout)
			{
				m_ForceRefreshColumnLayout = false;
				// Refresh flag to ensure it's set on first frame when reloading domain. Otherwise base types will not restore column layout correctly.
				isScriptableObject = IsScriptableObject();
				if (isScriptableObject && m_Settings.UserInterface.OverrideArraySize)
				{
					var tempScriptableObject = CreateInstance(m_SelectedType);
					if (tempScriptableObject != null)
					{
						RefreshColumnLayout(tempScriptableObject, hasSelectedTypeChanged);
						DestroyImmediate(tempScriptableObject);
					}
					else
					{
						RefreshColumnLayout(filteredObjects[0], hasSelectedTypeChanged);
					}
				}
				else if (isScriptableObject && filteredObjects[0].GetType() != m_SelectedType)
				{
					// Upcast Object and copy over array sizes.
					var tempScriptableObject = CreateInstance(m_SelectedType);
					if (tempScriptableObject != null)
					{
						var filteredObjectJson = EditorJsonUtility.ToJson(filteredObjects[0]);
						EditorJsonUtility.FromJsonOverwrite(filteredObjectJson, tempScriptableObject);
						RefreshColumnLayout(tempScriptableObject, hasSelectedTypeChanged);
						DestroyImmediate(tempScriptableObject);
					}
					else
					{
						RefreshColumnLayout(filteredObjects[0], hasSelectedTypeChanged);
					}
				}
				else
				{
					RefreshColumnLayout(filteredObjects[0], hasSelectedTypeChanged);
				}
			}

			if (m_SortingChanged || hasSelectedTypeChanged || hasMainAssetIndexChanged)
			{
				m_SortedObjects = m_MultiColumnHeader.GetSorted(filteredObjects);
				m_SortingChanged = false;
			}
			else
			{
				// Add new objects to the bottom of the current sort.
				m_SortedObjects = m_SortedObjects.Intersect(filteredObjects).ToList();
				m_SortedObjects.AddRange(filteredObjects.Except(m_SortedObjects));
			}

			EditorGUILayout.BeginHorizontal();
			var visibleColumnsLength = m_MultiColumnHeaderState.visibleColumns.Length;
			var excessVisibleColumns = visibleColumnsLength - m_Settings.Workload.VisibleColumnLimit;
			if (excessVisibleColumns > 0)
			{
				SetVisibleColumns(m_MultiColumnHeaderState.visibleColumns.Take(visibleColumnsLength - excessVisibleColumns).ToArray());
			}
			GUI.enabled = m_MultiColumnHeaderState.visibleColumns.Length < Mathf.Min(m_Columns.Length, m_Settings.Workload.VisibleColumnLimit);
			if (GUILayout.Button(SheetsContent.Button.ShowColumns, SheetLayout.InlineButton))
			{
				SetVisibleColumns(m_Columns.GetClampedColumns(m_Settings.Workload.VisibleColumnLimit));
			}
			GUI.enabled = true;
			var totalColumns = m_MultiColumnHeaderState.columns.Length;
			var totalVisibleColumns = m_MultiColumnHeaderState.visibleColumns.Length;
			var columnLabelContent = SheetsContent.Label.GetColumnContent(totalVisibleColumns, m_Settings.Workload.VisibleColumnLimit, totalColumns);
			var columnLabelWidth = GUI.skin.label.CalcSize(columnLabelContent).x;
			// GUI.color does not work in light theme so change the normal text color and then reset it based on light or dark theme.
			// https://docs.unity3d.com/ScriptReference/GUI-color.html
			var centerLabelStyleTextColor = SheetLayout.CenterLabelStyle.normal.textColor;
			if (totalColumns > totalVisibleColumns && totalVisibleColumns >= m_Settings.Workload.VisibleColumnLimit)
			{
				SheetLayout.CenterLabelStyle.normal.textColor = Color.yellow;
			}
			if (GUILayout.Button(columnLabelContent, SheetLayout.CenterLabelStyle, GUILayout.Width(columnLabelWidth)))
			{
				// Ensure we reset the color if the button was pressed.
				SheetLayout.CenterLabelStyle.normal.textColor = centerLabelStyleTextColor;
				ShowColumnVisibilityPopup();
			}
			SheetLayout.CenterLabelStyle.normal.textColor = centerLabelStyleTextColor;
			GUI.enabled = m_MultiColumnHeaderState.visibleColumns.Length > 1;
			if (GUILayout.Button(SheetsContent.Button.HideColumns, SheetLayout.InlineButton))
			{
				SetVisibleColumns(new int[] { 0 });
			}
			GUI.enabled = true;
			SheetLayout.DrawVerticalLine();
			if (GUILayout.Button(SheetsContent.Button.Stretch, SheetLayout.InlineButton))
			{
				m_MultiColumnHeader.ResizeToFit();
				// ResizeToFit is a Unity function that doesn't update widths immediately so we wait for a delay before caching the new widths.
				EditorApplication.delayCall -= CacheColumnLayout;
				EditorApplication.delayCall += CacheColumnLayout;
			}
			if (GUILayout.Button(SheetsContent.Button.Compact, SheetLayout.InlineButton))
			{
				m_MultiColumnHeader.ResizeToMinWidth();
				CacheColumnLayout();
			}
			if (GUILayout.Button(SheetsContent.Button.Expand, SheetLayout.InlineButton))
			{
				m_MultiColumnHeader.ResizeToHeaderWidth(SheetLayout.InlineLabelSpacing);
				CacheColumnLayout();
			}
			SheetLayout.DrawVerticalLine();
			if (GUILayout.Button(SheetsContent.Button.CopyToClipboard, SheetLayout.InlineButton))
			{
				SetTableAction(TableAction.Copy);
			}
			EditorGUI.BeginDisabledGroup(!m_TableNav.HasFocus);
			if (GUILayout.Button(SheetsContent.Button.CopyRowToClipboard, SheetLayout.InlineButton))
			{
				SetTableAction(TableAction.CopyRow);
			}
			if (GUILayout.Button(SheetsContent.Button.CopyColumnToClipboard, SheetLayout.InlineButton))
			{
				SetTableAction(TableAction.CopyColumn);
			}
			if (GUILayout.Button(SheetsContent.Button.SmartPaste, SheetLayout.InlineButton))
			{
				SetTableAction(TableAction.SmartPaste);
			}
			EditorGUI.EndDisabledGroup();
			SheetLayout.DrawVerticalLine();
			List<GoogleSheetsImporter> filteredImporters;
			if (m_Settings.UserInterface.SubAssetFilters && m_MainAsset != null)
			{
				filteredImporters = m_Settings.GoogleSheetsImporters?.Where(i => i != null && i.IsTypeMatch(m_SelectedType, monoScript) && i.MainAsset == m_MainAsset).ToList();
			}
			else
			{
				filteredImporters = m_Settings.GoogleSheetsImporters?.Where(i => i != null && i.IsTypeMatch(m_SelectedType, monoScript) && i.MainAsset == null).ToList();
			}
			if (filteredImporters == null || filteredImporters.Count <= 0)
			{
				m_GoogleSheetsImporter = null;
			}
			else
			{
				// Prioritize importers with matching window names.
				m_GoogleSheetsImporter = filteredImporters.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.WindowName) && i.WindowName == titleContent.text);
				if (m_GoogleSheetsImporter == null)
				{
					m_GoogleSheetsImporter = filteredImporters.FirstOrDefault(i => string.IsNullOrWhiteSpace(i.WindowName));
				}
			}
			EditorGUI.BeginDisabledGroup(m_GoogleSheetsImporter == null);
			var googleSheetsImporterName = m_GoogleSheetsImporter == null ? string.Empty : m_GoogleSheetsImporter.name;
			if (GUILayout.Button(SheetsContent.Button.GetGoogleSheetsImporterContent(googleSheetsImporterName), SheetLayout.InlineButton))
			{
				DownloadGoogleSheetsDataAsync();
			}
			EditorGUI.EndDisabledGroup();
			if (GUILayout.Button(SheetsContent.Button.ImportFile, SheetLayout.InlineButton))
			{
				var filePath = EditorUtility.OpenFilePanel("Import", Application.dataPath, "*");
				if (!string.IsNullOrEmpty(filePath))
				{
					m_ImportedFileContents = File.ReadAllText(filePath);
					if (!string.IsNullOrEmpty(m_ImportedFileContents))
					{
						var extension = FlatFileUtility.GetExtensionFromPath(filePath);
						if (!string.IsNullOrEmpty(extension))
						{
							// Auto detect new delimiter based on file extension.
							if (FlatFileUtility.FlatFileDelimiters.TryGetValue(extension, out string delimiter))
							{
								m_Settings.DataTransfer.SetColumnDelimiter(delimiter);
							}
							else if (extension == JsonExtension)
							{
								m_IsImportJson = true;
							}
						}
						SetTableAction(TableAction.Import);
					}
					else
					{
						Debug.LogWarning($"File at path '{filePath}' is empty.");
					}
				}
			}
			if (GUILayout.Button(SheetsContent.Button.SaveToDisk, SheetLayout.InlineButton))
			{
				var dataPath = Application.dataPath;
				var fileExtension = "dsv";
				if (FlatFileUtility.FlatFileExtensions.TryGetValue(m_Settings.DataTransfer.GetColumnDelimiter(), out string delimiterExtension))
				{
					fileExtension = delimiterExtension;
				}
				m_SelectedFilepath = EditorUtility.SaveFilePanel("Save to", dataPath, $"{m_SelectedType.Name}", fileExtension);
				if (!string.IsNullOrEmpty(m_SelectedFilepath))
				{
					SetTableAction(TableAction.Save);
				}
			}
			SheetLayout.DrawVerticalLine();
			if (GUILayout.Button(SheetsContent.Button.NewPastePad, SheetLayout.InlineButton))
			{
				PastePadEditorWindow.ShowWindow();
			}

			GUILayout.FlexibleSpace();
			GUI.SetNextControlName(string.Empty);
			m_SearchInput = m_SearchField.OnGUI(m_SearchInput, SheetLayout.SearchBar);
			var useStringEnums = m_Settings.DataTransfer.UseStringEnums;
			var ignoreEnumCasing = m_Settings.DataTransfer.IgnoreCase;
			var matchingObjects = SearchFilter.GetObjects(m_SearchInput, m_SortedObjects, m_Settings.ObjectManagement.Search, useStringEnums, ignoreEnumCasing);

			m_Paginator.SetObjectsPerPage(m_Settings.Workload.RowsPerPage);
			m_Paginator.SetTotalObjects(matchingObjects.Count);
			var totalPages = m_Paginator.GetTotalPages();
			if (totalPages > 1)
			{
				SheetLayout.DrawVerticalLine();
				var showFirstAndLastPageButtons = totalPages > SheetLayout.FirstAndLastPageThreshold;
				if (showFirstAndLastPageButtons)
				{
					EditorGUI.BeginDisabledGroup(m_Paginator.IsOnFirstPage());
					if (GUILayout.Button(SheetsContent.Button.FirstPage, SheetLayout.InlineButton))
					{
						m_Paginator.GoToFirstPage();
					}
					EditorGUI.EndDisabledGroup();
				}
				if (GUILayout.Button(SheetsContent.Button.PreviousPage, SheetLayout.InlineButton))
				{
					m_Paginator.PreviousPage();
				}
				var pageLabelContent = SheetsContent.Label.GetPageContent(m_Paginator.CurrentPage, totalPages, m_Paginator.TotalObjects);
				var pageLabelWidth = GUI.skin.label.CalcSize(pageLabelContent).x;
				EditorGUILayout.LabelField(pageLabelContent, SheetLayout.CenterLabelStyle, GUILayout.Width(pageLabelWidth));
				if (GUILayout.Button(SheetsContent.Button.NextPage, SheetLayout.InlineButton))
				{
					m_Paginator.NextPage();
				}
				if (showFirstAndLastPageButtons)
				{
					EditorGUI.BeginDisabledGroup(m_Paginator.IsOnLastPage());
					if (GUILayout.Button(SheetsContent.Button.LastPage, SheetLayout.InlineButton))
					{
						m_Paginator.GoToLastPage();
					}
					EditorGUI.EndDisabledGroup();
				}
			}
			EditorGUILayout.EndHorizontal();

			SheetLayout.DrawHorizontalLine();

			// Reset text field selection when page changes.
			if (m_PreviousSelectedPage != m_Paginator.CurrentPage)
			{
				m_PreviousSelectedPage = m_Paginator.CurrentPage;
				m_TableNav.ResetTextFieldEditing();
			}
			// Ignore pagination if we're trying to perform an action on all rows.
			var paginatedObjects = m_Settings.DataTransfer.PageRowsOnly || m_TableAction == TableAction.None ? m_Paginator.GetPageObjects(matchingObjects) : matchingObjects;
			var totalRows = paginatedObjects.Count;

			GUILayout.FlexibleSpace();
			var windowRect = GUILayoutUtility.GetLastRect();
			windowRect.width = position.width;
			windowRect.height = position.height;
			var rowHeight = EditorGUIUtility.singleLineHeight * m_Settings.UserInterface.RowLineHeight;
			var columnHeaderRowRect = new Rect(windowRect)
			{
				height = rowHeight,
			};
			m_MultiColumnHeader.OnGUI(columnHeaderRowRect, m_ScrollPosition.x);
			GUILayout.Space(rowHeight);

			// GetRect returns an empty rect during certain event types like layout.
			// In versions after 2022.3.43f1 and 6000.0.15f1. Unity starts returning a rect with float.MaxValue so we need to check that as well.
			// So validate the width and height before updating the scroll view.
			// https://forum.unity.com/threads/guilayoututility-getrect-with-inconsistent-results.8278/
			{
				var scrollViewArea = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);
				if (scrollViewArea.width > 1 && scrollViewArea.width < float.MaxValue && scrollViewArea.height > 1 && scrollViewArea.height < float.MaxValue)
				{
					m_ScrollViewArea = scrollViewArea;
					m_TableScrollViewRect = new Rect(windowRect)
					{
						height = (totalRows + SheetLayout.TableViewRowPadding) * rowHeight,
						xMax = m_MultiColumnHeaderState.widthOfAllVisibleColumns,
						yMin = windowRect.y + rowHeight
					};
					if (!m_TableNav.WasKeyboardNav && m_TableNav.HasFocus && m_PropertyTable.IsValidCoordinate(m_TableNav.PreviousFocusedCoordinate))
					{
						if (m_PropertyTable.TryGet(m_TableNav.PreviousFocusedCoordinate, out ITableProperty property))
						{
							// Force reselect property if it shifted during a layout update. Usually caused by virtualization setting.
							if (GUI.GetNameOfFocusedControl() != property.ControlName)
							{
								GUI.FocusControl(property.ControlName);
							}
						}
						else
						{
							// Reset the focused control because it is out of view. Cannot use empty string because it'll try to select a valid empty string control.
							GUI.FocusControl("null");
						}
					}
				}
			}
			m_ScrollPosition = GUI.BeginScrollView(m_ScrollViewArea, m_ScrollPosition, m_TableScrollViewRect, false, false);
			var scrollStart = new Vector2(m_ScrollViewArea.x + m_ScrollPosition.x, m_ScrollViewArea.y + m_ScrollPosition.y);
			var scrollEnd = new Vector2(scrollStart.x + m_ScrollViewArea.width, scrollStart.y + m_ScrollViewArea.height - rowHeight);

			Profiler.BeginSample("DrawTable");
			m_PropertyTable = new Table<ITableProperty>(totalRows, m_MultiColumnHeaderState.visibleColumns.Length);
			var startingRowIndex = 0;
			// Add a buffer to the starting column index for scrolling.
			var startingColumnIndex = -1;
			if (m_Settings.Workload.Virtualization && m_TableAction == TableAction.None)
			{
				var adjustedScrollStartY = scrollStart.y - rowHeight - m_ScrollViewArea.y;
				startingRowIndex = Mathf.Max(0, Mathf.CeilToInt(adjustedScrollStartY / rowHeight));

				var totalColumnWidth = 0f;
				foreach (var visibleColumn in m_MultiColumnHeaderState.visibleColumns)
				{
					var visibleColumnIndex = m_MultiColumnHeader.GetVisibleColumnIndex(visibleColumn);
					totalColumnWidth += m_MultiColumnHeader.GetColumnRect(visibleColumnIndex).width;
					if (totalColumnWidth > scrollStart.x)
					{
						break;
					}
					startingColumnIndex++;
				}
			}
			startingColumnIndex = Mathf.Max(0, startingColumnIndex);
			var lastVisibleRow = false;
			for (var rowIndex = startingRowIndex; rowIndex < totalRows; rowIndex++)
			{
				Profiler.BeginSample("DrawRow");
				var rootObject = paginatedObjects[rowIndex];
				var isFirstFilteredObject = rootObject == filteredObjects[0];
				var assetPath = AssetDatabase.GetAssetPath(rootObject);

				var rowRect = new Rect(columnHeaderRowRect);
				rowRect.y += rowHeight * (rowIndex + 1);

				var visualRowRect = new Rect(rowRect)
				{
					x = m_ScrollPosition.x
				};
				EditorGUI.DrawRect(visualRowRect, rowIndex % 2 == 0 ? SheetLayout.DarkerColor : SheetLayout.LighterColor);

				var serializedObject = new SerializedObject(rootObject);
				serializedObject.Update();

				var columnIndex = 0;
				if (columnIndex >= startingColumnIndex && m_MultiColumnHeader.IsColumnVisible(columnIndex))
				{
					var visibleColumnIndex = m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
					var columnRect = m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
					columnRect.y = rowRect.y;
					var actionRect = m_MultiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
					if (m_Settings.UserInterface.RowLineHeight > 1)
					{
						actionRect.height = EditorGUIUtility.singleLineHeight * SheetLayout.MaxActionRectLineHeight;
					}

					if (m_Settings.UserInterface.ShowRowIndex)
					{
						var rowIndexLabel = SheetsContent.Label.GetRowIndex(rowIndex);
						EditorGUI.LabelField(actionRect, rowIndexLabel);
						actionRect.x += SheetLayout.PropertyWidthSmall / 2;
					}
					var firstActionRect = new Rect(actionRect.x, actionRect.y, SheetLayout.InlineButtonWidth, actionRect.height);
					var secondActionRect = new Rect(firstActionRect.xMax + SheetLayout.InlineButtonSpacing, actionRect.y, SheetLayout.InlineButtonWidth, actionRect.height);
					if (GUI.Button(firstActionRect, SheetsContent.Button.Select))
					{
						EditorGUIUtility.PingObject(rootObject);
						Selection.activeObject = rootObject;
					}
					if (GUI.Button(secondActionRect, SheetsContent.Button.Delete))
					{
						var assetName = rootObject.name;
						var isSubAsset = AssetDatabase.IsSubAsset(rootObject);
						if (!m_Settings.UserInterface.ConfirmDelete || EditorUtility.DisplayDialog($"Delete {assetName} {(isSubAsset ? "subasset" : "asset")}?", $"{assetPath}\n\nYou cannot undo the delete assets action.", "Delete", "Cancel"))
						{
							if (!PackageUtility.IsAssetImmutable(rootObject))
							{
								if (m_Settings.Workload.Debug)
								{
									Debug.Log($"Deleting asset with name {assetName} at path {assetPath}");
								}
								if (isSubAsset)
								{
									AssetDatabase.RemoveObjectFromAsset(rootObject);
									AssetDatabase.SaveAssets();
									if (isScriptableObject && m_Settings.UserInterface.SubAssetFilters)
									{
										m_Scanner.SubAssetsByTypeAndMainAsset[m_SelectedType][m_MainAsset].Remove(rootObject);
									}
								}
								else
								{
									AssetDatabase.DeleteAsset(assetPath);
								}
								m_Scanner.ObjectsByType[m_SelectedType].Remove(rootObject);
								DestroyImmediate(rootObject, true);
								GUI.EndScrollView(true);
								GUIUtility.keyboardControl = 0;
								return;
							}
							else
							{
								Debug.LogWarning($"Unable to delete asset {assetName} at path {assetPath}\nThe asset is in an immutable folder.");
							}
						}
						actionRect.x = secondActionRect.xMax + SheetLayout.InlineButtonSpacing;
					}
				}

				columnIndex++;
				if (columnIndex >= startingColumnIndex && m_MultiColumnHeader.IsColumnVisible(columnIndex))
				{
					var visibleColumnIndex = m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
					var columnRect = m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
					columnRect.y = rowRect.y;
					var assetNameRect = m_MultiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
					// Only draw asset previews if the height is greater than 1.
					if (assetNameRect.height > EditorGUIUtility.singleLineHeight)
					{
						DrawUtility.TableAssetPreview(rootObject, assetNameRect, m_Settings.UserInterface.AssetPreview);
						assetNameRect.height = EditorGUIUtility.singleLineHeight;
					}
					var nameProperty = GetNameProperty(serializedObject);
					var nextControlName = m_TableNav.SetNextControlName(m_PropertyTable, rowIndex, visibleColumnIndex);
					EditorGUI.BeginDisabledGroup(m_Settings.UserInterface.LockNames);
					var newName = EditorGUI.TextField(assetNameRect, rootObject.name);
					EditorGUI.EndDisabledGroup();
					var nameTableProperty = new SerializedTableProperty(nameProperty.serializedObject.targetObject, nameProperty.propertyPath, nextControlName);
					m_PropertyTable.Set(rowIndex, visibleColumnIndex, nameTableProperty);
					if (newName != rootObject.name)
					{
						// Only rename if it's the main asset.
						if (assetPath.Contains($"/{rootObject.name}."))
						{
							AssetDatabase.RenameAsset(assetPath, newName);
						}
						else
						{
							rootObject.name = newName;
						}
						// Exit early for performance.
						// We prefer this over using a delayed field because the delayed field is less reliable especially when changing asset type mid edit.
						return;
					}
				}

				if (m_Settings.UserInterface.ShowAssetPath)
				{
					columnIndex++;
					if (columnIndex >= startingColumnIndex && m_MultiColumnHeader.IsColumnVisible(columnIndex))
					{
						var visibleColumnIndex = m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
						var columnRect = m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
						columnRect.y = rowRect.y;
						var assetPathRect = m_MultiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
						assetPathRect.height = EditorGUIUtility.singleLineHeight;

						EditorGUI.BeginDisabledGroup(true);
						var nextControlName = m_TableNav.SetNextControlName(m_PropertyTable, rowIndex, visibleColumnIndex);
						var assetPathTableProperty = new AssetPathTableProperty(rootObject, assetPath, nextControlName);
						EditorGUI.TextField(assetPathRect, assetPathTableProperty.GetProperty());
						m_PropertyTable.Set(rowIndex, visibleColumnIndex, assetPathTableProperty);
						EditorGUI.EndDisabledGroup();
					}
				}

				if (m_Settings.UserInterface.ShowGuid)
				{
					columnIndex++;
					if (columnIndex >= startingColumnIndex && m_MultiColumnHeader.IsColumnVisible(columnIndex))
					{
						var visibleColumnIndex = m_MultiColumnHeader.GetVisibleColumnIndex(columnIndex);
						var columnRect = m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
						columnRect.y = rowRect.y;
						var guiRect = m_MultiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
						guiRect.height = EditorGUIUtility.singleLineHeight;

						EditorGUI.BeginDisabledGroup(true);
						var nextControlName = m_TableNav.SetNextControlName(m_PropertyTable, rowIndex, visibleColumnIndex);
						var guidTableProperty = new GuidTableProperty(rootObject, assetPath, nextControlName);
						EditorGUI.TextField(guiRect, guidTableProperty.GetProperty());
						m_PropertyTable.Set(rowIndex, visibleColumnIndex, guidTableProperty);
						EditorGUI.EndDisabledGroup();
					}
				}

				var iterator = serializedObject.GetIterator();
				var lastVisibleColumn = false;
				var includeChildren = true;
				var renderingOverrides = m_Settings.Experimental.GetRenderingOverrides();
				var iterations = 0;
				while (iterator.NextVisible(includeChildren) && iterations++ < m_Settings.Workload.MaxIterations)
				{
					// Some Unity assets like Prefabs include the m_Name property in their iterator. Skip over it because we draw it separately for all Objects.
					if (iterator.propertyPath == UnityConstants.Field.Name)
					{
						continue;
					}

					var useAssetReferenceDrawer = !m_Settings.UserInterface.ShowReadOnly && iterator.IsAssetReference();
					includeChildren = m_Settings.UserInterface.ShowChildren && !useAssetReferenceDrawer;
					if (!m_MultiColumnTooltipPaths.TryGetValue(iterator.propertyPath, out int nextColumnIndex))
					{
						continue;
					}
					if (nextColumnIndex >= startingColumnIndex && (iterator.IsPropertyVisible(m_Settings.UserInterface.ShowArrays, m_Settings.UserInterface.ShowReadOnly, out bool isReadOnlyUnityField) || useAssetReferenceDrawer))
					{
						if (m_MultiColumnHeader.IsColumnVisible(nextColumnIndex))
						{
							var visibleColumnIndex = m_MultiColumnHeader.GetVisibleColumnIndex(nextColumnIndex);
							var columnRect = m_MultiColumnHeader.GetColumnRect(visibleColumnIndex);
							columnRect.y = rowRect.y;
							var propertyRect = m_MultiColumnHeader.GetCellRect(visibleColumnIndex, columnRect);
							var propertyControlName = m_TableNav.SetNextControlName(m_PropertyTable, rowIndex, visibleColumnIndex);
							var isCustomField = (isScriptableObject || serializedObject.targetObject is Component) && !isReadOnlyUnityField;
							Profiler.BeginSample("DrawProperty");
							EditorGUI.BeginDisabledGroup(!iterator.editable || isReadOnlyUnityField);
							if (renderingOverrides.Contains(iterator.propertyType))
							{
								propertyRect.height = EditorGUI.GetPropertyHeight(iterator, false);
								EditorGUI.PropertyField(propertyRect, iterator, GUIContent.none, false);
							}
							else
							{
								iterator.DrawProperty(propertyRect, rootObject, isCustomField, out bool arraySizeChanged, m_Settings.UserInterface.AssetPreview);
								if (m_Settings.UserInterface.ShowArrays && isFirstFilteredObject && arraySizeChanged)
								{
									// The first filtered Object drives the column layout. One of its array sizes changed so the column layout gets refreshed.
									m_ForceRefreshColumnLayout = true;
								}
							}
							EditorGUI.EndDisabledGroup();
							Profiler.EndSample();
							var tableProperty = new SerializedTableProperty(rootObject, iterator.propertyPath, propertyControlName);
							m_PropertyTable.Set(rowIndex, visibleColumnIndex, tableProperty);
							if (m_Settings.Workload.Virtualization)
							{
								if (propertyRect.x >= scrollEnd.x)
								{
									lastVisibleColumn = true;
								}
								if (propertyRect.y >= scrollEnd.y)
								{
									lastVisibleRow = true;
								}
								if (lastVisibleColumn && m_TableAction == TableAction.None)
								{
									break;
								}
							}
						}
					}
				}
				serializedObject.ApplyModifiedProperties();
				Profiler.EndSample();
				if (lastVisibleRow && m_TableAction == TableAction.None)
				{
					break;
				}
			}
			Profiler.EndSample();

			var tableNavVisualState = new TableNavVisualState()
			{
				MultiColumnHeader = m_MultiColumnHeader,
				ScrollViewArea = m_ScrollViewArea,
				ColumnHeaderRowRect = columnHeaderRowRect,
				RowHeight = rowHeight,
				ScrollStart = scrollStart,
				ScrollEnd = scrollEnd,
			};
			var highlightHeader = m_TableNav.UpdateFocusVisuals(m_PropertyTable, m_Settings.UserInterface.TableNav, tableNavVisualState, ref m_ScrollPosition, m_Settings.UserInterface.LockNames);

			GUI.EndScrollView(true);

			if (m_Settings.UserInterface.TableNav.HighlightSelectedColumn && highlightHeader)
			{
				var highlightColor = GUI.skin.button.focused.textColor;
				highlightColor.a = m_Settings.UserInterface.TableNav.HighlightAlpha;
				var focusedColumnHeaderRect = m_MultiColumnHeader.GetColumnRect(m_TableNav.VisualCoordinate.y);
				focusedColumnHeaderRect.x -= m_ScrollPosition.x;
				focusedColumnHeaderRect.y = columnHeaderRowRect.y;
				EditorGUI.DrawRect(focusedColumnHeaderRect, highlightColor);
			}

			// Handle file actions after property table is drawn.
			if (m_TableAction != TableAction.None)
			{
				var TableAction = m_TableAction;
				m_TableAction = TableAction.None;
				var flatFileFormatSettings = GetFlatFileFormatSettings();
				switch (TableAction)
				{
					case TableAction.Copy:
						EditorGUIUtility.systemCopyBuffer = m_PropertyTable.ToFlatFileFormat(flatFileFormatSettings);
						break;

					case TableAction.CopyRow:
						flatFileFormatSettings.FirstRowIndex = m_TableNav.FocusedCoordinate.x;
						flatFileFormatSettings.FirstRowOnly = true;
						EditorGUIUtility.systemCopyBuffer = m_PropertyTable.ToFlatFileFormat(flatFileFormatSettings);
						break;

					case TableAction.CopyColumn:
						flatFileFormatSettings.FirstColumnIndex = m_TableNav.FocusedCoordinate.y;
						flatFileFormatSettings.FirstColumnOnly = true;
						EditorGUIUtility.systemCopyBuffer = m_PropertyTable.ToFlatFileFormat(flatFileFormatSettings);
						break;

					case TableAction.CopyJson:
						EditorGUIUtility.systemCopyBuffer = m_PropertyTable.ToJsonFormat(m_Settings.DataTransfer.JsonSerializationFormat, flatFileFormatSettings);
						break;

					case TableAction.SmartPaste:
						var focusedCoordinate = m_TableNav.FocusedCoordinate;
						//  If page rows or visible columns are disabled, then apply offsets so we're still starting the paste from the selected cell.
						if (!m_Settings.DataTransfer.PageRowsOnly)
						{
							focusedCoordinate.x += (m_Paginator.CurrentPage - 1) * m_Paginator.ObjectsPerPage;
						}
						if (!m_Settings.DataTransfer.VisibleColumnsOnly)
						{
							focusedCoordinate.y = m_CachedVisibleColumns[focusedCoordinate.y];
						}
						flatFileFormatSettings.SetStartingIndex(focusedCoordinate);
						m_TableSmartPaste.Paste(m_PropertyTable, flatFileFormatSettings);
						// This notifies the selected field to immediately update its contents after a paste action.
						m_TableNav.ResetTextFieldEditing();
						break;

					case TableAction.Import:
						if (m_IsImportJson)
						{
							m_IsImportJson = false;
							m_PropertyTable.FromJsonFormat(m_ImportedFileContents, m_Settings.DataTransfer.JsonSerializationFormat, flatFileFormatSettings);
						}
						else
						{
							m_PropertyTable.FromFlatFileFormat(m_ImportedFileContents, flatFileFormatSettings);
						}
						m_ImportedFileContents = string.Empty;
						GUIUtility.keyboardControl = 0;
						break;

					case TableAction.Save:
						var extension = FlatFileUtility.GetExtensionFromPath(m_SelectedFilepath);
						var flatFileContents = string.Empty;
						if (extension == JsonExtension)
						{
							flatFileContents = m_PropertyTable.ToJsonFormat(m_Settings.DataTransfer.JsonSerializationFormat, flatFileFormatSettings);
						}
						else
						{
							// Auto detect delimiter to use based on file extension if possible.
							if (FlatFileUtility.FlatFileDelimiters.TryGetValue(extension, out string delimiter))
							{
								flatFileFormatSettings.ColumnDelimiter = delimiter;
							}
							flatFileContents = m_PropertyTable.ToFlatFileFormat(flatFileFormatSettings);
						}
						File.WriteAllText(m_SelectedFilepath, flatFileContents);
						if (m_SelectedFilepath.Contains(Application.dataPath))
						{
							AssetDatabase.Refresh();
						}
						m_SelectedFilepath = string.Empty;
						break;

					default:
						Debug.LogWarning($"Unsupported {nameof(TableAction)} {TableAction}.");
						break;
				}
			}
			if (m_CachedVisibleColumns != null && m_CachedVisibleColumns.Length > 0)
			{
				SetVisibleColumns(m_CachedVisibleColumns);
				m_CachedVisibleColumns = null;
			}

			if (m_Settings.Workload.AutoSave)
			{
				AssetDatabase.SaveAssets();
			}
		}

		private void RefreshColumnLayout(Object obj, bool hasSelectedTypeChanged)
		{
			var columns = new List<MultiColumnHeaderState.Column>();

			var extraPadding = m_Settings.UserInterface.ShowRowIndex ? SheetLayout.PropertyWidthSmall / 2 : 0;
			var width = SheetLayout.InlineButtonWidth * 2 + SheetLayout.InlineButtonSpacing * 2 + extraPadding;
			var actionColumnLabel = ColumnUtility.GetColumnIndexLabel(m_Settings.UserInterface.ShowColumnIndex, columns.Count);
			var actionColumn = ColumnUtility.CreateActionsColumn($"{actionColumnLabel}Actions", width);
			columns.Add(actionColumn);

			var isScriptableObject = IsScriptableObject();

			var serializedObject = new SerializedObject(obj);
			var nameProperty = GetNameProperty(serializedObject);
			var nameColumnLabel = ColumnUtility.GetColumnIndexLabel(m_Settings.UserInterface.ShowColumnIndex, columns.Count);
			var nameColumn = ColumnUtility.CreatePropertyColumn(nameProperty, isScriptableObject, m_Settings.UserInterface.HeaderFormat, nameColumnLabel);
			columns.Add(nameColumn);

			if (m_Settings.UserInterface.ShowAssetPath)
			{
				var assetPathColumnLabel = ColumnUtility.GetColumnIndexLabel(m_Settings.UserInterface.ShowColumnIndex, columns.Count);
				var assetPathColumn = ColumnUtility.CreateAssetPathColumn($"{assetPathColumnLabel}Asset Path");
				columns.Add(assetPathColumn);
			}

			if (m_Settings.UserInterface.ShowGuid)
			{
				var guidColumnLabel = ColumnUtility.GetColumnIndexLabel(m_Settings.UserInterface.ShowColumnIndex, columns.Count);
				var guidColumn = ColumnUtility.CreateGuidColumn($"{guidColumnLabel}GUID");
				columns.Add(guidColumn);
			}

			var iterator = serializedObject.GetIterator();
			var includeChildren = true;
			var iterations = 0;
			while (iterator.NextVisible(includeChildren) && iterations++ < m_Settings.Workload.MaxIterations)
			{
				var useAssetReferenceDrawer = !m_Settings.UserInterface.ShowReadOnly && iterator.IsAssetReference();
				includeChildren = m_Settings.UserInterface.ShowChildren && !useAssetReferenceDrawer;
				if (useAssetReferenceDrawer || iterator.IsPropertyVisible(m_Settings.UserInterface.ShowArrays, m_Settings.UserInterface.ShowReadOnly, out bool isReadOnly))
				{
					if (m_Settings.UserInterface.OverrideArraySize && iterator.propertyType == SerializedPropertyType.ArraySize)
					{
						iterator.intValue = m_Settings.UserInterface.ArraySize;
					}
					var propertyColumnLabel = ColumnUtility.GetColumnIndexLabel(m_Settings.UserInterface.ShowColumnIndex, columns.Count);
					var propertyColumn = ColumnUtility.CreatePropertyColumn(iterator, isScriptableObject, m_Settings.UserInterface.HeaderFormat, propertyColumnLabel);
					columns.Add(propertyColumn);
				}
			}

			// Remove duplicate columns like the name field for Prefabs.
			m_Columns = columns.GroupBy(c => c.headerContent.tooltip).Select(g => g.First()).ToArray();

			int[] cachedVisibleColumns;
			if (m_MultiColumnHeader == null || hasSelectedTypeChanged || m_Columns.Length != m_MultiColumnHeaderState.columns.Length)
			{
				m_MultiColumnHeaderState = new MultiColumnHeaderState(m_Columns)
				{
					visibleColumns = m_Columns.GetClampedColumns(m_Settings.Workload.VisibleColumnLimit)
				};
			}
			else
			{
				cachedVisibleColumns = m_MultiColumnHeaderState.visibleColumns;
				m_MultiColumnHeaderState = new MultiColumnHeaderState(m_Columns)
				{
					visibleColumns = cachedVisibleColumns.Take(m_Settings.Workload.VisibleColumnLimit).ToArray()
				};
			}
			m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState);
			TryRestoreTableLayout();
#if UNITY_2021_2_OR_NEWER
			// Raised when Column widths change.
			m_MultiColumnHeader.columnSettingsChanged += OnColumnSettingsChanged;
#endif
			m_MultiColumnHeader.sortingChanged += OnSortingChanged;
			m_MultiColumnHeader.visibleColumnsChanged += OnVisibleColumnsChanged;

			// Cache to map column tooltip paths directly to an index.
			m_MultiColumnTooltipPaths = m_MultiColumnHeaderState.columns
				.Select((column, index) => new KeyValuePair<string, int>(column.headerContent.tooltip, index))
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

		private SerializedProperty GetNameProperty(SerializedObject obj)
		{
			var nameProperty = obj.FindProperty(UnityConstants.Field.Name);
			// For Components attached to a Prefab we need to get the Prefab GameObject before finding the property.
			if (nameProperty == null && obj.targetObject is Component)
			{
				var targetComponent = (Component) obj.targetObject;
				var parentSerializedObject = new SerializedObject(targetComponent.gameObject);
				nameProperty = parentSerializedObject.FindProperty(UnityConstants.Field.Name);
			}
			return nameProperty;
		}

		private void SetTableAction(TableAction TableAction)
		{
			if (!m_Settings.DataTransfer.VisibleColumnsOnly)
			{
				// Temporarily restore all columns and cache current visible settings.
				m_CachedVisibleColumns = m_MultiColumnHeaderState.visibleColumns;
				SetVisibleColumns(Enumerable.Range(0, m_Columns.Length).ToArray());
			}
			m_TableAction = TableAction;
		}

		private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
		{
			m_SortingChanged = true;
			GUIUtility.keyboardControl = 0;
			var tableLayout = GetTableLayout();
			tableLayout.SortedColumnIndex = m_MultiColumnHeaderState.sortedColumnIndex;
			tableLayout.IsSortedAscending = m_MultiColumnHeader.IsSortedAscending(m_MultiColumnHeaderState.sortedColumnIndex);
		}

		private void OnColumnSettingsChanged(int column)
		{
			CacheColumnLayout();
		}

		private void OnVisibleColumnsChanged(MultiColumnHeader multiColumnHeader)
		{
			CacheColumnLayout();
		}

		private void SetVisibleColumns(int[] visibleColumns)
		{
			m_MultiColumnHeaderState.visibleColumns = visibleColumns;
			CacheColumnLayout();
		}

		private void CacheColumnLayout()
		{
			var tableLayout = GetTableLayout();
			tableLayout.ColumnCount = m_MultiColumnHeaderState.columns.Length;
			tableLayout.ColumnWidths = m_MultiColumnHeaderState.columns.Select(c => c.width).ToArray();
			tableLayout.VisibleColumns = m_MultiColumnHeaderState.visibleColumns;
		}

		private void TryRestoreTableLayout()
		{
			var tableLayoutName = m_SelectedType.FullName;
			if (!m_TableLayouts.TryGetValue(tableLayoutName, out TableLayout tableLayout))
			{
				m_MultiColumnHeader.ResizeToHeaderWidth(SheetLayout.InlineLabelSpacing);
				m_MultiColumnHeader.sortedColumnIndex = 1;
				return;
			}

			m_MultiColumnHeaderState.sortedColumnIndex = tableLayout.SortedColumnIndex;
			if (tableLayout.SortedColumnIndex < m_MultiColumnHeaderState.columns.Length)
			{
				m_MultiColumnHeaderState.sortedColumnIndex = tableLayout.SortedColumnIndex;
			}
			else
			{
				m_MultiColumnHeaderState.sortedColumnIndex = 1;
			}
			m_SelectedMainAssetIndex = tableLayout.MainAssetIndex;
			if (m_SelectedMainAssetIndex <= 0)
			{
				m_MultiColumnHeader.SetSortDirection(m_MultiColumnHeaderState.sortedColumnIndex, tableLayout.IsSortedAscending);
			}
			else
			{
				// Delay the call when there's a main asset index selected so the UI has time to update.
				EditorApplication.delayCall += () => m_MultiColumnHeader.SetSortDirection(m_MultiColumnHeaderState.sortedColumnIndex, tableLayout.IsSortedAscending);
			}

			if (tableLayout.VisibleColumns == null || tableLayout.ColumnCount != m_MultiColumnHeaderState.columns.Length)
			{
				m_MultiColumnHeader.ResizeToHeaderWidth(SheetLayout.InlineLabelSpacing);
				return;
			}

			m_MultiColumnHeaderState.visibleColumns = tableLayout.VisibleColumns;

			if (tableLayout.ColumnWidths == null || tableLayout.ColumnCount != tableLayout.ColumnWidths.Length)
			{
				m_MultiColumnHeader.ResizeToHeaderWidth(SheetLayout.InlineLabelSpacing);
				return;
			}

			for (var i = 0; i < tableLayout.ColumnCount; i++)
			{
				m_MultiColumnHeaderState.columns[i].width = tableLayout.ColumnWidths[i];
			}
		}

		private TableLayout GetTableLayout()
		{
			var tableLayoutName = m_SelectedType.FullName;
			if (!m_TableLayouts.TryGetValue(tableLayoutName, out var tableLayout))
			{
				tableLayout = new TableLayout();
				m_TableLayouts.Add(tableLayoutName, tableLayout);
			}
			return tableLayout;
		}

		private async void DownloadGoogleSheetsDataAsync()
		{
			if (!m_GoogleSheetsImporter.IsValidSheetId())
			{
				Debug.LogWarning($"Invalid {nameof(GoogleSheetsImporter)} {m_GoogleSheetsImporter.name}. {m_GoogleSheetsImporter.GetInvalidSheetIdWarning()}");
				return;
			}
			if (!m_GoogleSheetsImporter.IsValidSheetName())
			{
				Debug.LogWarning($"Invalid {nameof(GoogleSheetsImporter)} {m_GoogleSheetsImporter.name}. {m_GoogleSheetsImporter.GetInvalidSheetNameWarning()}");
				return;
			}

			var selectedType = m_SelectedType;
			var mainAsset = m_MainAsset;
			var sheetName = m_GoogleSheetsImporter.SheetName;
			var downloadUrl = m_GoogleSheetsImporter.Url;
			m_ImportedFileContents = await m_GoogleSheetsImporter.GetCsvDataAsync();
			if (!string.IsNullOrEmpty(m_ImportedFileContents))
			{
				Debug.Log($"Successfully downloaded '{sheetName}' CSV data from '{downloadUrl}'. Using {nameof(GoogleSheetsImporter)} {m_GoogleSheetsImporter.name}.");
				if (selectedType == m_SelectedType)
				{
					if (mainAsset == m_MainAsset)
					{
						m_IsImportJson = false;
						m_Settings.DataTransfer.SetRowDelimiter("\n");
						m_Settings.DataTransfer.SetColumnDelimiter(",");
						m_Settings.DataTransfer.WrapOption = WrapOption.DoubleQuotes;
						m_Settings.DataTransfer.EscapeOption = EscapeOption.Repeat;
						SetTableAction(TableAction.Import);
					}
					else
					{
						m_ImportedFileContents = string.Empty;
						var newMainAssetName = m_MainAsset == null ? "null" : m_MainAsset.name;
						Debug.LogWarning($"Selected main asset has changed. Expected {mainAsset.name} but was {newMainAssetName}. Google Sheets import will be ignored.");
					}
				}
				else
				{
					m_ImportedFileContents = string.Empty;
					Debug.LogWarning($"Selected type has changed. Expected {selectedType} but was {m_SelectedType}. Google Sheets import will be ignored.");
				}
			}
			else
			{
				Debug.LogWarning($"Imported content cannot be null or empty.");
			}
		}

		private FlatFileFormatSettings GetFlatFileFormatSettings()
		{
			var formatSettings = new FlatFileFormatSettings()
			{
				RowDelimiter = m_Settings.DataTransfer.GetRowDelimiter(),
				ColumnDelimiter = m_Settings.DataTransfer.GetColumnDelimiter(),
				// Skip Actions column.
				FirstColumnIndex = 1,
				RemoveEmptyRows = m_Settings.DataTransfer.RemoveEmptyRows,
				UseStringEnums = m_Settings.DataTransfer.UseStringEnums,
				IgnoreCase = m_Settings.DataTransfer.IgnoreCase,
				WrapOption = m_Settings.DataTransfer.WrapOption,
				EscapeOption = m_Settings.DataTransfer.EscapeOption,
				CustomEscapeSequence = m_Settings.DataTransfer.GetCustomEscapeSequence(),
			};
			if (m_Settings.DataTransfer.Headers)
			{
				formatSettings.ColumnHeaders = m_MultiColumnHeaderState.visibleColumns.Select
				(
					// Remove column index if it's in the header name.
					i => m_Settings.UserInterface.ShowColumnIndex ? m_Columns[i].headerContent.text.Remove(0, i.ToString().Length + 1) : m_Columns[i].headerContent.text
				).ToArray();
			}
			return formatSettings;
		}

		void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
		{
			var alphanumComparer = new AlphanumComparer();
			var windowSessionStates = m_Settings.SaveAndGetWindowSessionStates().OrderBy(s => s.Title, alphanumComparer).ThenBy(s => s.InstanceId).ToArray();
			if (Instances.Count < windowSessionStates.Length)
			{
				menu.AddItem(SheetsContent.Window.ContextMenu.OpenRecentSheet, false, ShowWindow);
			}
			else
			{
				menu.AddDisabledItem(SheetsContent.Window.ContextMenu.OpenRecentSheet, false);
			}
			menu.AddItem(SheetsContent.Window.ContextMenu.NewSheet, false, NewSheet);
			menu.AddItem(SheetsContent.Window.ContextMenu.RenameSheet, false, ShowRenameSheetPopup);
			menu.AddSeparator(string.Empty);
			var titleCounts = windowSessionStates.GroupBy(s => s.Title).ToDictionary(g => g.Key, g => g.Count());
			foreach (var windowSessionState in windowSessionStates)
			{
				var isActiveInstance = Instances.Any(i => i.GetInstanceID() == windowSessionState.InstanceId);
				var friendlyName = titleCounts[windowSessionState.Title] > 1 ? $"{windowSessionState.Title}/{windowSessionState.InstanceId}" : windowSessionState.Title;
				// Cannot open or delete windows that are open in the Editor.
				if (isActiveInstance)
				{
					menu.AddDisabledItem(SheetsContent.Window.ContextMenu.GetOpenSheetContent(friendlyName), false);
					menu.AddDisabledItem(SheetsContent.Window.ContextMenu.GetDeleteSheetContent(friendlyName), false);
				}
				else
				{
					menu.AddItem(SheetsContent.Window.ContextMenu.GetOpenSheetContent(friendlyName), false, () => OpenSheet(windowSessionState));
					menu.AddItem(SheetsContent.Window.ContextMenu.GetDeleteSheetContent(friendlyName), false, () => DeleteSheet(windowSessionState));
				}
				menu.AddItem(SheetsContent.Window.ContextMenu.GetCloneSheetContent(friendlyName), false, () => CloneSheet(windowSessionState));
			}
			menu.AddSeparator(string.Empty);
			menu.AddItem(SheetsContent.Window.ContextMenu.NewPastePad, false, PastePadEditorWindow.ShowWindow);
			menu.AddItem(SheetsContent.Window.ContextMenu.OpenSettings, false, ScriptableSheetsSettingsEditorWindow.ShowWindow);
			menu.AddSeparator(string.Empty);
			menu.AddItem(SheetsContent.Window.ContextMenu.EditColumnVisibility, false, ShowColumnVisibilityPopup);
			menu.AddSeparator(string.Empty);
			menu.AddItem(SheetsContent.Window.ContextMenu.Copy, false, () => SetTableAction(TableAction.Copy));
			menu.AddItem(SheetsContent.Window.ContextMenu.CopyJson, false, () => SetTableAction(TableAction.CopyJson));
		}

		private void NewSheet()
		{
			s_IsNewWindowSessionState = true;
			ShowWindow();
		}

		private void ShowRenameSheetPopup()
		{
			var anchoredRect = new Rect(position.position, PopupContent.Window.RenameSize);
			var renamePopupWindow = new InputPopupWindowContent(anchoredRect, PopupContent.Label.Rename, titleContent.text, OnRenameConfirmed);
			PopupWindow.Show(anchoredRect, renamePopupWindow);
		}

		private void OnRenameConfirmed(string input)
		{
			InitializeTitleContent(input);
			Repaint();
		}

		private void OpenSheet(WindowSessionState windowSessionState)
		{
			s_NextWindowSessionStateToLoad = windowSessionState;
			s_IsNextWindowSessionStateClone = false;
			ShowWindow();
		}

		private void CloneSheet(WindowSessionState windowSessionState)
		{
			s_NextWindowSessionStateToLoad = windowSessionState;
			s_IsNextWindowSessionStateClone = true;
			ShowWindow();
		}

		private void DeleteSheet(WindowSessionState windowSessionState)
		{
			m_Settings.DeleteWindowSessionState(windowSessionState);
		}

		private void ShowColumnVisibilityPopup()
		{
			var anchoredRect = new Rect(position.position, PopupContent.Window.ColumnVisibilityMaxSize);
			var columnLayoutPopupWindow = new ColumnVisibilityPopupWindowContent(anchoredRect, m_MultiColumnHeaderState, m_Settings.Workload.VisibleColumnLimit, SetVisibleColumns);
			PopupWindow.Show(anchoredRect, columnLayoutPopupWindow);
		}
	}
}
