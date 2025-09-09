using UnityEngine.Events;
using UnityEditor;



namespace HierarchyFocusedDebugConsole {

    public static class PrefsManager {

        private const string BaseKey = "HierarchyFocusedDebugConsole/";
        private const string EnabledKey = BaseKey + "Enabled";
        private const string PriorityKey = BaseKey + "Priority";
        private const string AlignmetKey = BaseKey + "Alignment";
        private const string ZebraStrippingKey = BaseKey + "ZebraStripping";



        public static event UnityAction OnEnabledChanged;
        public static event UnityAction OnPriorityChanged;
        public static event UnityAction OnAlignmentChanged;
        public static event UnityAction OnZebraStrippingChanged;



        public static bool Enabled {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set {
                if (Enabled == value)
                    return;
                EditorPrefs.SetBool(EnabledKey, value);
                OnEnabledChanged?.Invoke();
            }
        }



        public static HierarchyLogDrawer.Priority Priority {
            get => (HierarchyLogDrawer.Priority)EditorPrefs.GetInt(PriorityKey, (int)HierarchyLogDrawer.Priority.Error);
            set {
                if (Priority == value)
                    return;
                EditorPrefs.SetInt(PriorityKey, (int)value);
                OnPriorityChanged?.Invoke();
            }
        }



        public static HierarchyLogDrawer.Alignment Alignment {
            get => (HierarchyLogDrawer.Alignment)EditorPrefs.GetInt(AlignmetKey, (int)HierarchyLogDrawer.Alignment.RightWithCount);
            set {
                if (Alignment == value)
                    return;
                EditorPrefs.SetInt(AlignmetKey, (int)value);
                OnAlignmentChanged?.Invoke();
            }
        }



        public static bool ZebraStripping {
            get => EditorPrefs.GetBool(ZebraStrippingKey, true);
            set {
                if (ZebraStripping == value)
                    return;
                EditorPrefs.SetBool(ZebraStrippingKey, value);
                OnZebraStrippingChanged?.Invoke();
            }
        }


        
    }

}
