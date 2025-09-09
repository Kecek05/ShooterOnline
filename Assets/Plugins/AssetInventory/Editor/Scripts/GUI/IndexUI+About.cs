using UnityEditor;
#if UNITY_2021_3_OR_NEWER && !USE_TUTORIALS
using UnityEditor.PackageManager;
#endif
using UnityEngine;

namespace AssetInventory
{
    public partial class IndexUI
    {
        private void DrawAboutTab()
        {
            GUIStyle textColor = EditorGUIUtility.isProSkin ? UIStyles.whiteCenter : UIStyles.blackCenter;

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("A tool by Impossible Robert", UIStyles.centerHeading, GUILayout.Width(300), GUILayout.Height(50));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Developer: Robert Wetzold", textColor);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Online Resources", UIStyles.centerLinkLabel)) Application.OpenURL(AI.HOME_LINK);
            EditorGUILayout.LabelField(" | ", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(6));
            if (GUILayout.Button("Join Discord!", UIStyles.centerLinkLabel)) Application.OpenURL(AI.DISCORD_LINK);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
#if UNITY_2021_3_OR_NEWER && !USE_TUTORIALS
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(UIStyles.Content("Install/Upgrade Tutorials Package...", "Integrated tutorials require the Unity Tutorials package installed."), GUILayout.ExpandWidth(false)))
            {
                Client.Add($"com.unity.learn.iet-framework@{AI.TUTORIALS_VERSION}");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
#endif
            EditorGUILayout.LabelField($"Version {AI.VERSION}", textColor);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("If you like this asset please consider leaving a review on the Unity Asset Store. Thanks a million!", MessageType.Info);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Write Review")) Application.OpenURL(AI.ASSET_STORE_LINK);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(Logo, EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(200), GUILayout.MaxHeight(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (AI.DEBUG_MODE && GUILayout.Button("Reload Lookups")) ReloadLookups();
            if (ShowAdvanced())
            {
                if (GUILayout.Button("Restart Setup Wizard"))
                {
                    AI.Config.wizardCompleted = false;
                    AI.Config.wizardCurrentPage = 0;
                    AI.SaveConfig();
                }
                if (GUILayout.Button("Create Debug Support Report")) CreateDebugReport();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            if (AI.DEBUG_MODE && GUILayout.Button("Get Token", GUILayout.ExpandWidth(false))) Debug.Log(CloudProjectSettings.accessToken);
            if (AI.DEBUG_MODE && GUILayout.Button("Reload Lookups")) ReloadLookups();
            if (AI.DEBUG_MODE && GUILayout.Button("Free Memory")) Resources.UnloadUnusedAssets();
        }
    }
}