using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;



namespace HierarchyFocusedDebugConsole {

    public static class ConsoleReader {



        public static ConsoleLog[] GetDebugMessages() {
            // Source: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/LogEntries.bindings.cs

            // Get LogEntries class from UnityEditor assembly using reflection.
            Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
            Type logEntries = assembly.GetType("UnityEditor.LogEntries");

            // Get log entries count on the console.
            MethodInfo GetCount = logEntries.GetMethod("GetCount"); // This method is used to get the number of log entries in the console.
            int count = (int) GetCount.Invoke(null, null);

            // Get required methods and fields from LogEntries class.
            MethodInfo GetEntryInternal = logEntries.GetMethod("GetEntryInternal"); // This method is used to get the log entry at a specific index.
            Type logEntryType = assembly.GetType("UnityEditor.LogEntry"); // This is the type (class) of the log entry object.
            FieldInfo messageField = logEntryType.GetField("message"); // This field contains the message of the log entry.
            FieldInfo instanceIDField = logEntryType.GetField("instanceID"); // This field contains the instance ID of the GameObject associated with the log entry.
            FieldInfo callstackIndexField = logEntryType.GetField("callstackTextStartUTF8"); // This field contains the start index of the callstack text in the log entry.
            FieldInfo flagsField = logEntryType.GetField("mode"); // This field contains the flags of the log entry.
            FieldInfo fileField = logEntryType.GetField("file"); // This field contains the file name of the log entry.
            FieldInfo lineField = logEntryType.GetField("line"); // This field contains the line number of the log entry.
            FieldInfo columnField = logEntryType.GetField("column"); // This field contains the column number of the log entry.

            object[] parameters = new object[2];
            ConsoleLog[] logs = new ConsoleLog[count];

            // Loop through all log entries and get the required information.
            for (int i = 0; i < count; i++) {
                parameters[0] = i; // index
                parameters[1] = Activator.CreateInstance(logEntryType); // log entry object
                bool result = (bool) GetEntryInternal.Invoke(null, parameters);

                if (result) {
                    string fullMessage = (string) messageField.GetValue(parameters[1]);
                    int instanceID = (int) instanceIDField.GetValue(parameters[1]);
                    LogMessageFlags mode = (LogMessageFlags) flagsField.GetValue(parameters[1]);
                    int callstackTextStartIndex = (int)callstackIndexField.GetValue(parameters[1]);
                    string callstack = fullMessage[callstackTextStartIndex..].Trim(); // Get the callstack text from the message.
                    string message = fullMessage[..callstackTextStartIndex]; // Get the message before the callstack text.
                    string file = (string) fileField.GetValue(parameters[1]); // Get the file name from the log entry.
                    int line = (int) lineField.GetValue(parameters[1]); // Get the line number from the log entry.
                    int column = (int) columnField.GetValue(parameters[1]); // Get the column number from the log entry.

                    logs[i] = new ConsoleLog(i, instanceID, mode, message, callstack, file, line, column);
                }
            }

            return logs;
        }



        public static ConsoleLog[] FilterLogsByGameObject(ConsoleLog[] logs, GameObject targetObject) {
            List<ConsoleLog> filteredLogs = new();

            foreach (ConsoleLog log in logs) {
                UnityEngine.Object logObject = EditorUtility.InstanceIDToObject(log.InstanceID);
                if (logObject == null)
                    continue; // If the log object is null, skip it.

                // If the log object is a GameObject and matches the target object, add it to the filtered logs.
                if (logObject is GameObject && logObject.Equals(targetObject))
                    filteredLogs.Add(log);

                // If the log object is a Component and its GameObject matches the target object, add it to the filtered logs.
                if (logObject is Component && ((Component)logObject).gameObject.Equals(targetObject))
                    filteredLogs.Add(log);
            }

            return filteredLogs.ToArray();
        }



        public static ConsoleLog[] FilterLogsByType(ConsoleLog[] logs, FilterType filterType) {
            if (filterType == FilterType.All || logs == null || logs.Length == 0)
                return logs;

            List<ConsoleLog> filteredLogs = new();

            foreach (ConsoleLog log in logs) {
                if (log.Mode.IsError() && filterType == FilterType.Error || log.Mode.IsWarning() && filterType == FilterType.Warning || log.Mode.IsInfo() && filterType == FilterType.Info) {
                    filteredLogs.Add(log);
                }
            }

            return filteredLogs.ToArray();
        }



        public static ConsoleLog[] FilterLogsBySearch(ConsoleLog[] logs, string searchString) {
            if (string.IsNullOrEmpty(searchString) || logs == null || logs.Length == 0)
                return logs;

            searchString = searchString.Trim(); // Trim the search string to remove leading and trailing whitespace.

            if (string.IsNullOrEmpty(searchString))
                return logs; // If the trimmed search string is empty, return all logs.

            List<ConsoleLog> filteredLogs = new();

            foreach (ConsoleLog log in logs) {
                if (log.Message.Contains(searchString, StringComparison.OrdinalIgnoreCase) || log.Callstack.Contains(searchString, StringComparison.OrdinalIgnoreCase) || log.File.Contains(searchString, StringComparison.OrdinalIgnoreCase)) {
                    filteredLogs.Add(log);
                }
            }

            return filteredLogs.ToArray();
        }



        public enum FilterType {
            All,
            Error,
            Warning,
            Info,
        }



    }

}
