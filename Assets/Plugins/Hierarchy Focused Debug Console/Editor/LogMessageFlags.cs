// Source: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/LogEntries.bindings.cs

using System;



namespace HierarchyFocusedDebugConsole {

    [Flags]
    public enum LogMessageFlags : int {

        kNoLogMessageFlags = 0,
        kError = 1 << 0, // Message describes an error.
        kAssert = 1 << 1, // Message describes an assertion failure.
        kLog = 1 << 2, // Message is a general log message.
        kFatal = 1 << 4, // Message describes a fatal error, and that the program should now exit.
        kAssetImportError = 1 << 6, // Message describes an error generated during asset importing.
        kAssetImportWarning = 1 << 7, // Message describes a warning generated during asset importing.
        kScriptingError = 1 << 8, // Message describes an error produced by script code.
        kScriptingWarning = 1 << 9, // Message describes a warning produced by script code.
        kScriptingLog = 1 << 10, // Message describes a general log message produced by script code.
        kScriptCompileError = 1 << 11, // Message describes an error produced by the script compiler.
        kScriptCompileWarning = 1 << 12, // Message describes a warning produced by the script compiler.
        kStickyLog = 1 << 13, // Message is 'sticky' and should not be removed when the user manually clears the console window.
        kMayIgnoreLineNumber = 1 << 14, // The scripting runtime should skip annotating the log callstack with file and line information.
        kReportBug = 1 << 15, // When used with kFatal, indicates that the log system should launch the bug reporter.
        kDisplayPreviousErrorInStatusBar = 1 << 16, // The message before this one should be displayed at the bottom of Unity's main window, unless there are no messages before this one.
        kScriptingException = 1 << 17, // Message describes an exception produced by script code.
        kDontExtractStacktrace = 1 << 18, // Stacktrace extraction should be skipped for this message.
        kScriptingAssertion = 1 << 21, // The message describes an assertion failure in script code.
        kStacktraceIsPostprocessed = 1 << 22, // The stacktrace has already been postprocessed and does not need further processing.
        kIsCalledFromManaged = 1 << 23, // The message is being called from managed code.

        FromEditor = kDontExtractStacktrace | kMayIgnoreLineNumber | kIsCalledFromManaged,

        DebugLog = kScriptingLog | FromEditor,
        DebugWarning = kScriptingWarning | FromEditor,
        DebugError = kScriptingError | FromEditor,
        DebugException = kScriptingException | FromEditor,
        DebugAssert = kScriptingAssertion | FromEditor
        
    }



    public static class LogMessageFlagsExtensions {



        public static bool IsInfo(this LogMessageFlags flags) {
            return (flags & (LogMessageFlags.kLog | LogMessageFlags.kScriptingLog)) != 0;
        }



        public static bool IsWarning(this LogMessageFlags flags) {
            return (flags & (LogMessageFlags.kScriptCompileWarning | LogMessageFlags.kScriptingWarning | LogMessageFlags.kAssetImportWarning)) != 0;
        }



        public static bool IsError(this LogMessageFlags flags) {
            return (flags & (LogMessageFlags.kFatal | LogMessageFlags.kAssert | LogMessageFlags.kError | LogMessageFlags.kScriptCompileError |
                            LogMessageFlags.kScriptingError | LogMessageFlags.kAssetImportError | LogMessageFlags.kScriptingAssertion | LogMessageFlags.kScriptingException)) != 0;
        }



    }

}
