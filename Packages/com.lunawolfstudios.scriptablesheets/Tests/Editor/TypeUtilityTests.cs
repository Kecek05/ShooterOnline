using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LunaWolfStudiosEditor.ScriptableSheets.EditorTests
{
	[Flags]
	public enum TestFlagsEnum
	{
		None = 0,
		One = 1,
		Two = 2,
	}

	public class ValidConcreteClass { }
	public class ValidDerivedObject : Object { }
	public abstract class InvalidAbstractClass { }
	public class InvalidGenericClass<T> { }
	public class InvalidMonoBehaviour : MonoBehaviour { }
	public class InvalidNestedParent
	{
		[Serializable]
		public class InvalidNestedClass { }
	}
	public class ScriptableSingletonClass : ScriptableSingleton<ScriptableObject> { }

	[TestFixture]
	[Category(TestUtility.MainCategory)]
	public class TypeUtilityTests
	{
		[Test]
		public void HasFlagsAttribute_ReturnsTrueForFlagsEnum()
		{
			Assert.IsTrue(typeof(TestFlagsEnum).HasFlagsAttribute());
		}

		[Test]
		public void HasFlagsAttribute_ReturnsFalseForNonFlags()
		{
			Assert.IsFalse(typeof(int).HasFlagsAttribute());
			Assert.IsFalse(typeof(ValidConcreteClass).HasFlagsAttribute());
		}

		[Test]
		public void IsScriptableSingleton_ReturnsTrue_For_ScriptableSingleton()
		{
			Assert.IsTrue(typeof(ScriptableSingletonClass).IsScriptableSingleton());
		}

		[Test]
		public void IsValidConcreteClass_ReturnsFalse_For_ScriptableSingleton()
		{
			Assert.IsFalse(typeof(ValidConcreteClass).IsScriptableSingleton());
		}

		[Test]
		public void IsValidConcreteType_ReturnsTrue_For_ConcreteSerializableClass()
		{
			Assert.IsTrue(typeof(ValidConcreteClass).IsValidConcreteType());
		}

		[Test]
		public void IsValidConcreteType_ReturnsFalse_For_AbstractSerializableClass()
		{
			Assert.IsFalse(typeof(InvalidAbstractClass).IsValidConcreteType());
		}

		[Test]
		public void IsValidConcreteType_ReturnsFalse_For_GenericClass()
		{
			Assert.IsFalse(typeof(InvalidGenericClass<int>).IsValidConcreteType());
		}

		[Test]
		public void IsValidConcreteType_ReturnsFalse_For_NestedClass()
		{
			Assert.IsFalse(typeof(InvalidNestedParent.InvalidNestedClass).IsValidConcreteType());
		}

		[Test]
		public void IsValidUnityObjectSubclass_ReturnsTrueForDerivedUnityObject()
		{
			Assert.IsTrue(typeof(ValidDerivedObject).IsValidUnityObjectSubclass());
		}

		[Test]
		public void IsValidUnityObjectSubclass_ReturnsFalseForBaseTypes()
		{
			Assert.IsFalse(typeof(ScriptableObject).IsValidUnityObjectSubclass());
			Assert.IsFalse(typeof(MonoBehaviour).IsValidUnityObjectSubclass());
			Assert.IsFalse(typeof(Object).IsValidUnityObjectSubclass());
			Assert.IsFalse(((Type) null).IsValidUnityObjectSubclass());
		}
	}
}
