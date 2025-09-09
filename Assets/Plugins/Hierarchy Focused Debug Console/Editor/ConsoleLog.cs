namespace HierarchyFocusedDebugConsole {

    public struct ConsoleLog {

        public int Index;
        public int InstanceID;
        public LogMessageFlags Mode;
        public string Message;
        public string Callstack;
        public string File;
        public int Line;
        public int Column;



        public ConsoleLog(int index, int instanceID, LogMessageFlags mode, string message, string callstack, string file, int line, int column) {
            Index = index;
            InstanceID = instanceID;
            Mode = mode;
            Message = message;
            Callstack = callstack;
            File = file;
            Line = line;
            Column = column;
        }

        

    }

}
