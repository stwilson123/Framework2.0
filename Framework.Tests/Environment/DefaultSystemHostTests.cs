using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Castle.DynamicProxy.Contributors;
using Framework.Caching;
using Framework.Environment;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor;
using Framework.Environment.Descriptor.Models;
using Framework.Environment.Extensions;
using Framework.Environment.Extensions.Folders;
using Framework.Environment.Extensions.Models;
using Framework.Environment.ShellBuilder;
using Framework.FileSystems.AppData;
using Framework.FileSystems.VirtualPath;
using Framework.Mvc.ModelBinders;
using Framework.Mvc.Routes;
using Framework.Owin;
using Framework.Tests.Environment.ShellBuilders;
using Framework.Tests.Environment.TestDependencies;
using Framework.Tests.Stub;
using Framework.Tests.Utility.Container;
using Xunit; 

namespace Framework.Tests.Environment
{
   public class DefaultSystemHostTests {
        private IContainer _container;
        private ILifetimeScope _lifetime;
        private RouteCollection _routeCollection;
        private ModelBinderDictionary _modelBinderDictionary;
        private ControllerBuilder _controllerBuilder;
        private ViewEngineCollection _viewEngineCollection;

         
        public void Init() {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);

            _controllerBuilder = new ControllerBuilder();
            _routeCollection = new RouteCollection();
            _modelBinderDictionary = new ModelBinderDictionary();
            _viewEngineCollection = new ViewEngineCollection { new WebFormViewEngine() };

            _container = SystemStarter.CreateHostContainer(
                builder => {
                    builder.RegisterInstance(new StubShellSettingsLoader()).As<IShellSettingsManager>();
                    builder.RegisterType<RoutePublisher>().As<IRoutePublisher>();
                    builder.RegisterType<ModelBinderPublisher>().As<IModelBinderPublisher>();
                    builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>();
                    builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();
                    builder.RegisterType<StubVirtualPathMonitor>().As<IVirtualPathMonitor>();
                    builder.RegisterInstance(appDataFolder).As<IAppDataFolder>();
                    builder.RegisterInstance(_controllerBuilder);
                    builder.RegisterInstance(_routeCollection);
                    builder.RegisterInstance(_modelBinderDictionary);
                    builder.RegisterInstance(_viewEngineCollection);
                    builder.RegisterAutoMocking()
                        .Ignore<IExtensionFolders>()
                        .Ignore<IRouteProvider>()
                        .Ignore<IHttpRouteProvider>()
                        .Ignore<Mvc.ModelBinders.IModelBinderProvider>()
                        .Ignore<IWorkContextEvents>()
                        .Ignore<IOwinMiddlewareProvider>();
                });
            _lifetime = _container.BeginLifetimeScope();

            _container.Mock<IContainerProvider>()
                .SetupGet(cp => cp.ApplicationContainer).Returns(_container);
            _container.Mock<IContainerProvider>()
                .SetupGet(cp => cp.RequestLifetime).Returns(_lifetime);
            _container.Mock<IContainerProvider>()
                .Setup(cp => cp.EndRequestLifetime()).Callback(() => _lifetime.Dispose());

            _container.Mock<IShellDescriptorManager>()
                .Setup(cp => cp.GetShellDescriptor()).Returns(default(ShellDescriptor));

            _container.Mock<ISystemShellEvents>()
                .Setup(e => e.Activated());

            _container.Mock<ISystemShellEvents>()
                .Setup(e => e.Terminating()).Callback(() => new object());
        }

        public class StubExtensionManager : IExtensionManager {
            public ExtensionDescriptor GetExtension(string name) {
                throw new NotImplementedException();
            }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                var ext = new ExtensionDescriptor { Id = "System.Framework" };
                ext.Features = new[] { new FeatureDescriptor { Extension = ext, Id = ext.Id } };
                yield return ext;

                var settings = new ExtensionDescriptor { Id = "Settings" };
                settings.Features = new[] { new FeatureDescriptor { Extension = settings, Id = settings.Id } };
                yield return settings;
            }

            public IEnumerable<FeatureDescriptor> AvailableFeatures() {
                // note - doesn't order properly
                return AvailableExtensions().SelectMany(ed => ed.Features);
            }

            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
                foreach (var descriptor in featureDescriptors) {
                    if (descriptor.Id == "System.Framework") {
                        yield return FrameworkFeature(descriptor);
                    }
                }
            }

            private Feature FrameworkFeature(FeatureDescriptor descriptor) {
                return new Feature {
                    Descriptor = descriptor,
                    ExportedTypes = new[] {
                        typeof (DefaultShellContainerFactoryTests.TestDependency),
                        typeof (TestSingletonDependency),
                        typeof (TestTransientDependency),
                    }
                };
            }

            public void Monitor(Action<IVolatileToken> monitor) {
                throw new NotImplementedException();
            }
        }

        public class StubShellSettingsLoader : IShellSettingsManager {
            private readonly List<ShellSettings> _shellSettings = new List<ShellSettings> { new ShellSettings { Name = ShellSettings.DefaultName, State = TenantState.Running } };

            public IEnumerable<ShellSettings> LoadSettings() {
                return _shellSettings.AsEnumerable();
            }

            public void SaveSettings(ShellSettings settings) {
                _shellSettings.Add(settings);
            }
        }

       public DefaultSystemHostTests()
       {
           Init();
           
       }

        [Fact(Skip = "containers are disposed when calling BeginRequest, maybe by the StubVirtualPathMonitor")]
        public void NormalDependenciesShouldBeUniquePerRequestContainer() {
            var host = _lifetime.Resolve<ISystemHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            ((IShellDescriptorManagerEventHandler)host).Changed(null, ShellSettings.DefaultName);
            host.BeginRequest(); // force reloading the shell
            var container2 = host.CreateShellContainer_Obsolete();
            var requestContainer1a = container1.BeginLifetimeScope();
            var requestContainer1b = container1.BeginLifetimeScope();
            var requestContainer2a = container2.BeginLifetimeScope();
            var requestContainer2b = container2.BeginLifetimeScope();

            var dep1 = container1.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var dep1a = requestContainer1a.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var dep1b = requestContainer1b.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var dep2 = container2.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var dep2a = requestContainer2a.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var dep2b = requestContainer2b.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();

            Assert.NotSame(dep1,  dep2 );
            Assert.NotSame(dep1,  dep1a );
            Assert.NotSame(dep1,  dep1b );
            Assert.NotSame(dep2,  dep2a );
            Assert.NotSame(dep2,  dep2b );

            var again1 = container1.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var again1a = requestContainer1a.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var again1b = requestContainer1b.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var again2 = container2.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var again2a = requestContainer2a.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();
            var again2b = requestContainer2b.Resolve<DefaultShellContainerFactoryTests.ITestDependency>();

            Assert.Same(again1, dep1 );
            Assert.Same(again1a, dep1a );
            Assert.Same(again1b, dep1b );
            Assert.Same(again2, dep2 );
            Assert.Same(again2a, dep2a );
            Assert.Same(again2b, dep2b );
        }

        [Fact]
        public void SingletonDependenciesShouldBeUniquePerShell() {
            var host = _lifetime.Resolve<ISystemHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            var container2 = host.CreateShellContainer_Obsolete();
            var requestContainer1a = container1.BeginLifetimeScope();
            var requestContainer1b = container1.BeginLifetimeScope();
            var requestContainer2a = container2.BeginLifetimeScope();
            var requestContainer2b = container2.BeginLifetimeScope();

            var dep1 = container1.Resolve<ITestSingletonDependency>();
            var dep1a = requestContainer1a.Resolve<ITestSingletonDependency>();
            var dep1b = requestContainer1b.Resolve<ITestSingletonDependency>();
            var dep2 = container2.Resolve<ITestSingletonDependency>();
            var dep2a = requestContainer2a.Resolve<ITestSingletonDependency>();
            var dep2b = requestContainer2b.Resolve<ITestSingletonDependency>();

            //Assert.That(dep1, Is.Not.SameAs(dep2));
           Assert.Same(dep1,  dep1a );
           Assert.Same(dep1,  dep1b );
           Assert.Same(dep2,  dep2a );
           Assert.Same(dep2,  dep2b );
        }

        [Fact]
        public void TransientDependenciesShouldBeUniquePerResolve() {
            var host = _lifetime.Resolve<ISystemHost>();
            var container1 = host.CreateShellContainer_Obsolete();
            var container2 = host.CreateShellContainer_Obsolete();
            var requestContainer1a = container1.BeginLifetimeScope();
            var requestContainer1b = container1.BeginLifetimeScope();
            var requestContainer2a = container2.BeginLifetimeScope();
            var requestContainer2b = container2.BeginLifetimeScope();

            var dep1 = container1.Resolve<ITestTransientDependency>();
            var dep1a = requestContainer1a.Resolve<ITestTransientDependency>();
            var dep1b = requestContainer1b.Resolve<ITestTransientDependency>();
            var dep2 = container2.Resolve<ITestTransientDependency>();
            var dep2a = requestContainer2a.Resolve<ITestTransientDependency>();
            var dep2b = requestContainer2b.Resolve<ITestTransientDependency>();

            Assert.NotSame(dep1,  dep2 );
            Assert.NotSame(dep1,  dep1a );
            Assert.NotSame(dep1,  dep1b );
            Assert.NotSame(dep2,  dep2a );
            Assert.NotSame(dep2,  dep2b );

            var again1 = container1.Resolve<ITestTransientDependency>();
            var again1a = requestContainer1a.Resolve<ITestTransientDependency>();
            var again1b = requestContainer1b.Resolve<ITestTransientDependency>();
            var again2 = container2.Resolve<ITestTransientDependency>();
            var again2a = requestContainer2a.Resolve<ITestTransientDependency>();
            var again2b = requestContainer2b.Resolve<ITestTransientDependency>();

            Assert.NotSame(again1,  dep1 );
            Assert.NotSame(again1a,  dep1a );
            Assert.NotSame(again1b,  dep1b );
            Assert.NotSame(again2,  dep2 );
            Assert.NotSame(again2a,  dep2a );
            Assert.NotSame(again2b,  dep2b );

        }
    }

    public static class TextExtensions {
        public static ILifetimeScope CreateShellContainer_Obsolete(this ISystemHost host) {
            return ((DefaultSystemHost)host)
                .Current
                .Single(x => x.Settings.Name == ShellSettings.DefaultName)
                .LifetimeScope;
        }

        public static ISystemShell CreateShell_Obsolete(this ISystemHost host) {
            return host.CreateShellContainer_Obsolete().Resolve<ISystemShell>();
        }
    }
}