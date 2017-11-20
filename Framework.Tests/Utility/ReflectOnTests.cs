using Framework.Utility;
using Xunit;

namespace Framework.Tests.Utility
{
     public class ReflectOnTests {
        private class TestClass {
            public int MyField;
            public int MyField2;
            public int MyProperty { get { MyField = 5; return MyField; } }
            public int MyProperty2 { get { MyField2 = 5; return MyField2; } }
            public void MyMethod(int i) { }
            public int MyMethod(string s) { return 5; }
            public void MyMethod2(int i) { }
            public int MyMethod2(string s) { return 5; }
            public TestClass MyTestClass { get { return null; } }
            public TestClass this[int i] { get { return null; } }
        }

        [Fact]
        public void ReflectOnGetMemberShouldReturnCorrectMemberInfo() {
            Assert.Equal(ReflectOn<TestClass>.GetMember(p => p.MyField).Name, ("MyField"));
            Assert.Equal(ReflectOn<TestClass>.GetMember(p => p.MyMethod(5)).Name, ("MyMethod"));
        }

        [Fact]
        public void ReflectOnShouldWorkOnFields() {
            Assert.Equal(ReflectOn<TestClass>.GetField(p => p.MyField).Name, ("MyField"));
            Assert.Equal(ReflectOn<TestClass>.GetField(p => p.MyField2).Name, ("MyField2"));
        }

        [Fact]
        public void ReflectOnShouldWorkOnProperties() {
            Assert.Equal(ReflectOn<TestClass>.GetProperty(p => p.MyProperty).Name, ("MyProperty"));
            Assert.Equal(ReflectOn<TestClass>.GetProperty(p => p.MyProperty2).Name, ("MyProperty2"));
        }

        [Fact]
        public void ReflectOnShouldWorkOnMethods() {
            Assert.Equal(ReflectOn<TestClass>.GetMethod(p => p.MyMethod(5)).Name, ("MyMethod"));
            Assert.Equal(ReflectOn<TestClass>.GetMethod(p => p.MyMethod("")).Name, ("MyMethod"));
            Assert.Equal(ReflectOn<TestClass>.GetMethod(p => p.MyMethod("")).ReturnType, (typeof(int)));

            Assert.Equal(ReflectOn<TestClass>.GetMethod(p => p.MyMethod2(5)).Name, ("MyMethod2"));
            Assert.Equal(ReflectOn<TestClass>.GetMethod(p => p.MyMethod2("")).Name, ("MyMethod2"));
            Assert.Equal(ReflectOn<TestClass>.GetMethod(p => p.MyMethod2("")).ReturnType, (typeof(int)));
        }

        [Fact]
        public void ReflectOnShouldWorkOnDottedProperties() {
            Assert.Equal(ReflectOn<TestClass>.NameOf(p => p.MyTestClass.MyTestClass.MyProperty), ("MyTestClass.MyTestClass.MyProperty"));
        }

        [Fact]
        public void ReflectOnShouldWorkOnIndexers() {
            Assert.Equal(ReflectOn<TestClass>.NameOf(p => p[0].MyTestClass[1].MyProperty), ("[0].MyTestClass[1].MyProperty"));
            int j = 5;
            int index = j;
            Assert.Equal(ReflectOn<TestClass>.NameOf(p => p.MyTestClass[index].MyProperty), ("MyTestClass[5].MyProperty"));
        }
    }
}