using System.Collections.Generic;
using System.Linq;
using Autofac;
using Xunit;

namespace Framework.Tests.Environment.AutofacUtil
{
    public class AutofacTests {
        public interface IFoo { }
        public class Foo1 : IFoo { }
        public class Foo2 : IFoo { }
        public class Foo3 : IFoo { }

       // [Test(Description = "Exercises a problem in a previous version, to make sure older Autofac.dll isn't picked up")]
        [Fact]
        public void EnumerablesFromDifferentLifetimeScopesShouldReturnDifferentCollections() {
            var rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterType<Foo1>().As<IFoo>();
            var rootContainer = rootBuilder.Build();

            var scopeA = rootContainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Foo2>().As<IFoo>());
            var arrayA = scopeA.Resolve<IEnumerable<IFoo>>().ToArray();

            var scopeB = rootContainer.BeginLifetimeScope(
                scopeBuilder => scopeBuilder.RegisterType<Foo3>().As<IFoo>());
            var arrayB = scopeB.Resolve<IEnumerable<IFoo>>().ToArray();

            Assert.Equal(arrayA.Count(), 2);
            Assert.Contains(arrayA,t => (t as Foo1) != null);
            Assert.Contains(arrayA, t => (t as Foo2) != null);

            Assert.Equal(arrayB.Count(), 2);
            Assert.Contains(arrayB, t => (t as Foo1) != null);
            Assert.Contains(arrayB, t => (t as Foo3) != null);
        }
    }
}