using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets.Shared
{
	public static class TypeUtility
	{
		public static bool HasFlagsAttribute(this Type type)
		{
			return Attribute.IsDefined(type, typeof(FlagsAttribute));
		}

		public static bool IsScriptableSingleton(this Type type)
		{
			while (type.IsValidUnityObjectSubclass())
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ScriptableSingleton<>))
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		public static bool IsValidConcreteType(this Type type)
		{
			return !type.IsAbstract && !type.IsGenericType && !type.IsNested;
		}

		public static bool IsValidUnityObjectSubclass(this Type type)
		{
			return type != null && type != typeof(ScriptableObject) && type != typeof(MonoBehaviour) && type != typeof(Object);
		}
	}
}
