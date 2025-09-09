using LunaWolfStudiosEditor.ScriptableSheets.Serializables;
using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets.Tables
{
	public struct SerializedTableProperty : ITableProperty
	{
		public const string NullObjectValue = "null";
		private const char AssetDelimiter = '&';
		// Sub Asset names can contain special chars, so use a unique string.
		private const string SubAssetDelimiter = "&SUBASSET#";

		private readonly Object m_RootObject;
		public Object RootObject => m_RootObject;

		private readonly string m_PropertyPath;
		public string PropertyPath => m_PropertyPath;

		private readonly string m_ControlName;
		public string ControlName => m_ControlName;

		public SerializedTableProperty(Object rootObject, string propertyPath, string controlName)
		{
			m_RootObject = rootObject;
			m_PropertyPath = propertyPath;
			m_ControlName = controlName;
		}

		public SerializedObject GetSerializedObject()
		{
			return new SerializedObject(m_RootObject);
		}

		public SerializedProperty GetSerializedProperty()
		{
			return GetSerializedProperty(GetSerializedObject());
		}

		public SerializedProperty GetSerializedProperty(SerializedObject serializedObject)
		{
			return serializedObject.FindProperty(PropertyPath);
		}

		public string GetProperty(FlatFileFormatSettings formatSettings)
		{
			var property = GetSerializedProperty();
			if (property.propertyPath == UnityConstants.Field.Name)
			{
				return m_RootObject.name;
			}
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer:
					return property.GetIntStringValue();

				case SerializedPropertyType.Boolean:
					return property.boolValue.ToString();

				case SerializedPropertyType.Float:
					return property.GetFloatStringValue();

				case SerializedPropertyType.String:
					return property.stringValue;

				case SerializedPropertyType.Color:
					return ColorUtility.ToHtmlStringRGBA(property.colorValue);

				case SerializedPropertyType.LayerMask:
					return property.intValue.ToString();

				case SerializedPropertyType.Enum:
					if (!IsDefaultUnityEngineType(property.serializedObject) && formatSettings.UseStringEnums && property.TryGetEnumType(m_RootObject, out Type enumType))
					{
						if (!enumType.HasFlagsAttribute())
						{
							var enumName = Enum.GetName(enumType, property.intValue);
							if (!string.IsNullOrEmpty(enumName))
							{
								return enumName;
							}
						}
					}
					return property.intValue.ToString();

				case SerializedPropertyType.ObjectReference:
					var objValue = property.objectReferenceValue;
					if (objValue != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(property.objectReferenceValue, out string guid, out long localId))
					{
						return $"{guid}{AssetDelimiter}{objValue.name}";
					}
					else
					{
						return NullObjectValue;
					}

				case SerializedPropertyType.ArraySize:
					return property.intValue.ToString();

				case SerializedPropertyType.Character:
					return ((char) property.intValue).ToString();

				case SerializedPropertyType.AnimationCurve:
					var animationCurveJson = JsonUtility.ToJson(new SerializableAnimationCurve(property));
					if (formatSettings.WrapOption.IsJsonUnsafe())
					{
						return animationCurveJson.EncodeBase64();
					}
					else
					{
						return animationCurveJson;
					}

				case SerializedPropertyType.Gradient:
					var gradientJson = JsonUtility.ToJson(new SerializableGradient(property));
					if (formatSettings.WrapOption.IsJsonUnsafe())
					{
						return gradientJson.EncodeBase64();
					}
					else
					{
						return gradientJson;
					}

				case SerializedPropertyType.Generic:
					if (property.IsAssetReference())
					{
						var assetGuid = property.FindPropertyRelative(UnityConstants.Field.AssetRefGuid)?.stringValue;
						var subObjectName = property.FindPropertyRelative(UnityConstants.Field.AssetRefSubObjectName)?.stringValue;
						if (string.IsNullOrEmpty(assetGuid))
						{
							return NullObjectValue;
						}
						return $"{assetGuid}{SubAssetDelimiter}{subObjectName}";
					}
					else
					{
						Debug.LogWarning($"Unsupported generic property type for property at path {PropertyPath}.");
						return string.Empty;
					}

				default:
					Debug.LogWarning($"Unsupported property type {property.propertyType} for property at path {PropertyPath}.");
					return string.Empty;
			}
		}

		public void SetProperty(string value, FlatFileFormatSettings formatSettings)
		{
			var serializedObject = GetSerializedObject();
			var property = GetSerializedProperty(serializedObject);
			if (property != null)
			{
				if ((!property.editable && property.name != UnityConstants.Field.Name) || value == null)
				{
					// Cannot change read-only properties. To set properties null use an empty string or the null object string.
					return;
				}
				var propertyType = property.propertyType;
				switch (property.propertyType)
				{
					case SerializedPropertyType.Integer:
					case SerializedPropertyType.ArraySize:
						if (!property.TrySetIntValue(value))
						{
							LogParseWarning(value, propertyType, $"Not a valid int for numeric type '{property.type}'.");
						}
						break;

					case SerializedPropertyType.LayerMask:
						value = value.UnwrapLayerMask();
						if (int.TryParse(value, out int intValue))
						{
							property.intValue = intValue;
						}
						else
						{
							LogParseWarning(value, propertyType, "Not a valid int.");
						}
						break;

					case SerializedPropertyType.Enum:
						if (!IsDefaultUnityEngineType(serializedObject) && formatSettings.UseStringEnums)
						{
							if (property.TryGetEnumType(m_RootObject, out Type enumType))
							{
								if (!enumType.HasFlagsAttribute() || value.Contains(UnityConstants.EnumPrefix))
								{
									try
									{
										// Unity prefixes single enum values with 'Enum:' when copying from the Inspector.
										value = value.Replace(UnityConstants.EnumPrefix, string.Empty);
										// Use Enum.Parse instead of TryParse to support older versions of C#.
										var enumObject = Enum.Parse(enumType, value, formatSettings.IgnoreCase);
										property.intValue = (int) enumObject;
									}
									catch (Exception ex)
									{
										LogParseWarning(value, propertyType, ex.Message);
									}
									break;
								}
								else
								{
									value = value.UnwrapLayerMask();
								}
							}
							else
							{
								LogParseWarning(value, propertyType, "Did not find a valid enum type.");
							}
						}
						if (int.TryParse(value, out int enumValue))
						{
							property.intValue = enumValue;
						}
						else
						{
							LogParseWarning(value, propertyType, "Not a valid int.");
						}
						break;

					case SerializedPropertyType.Boolean:
						if (bool.TryParse(value, out bool boolValue))
						{
							property.boolValue = boolValue;
						}
						else if (int.TryParse(value, out int boolIntValue))
						{
							property.boolValue = boolIntValue > 0;
						}
						else
						{
							LogParseWarning(value, propertyType, "Not a valid bool.");
						}
						break;

					case SerializedPropertyType.Float:
						if (!property.TrySetFloatValue(value))
						{
							LogParseWarning(value, propertyType, "Not a valid float.");
						}
						break;

					case SerializedPropertyType.String:
						if (property.name == UnityConstants.Field.Name)
						{
							if (!value.Any(c => Path.GetInvalidFileNameChars().Contains(c)))
							{
								if (value != m_RootObject.name)
								{
									var assetPath = AssetDatabase.GetAssetPath(m_RootObject);
									if (assetPath.Contains($"/{m_RootObject.name}."))
									{
										var assetRenameResponse = AssetDatabase.RenameAsset(assetPath, value);
										if (!string.IsNullOrEmpty(assetRenameResponse))
										{
											LogParseWarning(value, propertyType, assetRenameResponse);
										}
									}
									else
									{
										m_RootObject.name = value;
									}
								}
							}
							else
							{
								Debug.LogWarning($"Invalid filename '{value}'.");
							}
						}
						else
						{
							property.stringValue = value;
						}
						break;

					case SerializedPropertyType.Color:
						if (value.Length > 0 && value[0] != '#')
						{
							value = "#" + value;
						}
						if (ColorUtility.TryParseHtmlString(value, out Color parsedColor))
						{
							property.colorValue = parsedColor;
						}
						else
						{
							LogParseWarning(value, propertyType, "Not a valid color hex code.");
						}
						break;

					case SerializedPropertyType.ObjectReference:
						if (value.Length > 0 && value != NullObjectValue)
						{
							// Handle case where user might copy from the inspector.
							UnityObjectWrapper objectWrapper = null;
							if (value.Contains(UnityConstants.ObjectWrapperJSON))
							{
								value = value.Replace(UnityConstants.ObjectWrapperJSON, string.Empty);
								try
								{
									objectWrapper = JsonConvert.DeserializeObject<UnityObjectWrapper>(value);
								}
								catch (Exception ex)
								{
									LogParseWarning(value, propertyType, ex.Message);
									break;
								}
								value = objectWrapper.Guid;
							}

							var parts = value.Split(AssetDelimiter);
							var assetPath = AssetDatabase.GUIDToAssetPath(parts[0]);
							if (!string.IsNullOrEmpty(assetPath))
							{
								// GUIDs for Unity built in extra assets are not unique and must be found with a manual search by filename or localId.
								if (assetPath == UnityConstants.Path.BuiltInExtra)
								{
									Object foundAsset = null;
									var friendlyType = property.FriendlyType();
									if (objectWrapper != null)
									{
										foreach (var asset in SerializedPropertyUtility.UnityBuiltInAssets)
										{
											if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId) && objectWrapper.LocalId == localId)
											{
												foundAsset = asset;
												break;
											}
										}
										if (foundAsset != null)
										{
											property.objectReferenceValue = foundAsset;
										}
										else
										{
											LogParseWarning(value, propertyType, $"Unable to find asset at path '{assetPath}' with {objectWrapper}.");
										}
									}
									else if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
									{
										foreach (var asset in SerializedPropertyUtility.UnityBuiltInAssets)
										{
											if (asset.name == parts[1] && friendlyType == asset.GetType().Name)
											{
												foundAsset = asset;
												break;
											}
										}
										if (foundAsset != null)
										{
											property.objectReferenceValue = foundAsset;
										}
										else
										{
											LogParseWarning(value, propertyType, $"Unable to find asset '{parts[1].GetEscapedText()}' at path '{assetPath}'.");
										}
									}
									else
									{
										LogParseWarning(value, propertyType, $"Unable to find asset with GUID '{parts[0]}' at path '{assetPath}'. Please include the asset name.");
									}
								}
								else
								{
									if (!IsDefaultUnityEngineType(serializedObject))
									{
										// Certain Object types like Sprite as well as Sub Assets require the Type to be explicit when loading from an asset path.
										var objectType = ReflectionUtility.GetNestedFieldType(m_RootObject.GetType(), property.propertyPath);
										if (objectType != null)
										{
											property.objectReferenceValue = AssetDatabase.LoadAssetAtPath(assetPath, objectType);
										}
										else
										{
											property.objectReferenceValue = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
										}
									}
									else
									{
										property.objectReferenceValue = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
									}

									// If we still haven't found the exact Object then see if it's a Sub Asset. This can occur with TMPro shared material assets and child GameObjects.
									if (parts.Length > 1 && (property.objectReferenceValue == null || property.objectReferenceValue.name != parts[1]))
									{
										foreach (var subAsset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
										{
											property.objectReferenceValue = subAsset;
											// Validate the asset names match up if possible.
											if (property.objectReferenceValue != null && property.objectReferenceValue.name == parts[1])
											{
												break;
											}
										}
									}
								}
							}
							else
							{
								LogParseWarning(value, propertyType, $"Unable to find asset path for GUID '{parts[0].GetEscapedText()}'.");
							}
						}
						else
						{
							property.objectReferenceValue = null;
						}
						break;

					case SerializedPropertyType.Character:
						if (value.Length > 0)
						{
							property.intValue = value[0];
						}
						else
						{
							LogParseWarning(value, propertyType, "Char cannot be empty.");
						}
						break;

					case SerializedPropertyType.AnimationCurve:
						if (formatSettings.WrapOption.IsJsonUnsafe())
						{
							value = value.DecodeBase64();
						}
						// Handle case where user might copy from the inspector.
						value = value.Replace("UnityEditor.AnimationCurveWrapperJSON:{\"c", "{\"m_AnimationC");
						try
						{
							var animationCurve = JsonUtility.FromJson<SerializableAnimationCurve>(value).AnimationCurve;
							property.animationCurveValue = animationCurve;
						}
						catch (Exception ex)
						{
							LogParseWarning(value, propertyType, ex.Message);
						}
						break;

					case SerializedPropertyType.Gradient:
						if (formatSettings.WrapOption.IsJsonUnsafe())
						{
							value = value.DecodeBase64();
						}
						// Handle case where user might copy from the inspector.
						value = value.Replace("UnityEditor.GradientWrapperJSON:{\"g", "{\"m_G");
						try
						{
							var gradient = JsonUtility.FromJson<SerializableGradient>(value).Gradient;
							property.SetGradientValue(gradient);
						}
						catch (Exception ex)
						{
							LogParseWarning(value, propertyType, ex.Message);
						}
						break;

					case SerializedPropertyType.Generic:
						if (property.IsAssetReference())
						{
							var guidProperty = property.FindPropertyRelative(UnityConstants.Field.AssetRefGuid);
							var subObjectNameProperty = property.FindPropertyRelative(UnityConstants.Field.AssetRefSubObjectName);
							var subObjectTypeProperty = property.FindPropertyRelative(UnityConstants.Field.AssetRefSubObjectType);
							if (guidProperty != null && subObjectNameProperty != null && subObjectTypeProperty != null)
							{
								// Remove line endings to avoid creating sub asset references when pasting content.
								value = value.Replace("\r", string.Empty).Replace("\n", string.Empty);
								if (value.Length > 0 && value != NullObjectValue)
								{
									var parts = value.Split(new string[] { SubAssetDelimiter }, StringSplitOptions.None);
									var assetGuid = parts[0];
									var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
									if (!string.IsNullOrEmpty(assetPath))
									{
										if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
										{
											var assetName = parts[1];
											var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
											var subObject = subAssets.FirstOrDefault(s => s.name == assetName);
											if (subObject != null)
											{
												guidProperty.stringValue = assetGuid;
												subObjectNameProperty.stringValue = assetName;
												subObjectTypeProperty.stringValue = subObject.GetType().AssemblyQualifiedName;
											}
											else
											{
												LogParseWarning(value, propertyType, $"Failed to find sub asset '{assetName}' at path '{assetPath}'.");
											}
										}
										else
										{
											guidProperty.stringValue = assetGuid;
											subObjectNameProperty.stringValue = string.Empty;
											subObjectTypeProperty.stringValue = string.Empty;
										}
									}
									else
									{
										LogParseWarning(value, propertyType, $"Failed to find asset with GUID '{assetGuid}'.");
									}
								}
								else
								{
									guidProperty.stringValue = string.Empty;
									subObjectNameProperty.stringValue = string.Empty;
									subObjectTypeProperty.stringValue = string.Empty;
								}
							}
							else
							{
								LogParseWarning(value, propertyType, $"Unsupported type of {UnityConstants.Type.AssetReference}. Unable to find {UnityConstants.Type.AssetReference} properties.");
							}
						}
						else
						{
							LogParseWarning(value, propertyType, "Unsupported Property Type.");
						}
						break;

					default:
						LogParseWarning(value, propertyType, "Unsupported Property Type.");
						break;
				}
				serializedObject.ApplyModifiedProperties();
			}
			else
			{
				if (SerializedPropertyUtility.IsArrayElement(m_PropertyPath))
				{
					Debug.LogWarning($"Unable to find array element at path {m_PropertyPath} on {nameof(Object)} {m_RootObject.name}. Did you update the size of the array?");
				}
				else
				{
					Debug.LogError($"Unable to find property at path {m_PropertyPath} on {nameof(Object)} {m_RootObject.name}");
				}
			}
		}

		private void LogParseWarning(string value, SerializedPropertyType propertyType, string message)
		{
			Debug.LogWarning($"Cannot parse '{value.GetEscapedText()}' to type {propertyType} for property at path {m_PropertyPath} on {nameof(Object)} {m_RootObject.name}.\n{message}");
		}

		public void AddNewLine()
		{
			var serializedObject = GetSerializedObject();
			var property = GetSerializedProperty(serializedObject);
			if (property.propertyPath == UnityConstants.Field.Name)
			{
				return;
			}
			if (property.propertyType != SerializedPropertyType.String)
			{
				return;
			}
			var editor = typeof(EditorGUI).GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as TextEditor;
			if (editor == null)
			{
				return;
			}
			editor.Insert('\n');
			property.stringValue = editor.text;
			serializedObject.ApplyModifiedProperties();

			var currentIndex = editor.cursorIndex;
			// Unity's default TextFields do not support multilines, brute force it.
			editor.cursorIndex = currentIndex;
			editor.selectIndex = currentIndex;
			EditorApplication.delayCall += () =>
			{
				if (editor != null)
				{
					editor.cursorIndex = currentIndex;
					editor.selectIndex = currentIndex;
					EditorApplication.delayCall += () =>
					{
						// If the user changed the index inbetween frames then we ignore this second delayed call, which would reset it back.
						if (editor != null && editor.cursorIndex != editor.selectIndex)
						{
							editor.cursorIndex = currentIndex;
							editor.selectIndex = currentIndex;
						}
					};
				}
			};
		}

		public bool IsInputFieldProperty(bool isScriptableObject)
		{
			var property = GetSerializedProperty();
			return property.IsInputFieldProperty(isScriptableObject);
		}

		// Draw our own selected cell border for properties Unity doesn't automatically.
		public bool NeedsSelectionBorder(bool lockNames = false)
		{
			var property = GetSerializedProperty();
			// Name property is readonly on certain fields like Enum TextAssets.
			var isNameProperty = property.propertyPath == UnityConstants.Field.Name;
			return (!isNameProperty && property.IsReadOnly() || (isNameProperty && lockNames)) || property.propertyType == SerializedPropertyType.AnimationCurve || property.propertyType == SerializedPropertyType.Gradient;
		}

		// Various default Unity Engine Components have different limitations on how backing fields are serialized and accessed.
		private static bool IsDefaultUnityEngineType(SerializedObject serializedObject)
		{
			return serializedObject.targetObject.GetType().FullName.StartsWith(UnityConstants.Type.UnityEngine);
		}
	}
}
