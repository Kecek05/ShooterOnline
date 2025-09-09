using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets.Comparables
{
	public static class ComparableUtility
	{
		public static IComparable GetAssetPathComparable<TObject>(TObject obj) where TObject : Object
		{
			var assetPath = AssetDatabase.GetAssetPath(obj);
			if (!string.IsNullOrEmpty(assetPath))
			{
				return assetPath;
			}
			else
			{
				Debug.LogWarning($"Cannot find asset path for '{obj.name}'.");
			}
			return string.Empty;
		}

		public static IComparable GetGUIDComparable<TObject>(TObject obj) where TObject : Object
		{
			var assetPath = AssetDatabase.GetAssetPath(obj);
			if (!string.IsNullOrEmpty(assetPath))
			{
				var guid = AssetDatabase.AssetPathToGUID(assetPath);
				return guid;
			}
			else
			{
				Debug.LogWarning($"Cannot find asset path for '{obj.name}'.");
			}
			return string.Empty;
		}

		public static IComparable GetPropertyComparable<TObject>(TObject obj, string propertyPath) where TObject : Object
		{
			if (propertyPath == UnityConstants.Field.Name)
			{
				return obj.name;
			}
			var serializedObject = new SerializedObject(obj);
			var property = serializedObject.FindProperty(propertyPath);
			if (property != null)
			{
				var propertyType = property.propertyType;
				switch (propertyType)
				{
					case SerializedPropertyType.Integer:
						if (property.IsTypeInt())
						{
							return property.intValue;
						}
						if (property.IsTypeLong())
						{
							return property.longValue;
						}
#if UNITY_2022_1_OR_NEWER
						else if (property.IsTypeUInt())
						{
							return property.uintValue;
						}
						else if (property.IsTypeULong())
						{
							return property.ulongValue;
						}
#endif
						else
						{
							return property.intValue;
						}

					case SerializedPropertyType.LayerMask:
					case SerializedPropertyType.Enum:
					case SerializedPropertyType.ArraySize:
					case SerializedPropertyType.Character:
						return property.intValue;

					case SerializedPropertyType.Boolean:
						return property.boolValue;

					case SerializedPropertyType.Float:
						if (property.IsTypeFloat())
						{
							return property.floatValue;
						}
						else if (property.IsTypeDouble())
						{
							return property.doubleValue;
						}
						else
						{
							return property.floatValue;
						}

					case SerializedPropertyType.String:
						return property.stringValue;

					case SerializedPropertyType.Color:
						return (ColorComparable) property.colorValue;

					case SerializedPropertyType.ObjectReference:
						if (property.objectReferenceValue != null)
						{
							return property.objectReferenceValue.name;
						}
						break;

					case SerializedPropertyType.AnimationCurve:
						return (AnimationCurveComparable) property.animationCurveValue;

					case SerializedPropertyType.Gradient:
						return (GradientComparable) property.GetGradientValue();

					case SerializedPropertyType.Generic:
						if (property.IsAssetReference())
						{
							var assetSubObjectName = property.FindPropertyRelative(UnityConstants.Field.AssetRefSubObjectName)?.stringValue;
							if (string.IsNullOrEmpty(assetSubObjectName))
							{
								var assetGuid = property.FindPropertyRelative(UnityConstants.Field.AssetRefGuid)?.stringValue;
								var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
								if (string.IsNullOrEmpty(assetGuid) || string.IsNullOrEmpty(assetPath))
								{
									return null;
								}
								else
								{
									var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
									return asset == null ? null : asset.name;
								}
							}
							else
							{
								return assetSubObjectName;
							}
						}
						else
						{
							break;
						}

					default:
						break;
				}
			}
			return null;
		}
	}
}
