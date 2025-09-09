using UnityEngine;
using UnityEditor;



namespace HierarchyFocusedDebugConsole {

    public static class IconManager {

        private static Texture _infoIconSmall;
        private static Texture _warnIconSmall;
        private static Texture _errorIconSmall;
        private static Texture _infoIcon;
        private static Texture _warnIcon;
        private static Texture _errorIcon;
        private static Texture _foldoutIconOff;
        private static Texture _foldoutIconOn;
        private static Texture _allIcon;
        private static Texture _clearIcon;
        private static Texture _searchIcon;



        public static Texture InfoIconSmall {
            get {
                if (_infoIconSmall == null) {
                    _infoIconSmall = EditorGUIUtility.Load("d_console.infoicon.sml@2x") as Texture;
                }
                return _infoIconSmall;
            }
        }



        public static Texture WarnIconSmall {
            get {
                if (_warnIconSmall == null) {
                    _warnIconSmall = EditorGUIUtility.Load("d_console.warnicon.sml@2x") as Texture;
                }
                return _warnIconSmall;
            }
        }



        public static Texture ErrorIconSmall {
            get {
                if (_errorIconSmall == null) {
                    _errorIconSmall = EditorGUIUtility.Load("d_console.erroricon.sml@2x") as Texture;
                }
                return _errorIconSmall;
            }
        }



        public static Texture InfoIcon {
            get {
                if (_infoIcon == null) {
                    _infoIcon = EditorGUIUtility.Load("d_console.infoicon@2x") as Texture;
                }
                return _infoIcon;
            }
        }



        public static Texture WarnIcon {
            get {
                if (_warnIcon == null) {
                    _warnIcon = EditorGUIUtility.Load("d_console.warnicon@2x") as Texture;
                }
                return _warnIcon;
            }
        }



        public static Texture ErrorIcon {
            get {
                if (_errorIcon == null) {
                    _errorIcon = EditorGUIUtility.Load("d_console.erroricon@2x") as Texture;
                }
                return _errorIcon;
            }
        }



        public static Texture FoldoutIconOff {
            get {
                if (_foldoutIconOff == null) {
                    _foldoutIconOff = (Texture)EditorGUIUtility.Load("Assets/Hierarchy Focused Debug Console/Icons/d_IN_foldout_act@2x.png");
                }
                return _foldoutIconOff;
            }
        }



        public static Texture FoldoutIconOn {
            get {
                if (_foldoutIconOn == null) {
                    _foldoutIconOn = (Texture)EditorGUIUtility.Load("Assets/Hierarchy Focused Debug Console/Icons/d_IN_foldout_act_on@2x.png");
                }
                return _foldoutIconOn;
            }
        }



        public static Texture AllIcon {
            get {
                if (_allIcon == null) {
                    _allIcon = EditorGUIUtility.Load("d_align_vertically_center_active") as Texture;
                }
                return _allIcon;
            }
        }



        public static Texture ClearIcon {
            get {
                if (_clearIcon == null) {
                    _clearIcon = EditorGUIUtility.Load("d_P4_DeletedLocal@2x") as Texture;
                }
                return _clearIcon;
            }
        }



        public static Texture SearchIcon {
            get {
                if (_searchIcon == null) {
                    _searchIcon = EditorGUIUtility.Load("d_SearchOverlay@2x") as Texture;
                }
                return _searchIcon;
            }
        }


        
    }

}
