using LunaWolfStudiosEditor.ScriptableSheets.Layout;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LunaWolfStudiosEditor.ScriptableSheets
{
	[System.Serializable]
	public class ExperimentalSettings : AbstractBaseSettings
	{
		[SerializeField]
		private List<SerializedPropertyType> m_RenderingOverrides = new List<SerializedPropertyType>();
		private HashSet<SerializedPropertyType> m_RenderingOverridesLookup;

		public override GUIContent FoldoutContent => SettingsContent.Foldouts.Experimental;

		public ExperimentalSettings()
		{
			m_RenderingOverrides = new List<SerializedPropertyType>();
			m_RenderingOverridesLookup = null;
		}

		protected override void DrawProperties(SerializedObject target)
		{
			var customDrawerAllowedTypesProperty = target.FindProperty($"m_Experimental.{nameof(m_RenderingOverrides)}");
			EditorGUILayout.PropertyField(customDrawerAllowedTypesProperty, SettingsContent.Label.RenderingOverrides, true);
			m_RenderingOverridesLookup = new HashSet<SerializedPropertyType>(m_RenderingOverrides);
		}

		public HashSet<SerializedPropertyType> GetRenderingOverrides()
		{
			if (m_RenderingOverridesLookup == null)
			{
				m_RenderingOverridesLookup = new HashSet<SerializedPropertyType>(m_RenderingOverrides);
			}
			return m_RenderingOverridesLookup;
		}
	}
}
