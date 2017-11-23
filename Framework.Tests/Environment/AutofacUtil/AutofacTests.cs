using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Features.Metadata;
using Xunit;

namespace Framework.Tests.Environment.AutofacUtil
{
    public class AutofacTests {
        public interface IFoo: IDependency { }
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

        public delegate IFoo CreateFoo5(string dataFolder, string connectionString);

        public interface IFoo1 : IDependency
        {
            IFoo GetFool5(string foolA,string foolB);

        }
        
        public class Foo4 : IFoo1
        {
            private readonly IEnumerable<Meta<CreateFoo5>> _providers;

            public Foo4(IEnumerable<Meta<CreateFoo5>> providers)
            {
                _providers = providers;
            }

            public IFoo GetFool5(string foolA,string foolB)
            {
                return _providers.First().Value(foolA,foolB);
            }
        }
        
        public class  Foo5 : IFoo
        {
            private readonly string _dataFolder;
            private readonly string _connectionString;

            public Foo5(string dataFolder, string connectionString)
            {
                _dataFolder = dataFolder;
                _connectionString = connectionString;
            }
        }
         
        [Fact]
        public void DelegateInjectTest() {
            var rootBuilder = new ContainerBuilder();
            rootBuilder.RegisterType<Foo5>().As<IFoo>();
            rootBuilder.RegisterType<Foo1>().As<IFoo>();

            rootBuilder.RegisterType<Foo4>().As<IFoo1>();
            var rootContainer = rootBuilder.Build();


            var a = rootContainer.Resolve<IFoo1>();
            var data = a.GetFool5("A","B");
        }
    }
}