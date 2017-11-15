using Autofac;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Framework.Environment;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor;
using Framework.Environment.Descriptor.Models;
using Framework.Environment.ShellBuilder;
using Framework.Environment.ShellBuilder.Models;
using Framework.Mvc;
using Framework.Tests.Stub;
using Framework.Tests.Utility.Container;
using Moq;
using Xunit;
using IContainer = Autofac.IContainer;

namespace Framework.Tests.Environment.ShellBuilders
{
    public class DefaultShellContextFactoryTests
    {
        private IContainer _container;

        
        public void Init()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>();
            builder.RegisterModule(new MvcModule());
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterAutoMocking(Moq.MockBehavior.Strict);
            _container = builder.Build();
        }

        public DefaultShellContextFactoryTests()
        {
            Init();
        }
        [Fact]
        public void NormalExecutionReturnsExpectedObjects()
        {
            var settings = new ShellSettings { Name = ShellSettings.DefaultName };
            var descriptor = new ShellDescriptor { SerialNumber = 6655321 };
            var blueprint = new ShellBlueprint();
            var shellLifetimeScope = _container.BeginLifetimeScope("shell");
            var httpContext = new StubHttpContext();

            _container.Mock<IShellDescriptorCache>()
                .Setup(x => x.Fetch(ShellSettings.DefaultName))
                .Returns(descriptor);

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(settings, descriptor))
                .Returns(blueprint);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(settings, blueprint))
                .Returns(shellLifetimeScope);

            _container.Mock<IShellDescriptorManager>()
                .Setup(x => x.GetShellDescriptor())
                .Returns(descriptor);

            _container.Mock<IWorkContextEvents>()
                .Setup(x => x.Started());

            _container.Mock<IHttpContextAccessor>()
                .Setup(x => x.Current())
                .Returns(default(HttpContextBase));

            var factory = _container.Resolve<IShellContextFactory>();

            var context = factory.CreateShellContext(settings);

            Assert.Same(context.Settings, settings);
            Assert.Same(context.Descriptor, descriptor);
            Assert.Same(context.Blueprint, blueprint);
            Assert.Same(context.LifetimeScope, shellLifetimeScope);
            //TODO why is same
            Assert.Same(context.Shell, shellLifetimeScope.Resolve<ISystemShell>());
        }

        [Fact]
        public void CreatingSetupContextUsesSystemSetupFeature()
        {
            var settings = default(ShellSettings);
            var descriptor = default(ShellDescriptor);
            var blueprint = new ShellBlueprint();

            _container.Mock<ICompositionStrategy>()
                .Setup(x => x.Compose(It.IsAny<ShellSettings>(), It.IsAny<ShellDescriptor>()))
                .Callback((ShellSettings s, ShellDescriptor d) => {
                    settings = s;
                    descriptor = d;
                })
                .Returns(blueprint);

            _container.Mock<IShellContainerFactory>()
                .Setup(x => x.CreateContainer(It.IsAny<ShellSettings>(), blueprint))
                .Returns(_container.BeginLifetimeScope("shell"));

            var factory = _container.Resolve<IShellContextFactory>();
            var context = factory.CreateSetupContext(new ShellSettings { Name = ShellSettings.DefaultName });

            Assert.Contains(context.Descriptor.Features, t=> t.Name == "System.Setup");
        }
    }
}
