using LunaWolfStudiosEditor.ScriptableSheets.Shared;
using NUnit.Framework;

namespace LunaWolfStudiosEditor.ScriptableSheets.EditorTests
{
	[TestFixture]
	[Category(TestUtility.MainCategory)]
	public class SerializedPropertyUtilityTests
	{
		[TestCase("", "DisplayName", ExpectedResult = "DisplayName")]
		[TestCase(".", "DisplayName", ExpectedResult = "DisplayName")]
		[TestCase("m_", "DisplayName", ExpectedResult = "DisplayName")]
		[TestCase("m_.m_", "DisplayName", ExpectedResult = "DisplayName")]
		[TestCase("m_.m_.m_", "DisplayName", ExpectedResult = "DisplayName")]
		[TestCase("m_.m_.m_X", "DisplayName", ExpectedResult = "X")]
		[TestCase(".x", "DisplayName", ExpectedResult = "X")]
		[TestCase(".x.", "DisplayName", ExpectedResult = "X")]
		[TestCase("x", "X", ExpectedResult = "X")]
		[TestCase("x.y", "Y", ExpectedResult = "X Y")]
		[TestCase("m_X", "X", ExpectedResult = "X")]
		[TestCase("m_X.m_Y", "Y", ExpectedResult = "X Y")]
		[TestCase("foobar", "Foobar", ExpectedResult = "Foobar")]
		[TestCase("m_FooBar", "Foo Bar", ExpectedResult = "Foo Bar")]
		[TestCase("m_Bar.x", "X", ExpectedResult = "Bar X")]
		[TestCase("m_m_Bar.m_m_x", "X", ExpectedResult = "Bar X")]
		[TestCase("m_Foo.m_Bar", "Bar", ExpectedResult = "Foo Bar")]
		[TestCase("m_Foo.m_Bar.x", "X", ExpectedResult = "Bar X")]
		[TestCase("m_Foo.m_Bar.m_x", "X", ExpectedResult = "Bar X")]
		[TestCase("m_foo.m_bar.m_baz.m_x", "X", ExpectedResult = "Baz X")]
		[TestCase("m_foo.m_bar.m_baz", "Baz", ExpectedResult = "Bar Baz")]
		[TestCase("m_Bar.Array.data[0]", "Element 0", ExpectedResult = "Bar[0]")]
		[TestCase("m_Bar.Array.data[0].m_X", "Element 0", ExpectedResult = "Bar[0] X")]
		[TestCase("m_Bar.Array.data[1].m_Baz", "Baz", ExpectedResult = "Bar[1] Baz")]
		[TestCase("m_Foo.m_Bar.Array.data[1].m_Baz", "Baz", ExpectedResult = "Bar[1] Baz")]
		[TestCase("m_Foo.m_Bar.Array.data[0]", "Element 0", ExpectedResult = "Bar[0]")]
		[TestCase("m_Foo.m_Bar.Array.data[10]", "Element 10", ExpectedResult = "Bar[10]")]
		[TestCase("m_Foo.m_Bar.Array.data[0].m_Baz", "Baz", ExpectedResult = "Bar[0] Baz")]
		[TestCase("m_Foo.m_Bar.Array.data[0].m_BazArray", "Baz Array", ExpectedResult = "Bar[0] Baz Array")]
		[TestCase("m_Bar.Array.data[0].m_Baz.Array.data[10]", "Element 10", ExpectedResult = "Bar[0] Baz[10]")]
		[TestCase("m_Foo.m_Bar.Array.data[0].m_Baz.Array.data[10]", "Element 10", ExpectedResult = "Bar[0] Baz[10]")]
		[TestCase("m_Foo.m_Bar.Array.data[0].m_Baz.Array.data[0].hello", "Hello", ExpectedResult = "Baz[0] Hello")]
		[TestCase("m_Foo.m_Bar.Array.data[0].m_Baz.Array.data[0].hello.Array.data[1].world", "World", ExpectedResult = "Hello[1] World")]
		[TestCase("Size", "Size", ExpectedResult = "Size")]
		[TestCase("m_Size.m_Size", "Size", ExpectedResult = "Size Size")]
		[TestCase("m_Size.Size.m_size", "Size", ExpectedResult = "Size Size")]
		[TestCase("m_Foo.Size", "Size", ExpectedResult = "Foo Size")]
		[TestCase("m_Foo.size", "Size", ExpectedResult = "Foo Size")]
		[TestCase("m_Bar.Array.size", "Size", ExpectedResult = "Bar Size")]
		[TestCase("m_Foo.m_Bar.Array.size", "Size", ExpectedResult = "Bar Size")]
		[TestCase("m_Bar.Array.data[0].m_Baz.m_Size", "Size", ExpectedResult = "Baz Size")]
		[TestCase("m_Bar.Array.data[0].m_Baz.Array.size", "Size", ExpectedResult = "Baz Size")]
		[TestCase("m_Foo.m_Bar.Array.data[0].m_Baz.Array.data[0].hello.Array.size", "Size", ExpectedResult = "Hello Size")]
		[TestCase("<FooBar>k__BackingField", "Foo Bar", ExpectedResult = "Foo Bar")]
		[TestCase("<Foo>k__BackingField.<Bar>k__BackingField", "Foo Bar", ExpectedResult = "Foo Bar")]
		[TestCase("<Foo>k__BackingField.<Bar>k__BackingField.Array.data[0]", "Foo Bar", ExpectedResult = "Bar[0]")]
		[TestCase("<Foo>k__BackingField.Array.data[0].<Bar>k__BackingField.Array.data[1]", "Foo Bar", ExpectedResult = "Foo[0] Bar[1]")]
		[Category(TestUtility.MainCategory)]
		public string FriendlyPropertyPathTests(string propertyPath, string displayName)
		{
			return SerializedPropertyUtility.FriendlyPropertyPath(propertyPath, displayName);
		}

		[TestCase("Sprite PPtr<$Sprite>", "Sprite")]
		[TestCase("PPtr<Material PPtr<Material>", "Material")]
		[TestCase("NoBracketsFound", "NoBracketsFound")]
		[TestCase("", "")]
		[TestCase("<>", "")]
		[Category(TestUtility.MainCategory)]
		public void FriendlyTypeTests(string type, string expected)
		{
			Assert.AreEqual(expected, SerializedPropertyUtility.FriendlyType(type));
		}

		[TestCase("m_Foo.Array.data[0]", ExpectedResult = true, TestName = "Path with array index should return true")]
		[TestCase("m_Foo.m_Bar.Array.data[1]", ExpectedResult = true, TestName = "Path with nested array index should return true")]
		[TestCase("", ExpectedResult = false, TestName = "Empty path should return false")]
		[TestCase("m_Foo", ExpectedResult = false, TestName = "Path without array index should return false")]
		[TestCase("m_Foo.m_Bar", ExpectedResult = false, TestName = "Nested path without array index should return false")]
		[Category(TestUtility.MainCategory)]
		public bool IsArrayElementTests(string propertyPath)
		{
			return SerializedPropertyUtility.IsArrayElement(propertyPath);
		}
	}
}
