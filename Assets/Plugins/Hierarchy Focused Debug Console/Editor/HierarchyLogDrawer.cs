using UnityEngine;
using UnityEditor;



namespace HierarchyFocusedDebugConsole {

    [InitializeOnLoad]
    public static class HierarchyLogDrawer {

        private static ConsoleLog[] _logs;
        private static bool _enabled = true;
        private static bool _isErrorMorePrior = true;
        private static bool _showCount = true;
        private static bool _leftAlignment = false;



        // Constructor will be called every time when Unity Editor is loaded or scripts are compiled because of [InitializeOnLoad] attribute.
        static HierarchyLogDrawer() {
            GetPrefs();
            if (!_enabled)
                return;

            // Every time scripts are compiled, the constructor will be called.
            // So, first remove the previous delegates to avoid multiple calls.
            EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
            ConsoleWindowUtility.consoleLogsChanged -= OnConsoleLogsChanged;
            PrefsManager.OnEnabledChanged -= GetPrefs;
            PrefsManager.OnAlignmentChanged -= GetPrefs;
            PrefsManager.OnPriorityChanged -= GetPrefs;

            // Then, add new delegates to the events.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            ConsoleWindowUtility.consoleLogsChanged += OnConsoleLogsChanged;
            PrefsManager.OnEnabledChanged += GetPrefs;
            PrefsManager.OnAlignmentChanged += GetPrefs;
            PrefsManager.OnPriorityChanged += GetPrefs;
        }



        private static void OnConsoleLogsChanged() {
            if (!_enabled)
                return;

            _logs = ConsoleReader.GetDebugMessages();
            EditorApplication.RepaintHierarchyWindow();
        }



        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
            if (!_enabled || _logs == null || _logs.Length == 0)
                return;

            GameObject hierarchyObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (hierarchyObject == null)
                return;

            ConsoleLog[] filteredLogs = ConsoleReader.FilterLogsByGameObject(_logs, hierarchyObject);
            if (filteredLogs == null || filteredLogs.Length == 0)
                return;
            
            AddButton(hierarchyObject, filteredLogs, selectionRect);
        }



        private static void AddButton(GameObject hierarchyObject, ConsoleLog[] logs, Rect selectionRect) {
            if (logs == null || logs.Length == 0)
                return;

            Texture icon = GetIcon(_logs, _isErrorMorePrior);

            float size = selectionRect.height;
            float x = _leftAlignment ? selectionRect.x : selectionRect.xMax - size;
            Rect iconRect = new(x, selectionRect.y, size, size);

            if (_leftAlignment) {
                Color color = EditorGUIUtility.isProSkin ? GUI.backgroundColor * 0.22f : GUI.backgroundColor * 0.78f;
                color.a = 1f;
                EditorGUI.DrawRect(iconRect, color);
            }

            GUIStyle style = new () {
                fixedWidth = size,
                fixedHeight = size,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(0, 0, 0, 0)
            };


            GUIContent content = new(icon, "Click to show logs for this object.");

            if (GUI.Button(iconRect, content, style))
                DebugConsoleWindowManager.ShowWindow(hierarchyObject);

            if (_showCount && _logs.Length > 1)
                AddCountBadge(logs, selectionRect);
    
        }



        private static Texture GetIcon(ConsoleLog[] logs, bool isErrorPriority) {
            if (logs == null || logs.Length == 0)
                return null;
            
            bool hasError = false;
            bool hasWarning = false;
            bool hasLog = false;

            foreach (ConsoleLog log in logs) {
                if (log.Mode.IsError())
                    hasError = true;
                else if (log.Mode.IsWarning())
                    hasWarning = true;
                else
                    hasLog = true;
            }

            // If error priority is true, then error icon will be shown first
            if (isErrorPriority) {
                if (hasError)
                    return IconManager.ErrorIconSmall;
                else if (hasWarning)
                    return IconManager.WarnIconSmall;
                else
                    return IconManager.InfoIconSmall;

            // Otherwise log icon will be shown first.
            } else {
                if (hasLog)
                    return IconManager.InfoIconSmall;
                else if (hasWarning)
                    return IconManager.WarnIconSmall;
                else
                    return IconManager.ErrorIconSmall;
            }

        }



        private static void AddCountBadge(ConsoleLog[] logs, Rect selectionRect) {
            if (logs == null || logs.Length == 0)
                return;

            GUIContent content = new ();

            int logCount = logs.Length;
            if (logCount == 0) {
                content.text = string.Empty;
                content.tooltip = logs[0].Message;
            } else if (logCount < 1000) {
                content.text = logCount.ToString();
                content.tooltip = $"{logCount} logs for this object.";
            } else {
                content.text = "999+";
                content.tooltip = $"{logCount} logs for this object.";
            }

            float width = selectionRect.height * 3f;
            Rect rect = new(selectionRect.xMax - selectionRect.height - width - 1f, selectionRect.y, width, selectionRect.height);

            GUIStyle badgeStyle = new (GUI.skin.label);
            badgeStyle.alignment = TextAnchor.MiddleRight;

            GUI.Label(rect, content, badgeStyle);

        }



        private static void GetPrefs() {
            _enabled = PrefsManager.Enabled;
            _leftAlignment = PrefsManager.Alignment == Alignment.Left;
            _showCount = PrefsManager.Alignment == Alignment.RightWithCount;
            _isErrorMorePrior = PrefsManager.Priority == Priority.Error;
            EditorApplication.RepaintHierarchyWindow();
        }



        public enum Alignment {
            RightWithCount,
            Right,
            Left
        }



        public enum Priority {
            Error,
            Info
        }



    }

}
