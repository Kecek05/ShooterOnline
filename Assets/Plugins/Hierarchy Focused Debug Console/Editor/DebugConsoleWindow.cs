using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;



namespace HierarchyFocusedDebugConsole {

    public class DebugConsoleWindow : EditorWindow  {

        private bool _zebraStripping;
        private DebugConsoleWindow _window;
        private ConsoleLog[] _rawLogs;
        private ConsoleLog[] _filteredLogs;
        private bool[] _isFoldout;
        private GameObject _targetObject;
        private Vector2 _scrollPosition;
        private GUIStyle _messageStyle;
        private GUIStyle _callstackStyle;
        private GUIStyle _noLogsMessageStyle;
        private GUIStyle _darkBoxStyle;
        private ConsoleReader.FilterType _filterType;
        private GUIContent _clearButtonContent;
        private GUIContent _searchLabelContent;
        private GUIContent[] _filterContents;
        private string _searchString = string.Empty;
        private bool _applyFilter = true;

        public GameObject TargetObject => _targetObject;



        public void ShowWindow(GameObject targetObject) {
            if (targetObject == null)
                return;

            _targetObject = targetObject;
            _window = GetWindow<DebugConsoleWindow>();
            _window.minSize = new Vector2(650f, 200f);
            _window.titleContent = new GUIContent("Debug Console for " + _targetObject.name);

            _rawLogs = ReadLogsForGameObject(_targetObject);
            _window.Repaint();

            GetPrefs();
        }



        private void OnEnable() {
            ConsoleWindowUtility.consoleLogsChanged += OnConsoleLogsChanged;
            PrefsManager.OnEnabledChanged += GetPrefs;
            PrefsManager.OnZebraStrippingChanged += GetPrefs;

            _messageStyle ??= new (EditorStyles.label) {
                richText = true,
                wordWrap = true,
                clipping = TextClipping.Ellipsis,
                alignment = TextAnchor.MiddleLeft,
            };

            _callstackStyle ??= new(EditorStyles.label) {
                wordWrap = true,
                clipping = TextClipping.Ellipsis,
                alignment = TextAnchor.MiddleLeft,
            };

            _noLogsMessageStyle ??= new(EditorStyles.largeLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
            };

            _darkBoxStyle ??= new(GUI.skin.box) {
                padding = GUIStyle.none.padding,
                margin = GUIStyle.none.margin,
                border = GUIStyle.none.border,
            };

            _clearButtonContent = new GUIContent("Clear", IconManager.ClearIcon, "Clear all logs.");
            _searchLabelContent = new GUIContent(IconManager.SearchIcon, "Search logs.");

            _filterContents = new GUIContent[] {
                new ("All", IconManager.AllIcon, "Show all logs."),
                new ("Errors", IconManager.ErrorIconSmall, "Show error logs."),
                new ("Warnings", IconManager.WarnIconSmall, "Show warning logs."),
                new ("Logs", IconManager.InfoIconSmall, "Show log messages."),
            };
        }



        private void OnDisable() {
            ConsoleWindowUtility.consoleLogsChanged -= OnConsoleLogsChanged;
            PrefsManager.OnEnabledChanged -= GetPrefs;
            PrefsManager.OnZebraStrippingChanged -= GetPrefs;
        }



        private void OnGUI() {
            DisplayMenu();
            FilterLogs(_rawLogs);
            DisplayLogs(_filteredLogs, _isFoldout);
        }



        private void OnConsoleLogsChanged() {
            _rawLogs = ReadLogsForGameObject(_targetObject);
            _window.Repaint();
        }



        private void DisplayMenu() {
            EditorGUILayout.BeginHorizontal();
            DisplayFilters();
            GUILayout.Space(10f);
            GUILayout.FlexibleSpace();
            DisplaySearchBox();
            GUILayout.FlexibleSpace();
            GUILayout.Space(10f);
            DisplayClearButton();
            EditorGUILayout.EndHorizontal();
        }



        private void DisplayFilters() {
            ConsoleReader.FilterType newFilterType = (ConsoleReader.FilterType)GUILayout.Toolbar((int)_filterType, _filterContents, GUI.skin.button, GUILayout.Height(30f), GUILayout.Width(360f));

            if (newFilterType != _filterType) {
                _filterType = newFilterType;
                _applyFilter = true;
            }
        }



        private void DisplaySearchBox() {
            EditorGUILayout.BeginVertical();

            GUILayout.Space(8f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(_searchLabelContent, EditorStyles.label, GUILayout.Width(20f), GUILayout.Height(20f));
            string newSearchString = EditorGUILayout.TextField(_searchString, EditorStyles.textField, GUILayout.Height(20f), GUILayout.MinWidth(100f), GUILayout.MaxWidth(300f));
            
            if (newSearchString != _searchString) {
                _searchString = newSearchString;
                _applyFilter = true;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }



        private void DisplayClearButton() {
            if (GUILayout.Button(_clearButtonContent, GUILayout.Height(30f), GUILayout.Width(100f)))
                ClearLogs();
        }



        private ConsoleLog[] ReadLogsForGameObject(GameObject targetObject) {;
            ConsoleLog[] logsFilteredByGameObject = null;

            ConsoleLog[] logs = ConsoleReader.GetDebugMessages();
            if (logs != null && logs.Length > 0)
                logsFilteredByGameObject = ConsoleReader.FilterLogsByGameObject(logs, targetObject);

            _applyFilter = true;
            return logsFilteredByGameObject;
        }



        private void DisplayLogs(ConsoleLog[] logs, bool[] isFoldout) {
            if (logs == null || logs.Length == 0) {
                DisplayNoLogMessage();
                return;
            }

            GUILayout.Space(5f);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.scrollView);
            EditorGUILayout.BeginVertical();

            for (int i = 0; i < logs.Length; i++)
                DisplaySingleLog(logs[i], i, isFoldout);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }



        private void DisplayNoLogMessage() {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("No logs found.", _noLogsMessageStyle, GUILayout.Height(50f), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }



        private void DisplaySingleLog(ConsoleLog log, int index, bool[] isFoldout) {
            if (_zebraStripping && index % 2 == 0)
                EditorGUILayout.BeginVertical(_darkBoxStyle);
            else 
                EditorGUILayout.BeginVertical();

            DisplayMessage(log, index, isFoldout);

            if (isFoldout[index])
                DisplayCallstack(log);

            EditorGUILayout.EndVertical();
        }



        private void DisplayMessage(ConsoleLog log, int index, bool[] isFoldout) {
            EditorGUILayout.BeginHorizontal();

            // Foldout button
            Texture icon = isFoldout[index] ? IconManager.FoldoutIconOn : IconManager.FoldoutIconOff;
            if (GUILayout.Button(icon, EditorStyles.label, GUILayout.Width(15f), GUILayout.Height(45f)))
                isFoldout[index] = !isFoldout[index];

            GUILayout.Space(7);

            // Message as button
            string firstLine = log.Callstack.Split('\n')[0];
            string text = "<size=13>" + log.Message + "</size>\n" + firstLine;
            GUIContent content = new(text, GetIcon(log), "Click to show callstack.");
            if (GUILayout.Button(content, _messageStyle, GUILayout.MinHeight(45f), GUILayout.ExpandWidth(true)))
                isFoldout[index] = !isFoldout[index];

            // Copy button for message
            content = new GUIContent("Copy", "Copy message to clipboard.");
            if (GUILayout.Button(content, GUILayout.Width(45f), GUILayout.Height(35f)))
                EditorGUIUtility.systemCopyBuffer = log.Message;

            GUILayout.Space(7);

            // Open button
            content = new GUIContent("Open", "Open related script in editor.");
            if (GUILayout.Button(content, GUILayout.Width(45f), GUILayout.Height(35f)))
                OpenFile(log.File, log.Line, log.Column);

            EditorGUILayout.EndHorizontal();
        }



        private void DisplayCallstack(ConsoleLog log) {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(30f);

            // Callstack label
            EditorGUILayout.SelectableLabel(log.Callstack, _callstackStyle);

            // Copy button for callstack
            GUIContent content = new ("Copy Callstack", "Copy callstack to clipboard.");
            if (GUILayout.Button(content, GUILayout.Width(100f), GUILayout.Height(25f)))
                EditorGUIUtility.systemCopyBuffer = log.Callstack;

            EditorGUILayout.EndHorizontal();
        }



        private Texture GetIcon(ConsoleLog log) {
            if (log.Mode.IsError())
                return IconManager.ErrorIcon;
            else if (log.Mode.IsWarning())
                return IconManager.WarnIcon;
            else
                return IconManager.InfoIcon;
        }



        private void OpenFile (string file, int line, int column) {
            // Get LogEntries class from UnityEditor assembly using reflection.
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");

            // Get log entries count on the console.
            MethodInfo OpenFileOnSpecificLineAndColumn = logEntries.GetMethod("OpenFileOnSpecificLineAndColumn"); // This method is used to open a file at a specific line and column.
            OpenFileOnSpecificLineAndColumn.Invoke(null, new object[] { file, line, column }); // Invoke the method with the file, line, and column as parameters.
        }



        private void FilterLogs(ConsoleLog[] logs) {
            if (!_applyFilter)
                return;

            if (logs == null || logs.Length == 0) {
                _filteredLogs = null;
                UpdateFoldouts(_filteredLogs);
                return;
            }

            _applyFilter = false;

            ConsoleLog[] logsFilteredByType = ConsoleReader.FilterLogsByType(logs, _filterType);
            _filteredLogs = ConsoleReader.FilterLogsBySearch(logsFilteredByType, _searchString);
            UpdateFoldouts(_filteredLogs);
        }



        private void UpdateFoldouts(ConsoleLog[] logs) {
            if (logs == null) {
                _isFoldout = null;
                return;
            }

            _isFoldout = new bool[logs.Length];
        }



        private void ClearLogs() {
            // Get LogEntries class from UnityEditor assembly using reflection.
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");

            // Get log entries count on the console.
            MethodInfo Clear = logEntries.GetMethod("Clear"); // This method is used to open a file at a specific line and column.
            Clear.Invoke(null, null); // Invoke the method
        }



        private void GetPrefs() {
            if (!PrefsManager.Enabled) {
                Close();
                return;
            }

            _zebraStripping = PrefsManager.ZebraStripping;
            _window.Repaint();
        }



    }

}
