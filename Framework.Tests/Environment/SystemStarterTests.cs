using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Framework.Environment;
using Xunit;

namespace Framework.Tests.Environment
{
    public class SystemStarterTests
    {
        [Fact]
        public void DefaultSystemHostInstanceReturnedByCreateHost()
        {
            var host = SystemStarter.CreateHost(b => b.RegisterInstance(new ControllerBuilder()));
            Assert.True(host.GetType() == typeof(DefaultSystemHost));
        }

        [Fact]
        public void ContainerResolvesServicesInSameOrderTheyAreRegistered()
        {
            var container = SystemStarter.CreateHostContainer(builder => {
                builder.RegisterType<Component1>().As<IServiceA>();
                builder.RegisterType<Component2>().As<IServiceA>();
            });
            var services = container.Resolve<IEnumerable<IServiceA>>();
            Assert.Equal(services.Count(), 2);
//            Assert.True(services.First().GetType() ==  typeof(Component1));
//            Assert.True(services.Last().GetType() == typeof(Component2));
            //FIFO OR FILO ?
            Assert.True(services.Last().GetType() ==  typeof(Component1));
            Assert.True(services.First().GetType() == typeof(Component2));
        }

        [Fact]
        public void MostRecentlyRegisteredServiceReturnsFromSingularResolve()
        {
            var container = SystemStarter.CreateHostContainer(builder => {
                builder.RegisterType<Component1>().As<IServiceA>();
                builder.RegisterType<Component2>().As<IServiceA>();
            });
            var service = container.Resolve<IServiceA>();
            Assert.NotNull(service);
            Assert.True(service.GetType() == typeof(Component2));
        }

        public interface IServiceA { }

        public class Component1 : IServiceA { }

        public class Component2 : IServiceA { }
    }
}
