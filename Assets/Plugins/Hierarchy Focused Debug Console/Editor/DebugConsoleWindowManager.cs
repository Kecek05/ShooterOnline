using UnityEngine;



namespace HierarchyFocusedDebugConsole {

    public static class DebugConsoleWindowManager {



        public static void ShowWindow(GameObject targetObject) {
            if (targetObject == null)
                return;

            DebugConsoleWindow window = GetOpenInstances(targetObject);

            if (window == null) {
                window = ScriptableObject.CreateInstance<DebugConsoleWindow>();
                window.ShowWindow(targetObject);
            } else {
                window.Focus();
                window.Repaint();
            }
            
        }



        private static DebugConsoleWindow GetOpenInstances(GameObject targetObject) {
            Object[] array = Resources.FindObjectsOfTypeAll(typeof(DebugConsoleWindow));

            foreach (Object obj in array) {
                if (obj is DebugConsoleWindow window && window.TargetObject == targetObject) {
                    return window;
                }
            }

            return null;
        }



    }

}
