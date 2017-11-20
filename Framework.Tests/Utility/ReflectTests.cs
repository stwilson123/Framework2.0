using Framework.Utility;
using Xunit;

namespace Framework.Tests.Utility
{
   public class ReflectTests {
        private class TestClass {
            public static int MyField;
            public static int MyField2;
            public static int MyProperty { get { MyField = 5; return MyField; } }
            public static int MyProperty2 { get { MyField2 = 5; return MyField2; } }
            public static void MyMethod(int i) { }
            public static int MyMethod(string s) { return 5; }
            public static void MyMethod2(int i) { }
            public static int MyMethod2(string s) { return 5; }
        }

       [Fact]
        public void ReflectGetMemberShouldReturnCorrectMemberInfo() {
            Assert.Equal(Reflect.GetMember(() => TestClass.MyField).Name, ("MyField"));
            Assert.Equal(Reflect.GetMember(() => TestClass.MyMethod(5)).Name, ("MyMethod"));
        }

        [Fact]
        public void ReflectShouldWorkOnFields() {
            Assert.Equal(Reflect.GetField(() => TestClass.MyField).Name, ("MyField"));
            Assert.Equal(Reflect.GetField(() => TestClass.MyField2).Name, ("MyField2"));
        }

        [Fact]
        public void ReflectShouldWorkOnProperties() {
            Assert.Equal(Reflect.GetProperty(() => TestClass.MyProperty).Name, ("MyProperty"));
            Assert.Equal(Reflect.GetProperty(() => TestClass.MyProperty2).Name, ("MyProperty2"));
        }

        [Fact]
        public void ReflectShouldWorkOnMethods() {
            Assert.Equal(Reflect.GetMethod(() => TestClass.MyMethod(5)).Name, ("MyMethod"));
            Assert.Equal(Reflect.GetMethod(() => TestClass.MyMethod("")).Name, ("MyMethod"));
            Assert.Equal(Reflect.GetMethod(() => TestClass.MyMethod("")).ReturnType, (typeof(int)));

            Assert.Equal(Reflect.GetMethod(() => TestClass.MyMethod2(5)).Name, ("MyMethod2"));
            Assert.Equal(Reflect.GetMethod(() => TestClass.MyMethod2("")).Name, ("MyMethod2"));
            Assert.Equal(Reflect.GetMethod(() => TestClass.MyMethod2("")).ReturnType, (typeof(int)));
        }
    }
}