using UnityEditor;




namespace HierarchyFocusedDebugConsole {

    public static class OptionsMenu {

        private const string BaseMenu = "Tools/Hierarchy Focused Debug Console/";



        [MenuItem(BaseMenu + "Enabled", false, 1)]
        public static void EnabledItem() {
            PrefsManager.Enabled = !PrefsManager.Enabled;
        }



        [MenuItem(BaseMenu + "Enabled", true)]
        public static bool EnabledItemValidate() {
            Menu.SetChecked(BaseMenu + "Enabled", PrefsManager.Enabled);
            return true;
        }



        [MenuItem(BaseMenu + "Zebra Stripping", false, 12)]
        public static void ZebraStrippingItem() {
            PrefsManager.ZebraStripping = !PrefsManager.ZebraStripping;
        }



        [MenuItem(BaseMenu + "Zebra Stripping", true)]
        public static bool ZebraStrippingItemValidate() {
            Menu.SetChecked(BaseMenu + "Zebra Stripping", PrefsManager.ZebraStripping);
            return true;
        }



        [MenuItem(BaseMenu + "Priority/Error", false, 13)]
        public static void PriorityErrorItem() {
            PrefsManager.Priority = HierarchyLogDrawer.Priority.Error;;
        }



        [MenuItem(BaseMenu + "Priority/Info", false, 14)]
        public static void PriorityInfoItem() {
            PrefsManager.Priority = HierarchyLogDrawer.Priority.Info;
        }



        [MenuItem(BaseMenu + "Priority/Error", true)]
        [MenuItem(BaseMenu + "Priority/Info", true)]
        private static bool PriorityItemValidate() {
            Menu.SetChecked(BaseMenu + "Priority/Error", PrefsManager.Priority == HierarchyLogDrawer.Priority.Error);
            Menu.SetChecked(BaseMenu + "Priority/Info", PrefsManager.Priority == HierarchyLogDrawer.Priority.Info);
            return true;
        }



        [MenuItem(BaseMenu + "Alignment/Right with Count", false, 13)]
        public static void AlignmentRightWithCountItem() {
            PrefsManager.Alignment = HierarchyLogDrawer.Alignment.RightWithCount;
        }



        [MenuItem(BaseMenu + "Alignment/Right", false, 14)]
        public static void AlignmentRightItem() {
            PrefsManager.Alignment = HierarchyLogDrawer.Alignment.Right;
        }



        [MenuItem(BaseMenu + "Alignment/Left", false, 15)]
        public static void AlignmentLeftItem() {
            PrefsManager.Alignment = HierarchyLogDrawer.Alignment.Left;
        }



        [MenuItem(BaseMenu + "Alignment/Right with Count", true)]
        [MenuItem(BaseMenu + "Alignment/Right", true)]
        [MenuItem(BaseMenu + "Alignment/Left", true)]
        private static bool AlignmentItemValidate() {
            Menu.SetChecked(BaseMenu + "Alignment/Right with Count", PrefsManager.Alignment == HierarchyLogDrawer.Alignment.RightWithCount);
            Menu.SetChecked(BaseMenu + "Alignment/Right", PrefsManager.Alignment == HierarchyLogDrawer.Alignment.Right);
            Menu.SetChecked(BaseMenu + "Alignment/Left", PrefsManager.Alignment == HierarchyLogDrawer.Alignment.Left);
            return true;
        }



    }

}
