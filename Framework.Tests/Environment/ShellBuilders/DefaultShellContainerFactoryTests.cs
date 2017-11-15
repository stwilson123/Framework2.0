using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Services;
using System.Web.Mvc;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Castle.DynamicProxy;
using Framework.AutofacExtend.DynamicProxy2;
using Framework.Environment;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor.Models;
using Framework.Environment.Extensions.Models;
using Framework.Environment.ShellBuilder;
using Framework.Environment.ShellBuilder.Models;
using Framework.Events;
using Framework.Exceptions.Exception;
using Xunit;

namespace Framework.Tests.Environment.ShellBuilders
{
     public class DefaultShellContainerFactoryTests {
        private IContainer _container;

        
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShellContainerFactory>().As<IShellContainerFactory>();
            builder.RegisterType<ShellContainerRegistrations>().As<IShellContainerRegistrations>();
            builder.RegisterType<ComponentForHostContainer>();
            builder.RegisterType<ControllerActionInvoker>().As<IActionInvoker>();
            _container = builder.Build();
        }

        ShellSettings CreateSettings() {
            return new ShellSettings { Name = ShellSettings.DefaultName };
        }
        ShellBlueprint CreateBlueprint(params ShellBlueprintItem[] items) {
            return new ShellBlueprint {
                Dependencies = items.OfType<DependencyBlueprint>(),
                Controllers = items.OfType<ControllerBlueprint>().Where(bp => typeof(IController).IsAssignableFrom(bp.Type)),
                HttpControllers = items.OfType<ControllerBlueprint>().Where(bp => typeof(IHttpController).IsAssignableFrom(bp.Type)),
                Records = items.OfType<RecordBlueprint>(),
            };
        }

        DependencyBlueprint WithModule<T>() {
            return new DependencyBlueprint { Type = typeof(T), Parameters = Enumerable.Empty<ShellParameter>() };
        }

        ControllerBlueprint WithController<T>(string areaName, string controllerName) {
            return new ControllerBlueprint { Type = typeof(T), AreaName = areaName, ControllerName = controllerName };
        }

        DependencyBlueprint WithDependency<T>() {
            return new DependencyBlueprint { Type = typeof(T), Parameters = Enumerable.Empty<ShellParameter>() };
        }

         public DefaultShellContainerFactoryTests()
         {
             Init();
         }
        [Fact]
        public void ShouldReturnChildLifetimeScopeNamedShell() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint();
            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            Assert.Equal(shellContainer.Tag, "shell");

            var scope = (LifetimeScope)shellContainer;
            Assert.Same(scope.RootLifetimeScope,  _container.Resolve<ILifetimeScope>());
            Assert.NotSame(scope.RootLifetimeScope, shellContainer.Resolve<ILifetimeScope>());
        }



        [Fact]
        public void ControllersAreRegisteredAsKeyedServices() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithModule<TestModule>(),
                WithController<TestController>("foo", "bar"));

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);
            var controllers = shellContainer.Resolve<IIndex<string, IController>>();
            var controller = controllers["foo/bar"];
            Assert.NotNull(controller);
            Assert.True((controller as TestController) != null);
        }
      

        public class TestController : Controller {
        }


        [Fact]
        public void ModulesAreResolvedAndRegistered() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithModule<TestModule>(),
                WithController<TestController>("foo", "bar"));

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var controllerMetas = shellContainer.Resolve<IIndex<string, Meta<IController>>>();
            var metadata = controllerMetas["foo/bar"].Metadata;

            Assert.Equal(metadata["Hello"], "World");
        }


        public class TestModule : Module {
            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
            {
                if (typeof(IContainer).IsAssignableFrom(registration.Activator.LimitType.BaseType))
                    registration.Metadata["Hello"] = "World";
            }    
        }

        [Fact]
        public void ModulesMayResolveHostServices() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithModule<ModuleUsingThatComponent>());

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);
            Assert.Equal(shellContainer.Resolve<string>(),  "Module was loaded");
        }

        public class ComponentForHostContainer {

        }

        public class ModuleUsingThatComponent : Module {
            private readonly ComponentForHostContainer _di;

            public ModuleUsingThatComponent(ComponentForHostContainer di) {
                _di = di;
            }

            protected override void Load(ContainerBuilder builder) {
                builder.RegisterInstance("Module was loaded");
            }
        }

        [Fact]
        public void DependenciesAreResolvable() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDependency>());

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var testDependency = shellContainer.Resolve<ITestDependency>();
            Assert.NotNull(testDependency );
            Assert.NotNull(testDependency as TestDependency);
        }

        public interface ITestDependency : IDependency {

        }
        public class TestDependency : ITestDependency {
        }

        [Fact]
        public void ComponentsImplementingMultipleContractsAreResolvableOnce() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<MultipleDependency>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var multipleDependency1 = shellContainer.Resolve<IMultipleDependency1>();
            var multipleDependency2 = shellContainer.Resolve<IMultipleDependency2>();

            Assert.NotNull(multipleDependency1 );
            Assert.NotNull(multipleDependency2 );

            Assert.NotNull(multipleDependency1 as MultipleDependency);
            Assert.NotNull(multipleDependency2 as MultipleDependency);

            Assert.True(multipleDependency1 == multipleDependency2);
        }

        public interface IMultipleDependency1 : IDependency {

        }
        public interface IMultipleDependency2 : IDependency {

        }
        public class MultipleDependency : IMultipleDependency1, IMultipleDependency2 {

        }

        [Fact]
        public void ExtraInformationCanDropIntoProperties() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                          WithDependency<TestDependency2>());

            blueprint.Dependencies.Single().Feature =
                new Feature { Descriptor = new FeatureDescriptor { Id = "Hello" } };
           
            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var testDependency = shellContainer.Resolve<ITestDependency>();
            Assert.NotNull(testDependency);
            Assert.NotNull(testDependency as TestDependency2);

            var testDependency2 = (TestDependency2)testDependency;
            
            Assert.NotNull(testDependency2.Feature.Descriptor );
            Assert.Equal(testDependency2.Feature.Descriptor.Id,  "Hello");
        }

        public class TestDependency2 : ITestDependency {
            public Feature Feature { get; set; }
        }

        [Fact]
        public void ParametersMayOrMayNotBeUsedAsPropertiesAndConstructorParameters() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDependency3>());

            blueprint.Dependencies.Single().Parameters =
                new[] {
                          new ShellParameter {Name = "alpha", Value = "-a-"},
                          new ShellParameter {Name = "Beta", Value = "-b-"},
                          new ShellParameter {Name = "Gamma", Value = "-g-"},
                      };

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var testDependency = shellContainer.Resolve<ITestDependency>();
            Assert.NotNull(testDependency );
            Assert.NotNull(testDependency as TestDependency3);

            var testDependency3 = (TestDependency3)testDependency;
            Assert.Equal(testDependency3.GetAlpha(), "-a-");
            Assert.Equal(testDependency3.Beta, "-b-");
            Assert.Equal(testDependency3.Delta, "y");
        }

        public class TestDependency3 : ITestDependency {
            private readonly string _alpha;

            public TestDependency3(string alpha) {
                _alpha = alpha;
                Beta = "x";
                Delta = "y";
            }

            public string Beta { get; set; }
            public string Delta { get; set; }

            public string GetAlpha() {
                return _alpha;
            }
        }


        [Fact]
        public void DynamicProxyIsInEffect() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithModule<ProxModule>(),
                WithDependency<ProxDependency>());

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var testDependency = shellContainer.Resolve<IProxDependency>();
            Assert.Equal(testDependency.Hello(), "Foo");

            var blueprint2 = CreateBlueprint(
                WithDependency<ProxDependency>());

            var shellContainer2 = factory.CreateContainer(settings, blueprint2);

            var testDependency2 = shellContainer2.Resolve<IProxDependency>();
            Assert.Equal(testDependency2.Hello(), "World");
        }

        public interface IProxDependency : IDependency {
            string Hello();
        }

        public class ProxDependency : IProxDependency {
            public virtual string Hello() {
                return "World";
            }
        }

        public class ProxIntercept : IInterceptor {
            public void Intercept(IInvocation invocation) {
                invocation.ReturnValue = "Foo";
            }
        }

        public class ProxModule : Module {
            protected override void Load(ContainerBuilder builder) {
                builder.RegisterType<ProxIntercept>();
            }

            protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
                if (registration.Activator.LimitType == typeof(ProxDependency)) {
                    registration.InterceptedBy<ProxIntercept>();
                }
            }
        }

        [Fact]
        public void DynamicProxyAndShellSettingsAreResolvableToSameInstances() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint();

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var proxa = shellContainer.Resolve<DynamicProxyContext>();
            var proxb = shellContainer.Resolve<DynamicProxyContext>();
            var setta = shellContainer.Resolve<ShellSettings>();
            var settb = shellContainer.Resolve<ShellSettings>();

            Assert.NotNull(proxa);
            Assert.Same(proxa, proxb);
            Assert.NotNull(setta);
            Assert.Same(setta, settb);

            var settings2 = CreateSettings();
            var blueprint2 = CreateBlueprint();
            var shellContainer2 = factory.CreateContainer(settings2, blueprint2);

            var proxa2 = shellContainer2.Resolve<DynamicProxyContext>();
            var proxb2 = shellContainer2.Resolve<DynamicProxyContext>();
            var setta2 = shellContainer2.Resolve<ShellSettings>();
            var settb2 = shellContainer2.Resolve<ShellSettings>();

            Assert.NotNull(proxa2);
            Assert.Same(proxa2,proxb2);
            Assert.NotNull(setta2);
            Assert.Same(setta2,settb2);

            Assert.NotSame(proxa, proxa2);
            Assert.NotSame(setta, setta2);
        }

        public interface IStubEventHandlerA : IEventHandler { }
        public interface IStubEventHandlerB : IEventHandler { }
        public class StubEventHandler1 : IStubEventHandlerA { }
        public class StubEventHandler2 : IStubEventHandlerA { }
        public class StubEventHandler3 : IStubEventHandlerB { }

        [Fact]
        public void EventHandlersAreNamedAndResolvedCorrectly()
        {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<StubEventHandler1>(),
                WithDependency<StubEventHandler2>(),
                WithDependency<StubEventHandler3>()
                );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var eventHandlers = shellContainer.ResolveNamed<IEnumerable<IEventHandler>>(typeof(IStubEventHandlerA).Name).ToArray();

            Assert.NotNull(eventHandlers);
            Assert.Equal(eventHandlers.Count(), 2);
            Assert.Contains(eventHandlers, t => t.GetType().BaseType == typeof(StubEventHandler2));
            Assert.Contains(eventHandlers, t => t.GetType().BaseType == typeof(StubEventHandler1));
        }

        public interface ITestDecorator : IDependency { ITestDecorator DecoratedService { get; } }
        public class TestDecoratorImpl1 : ITestDecorator { public ITestDecorator DecoratedService => null; }
        public class TestDecoratorImpl2 : ITestDecorator { public ITestDecorator DecoratedService => null; }
        public class TestDecoratorImpl3 : ITestDecorator { public ITestDecorator DecoratedService => null; }

        public class TestDecorator1 : IDecorator<ITestDecorator>, ITestDecorator {
            public TestDecorator1(ITestDecorator decoratedService) {
                DecoratedService = decoratedService;
            }

            public ITestDecorator DecoratedService { get; }
            public ITestDecorator Inner { get; }
        }

        public class TestDecorator2 : IDecorator<ITestDecorator>, ITestDecorator {
            public TestDecorator2(ITestDecorator decoratedService) {
                DecoratedService = decoratedService;
            }

            public ITestDecorator DecoratedService { get; }
            public ITestDecorator Inner { get; }
        }

        public class TestDecorator3 : IDecorator<ITestDecorator>, ITestDecorator {
            public TestDecorator3(ITestDecorator decoratedService) {
                DecoratedService = decoratedService;
            }

            public ITestDecorator DecoratedService { get; }
            public ITestDecorator Inner { get; }
        }

        [Fact]
        public void DecoratedComponentsAreResolvedToTheDecorator() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDecoratorImpl1>(),
                WithDependency<TestDecorator1>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var decorator = shellContainer.Resolve<ITestDecorator>();

            Assert.NotNull(decorator);
            Assert.NotNull(decorator as TestDecorator1);
            Assert.NotNull(decorator.DecoratedService as TestDecoratorImpl1);
        }

        [Fact]
        public void DecoratedComponentsAreResolvedToTheDecoratorWhenTheDecoratorIsRegisteredFirst() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDecorator1>(),
                WithDependency<TestDecoratorImpl1>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var decorator = shellContainer.Resolve<ITestDecorator>();

            Assert.NotNull(decorator);
            Assert.NotNull(decorator as TestDecorator1);
            Assert.NotNull(decorator.DecoratedService as TestDecoratorImpl1);
        }

        [Fact]
        public void DecoratedComponentsAreNeverResolved() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDecoratorImpl1>(),
                WithDependency<TestDecorator1>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var services = shellContainer.Resolve<IEnumerable<ITestDecorator>>();

            Assert.NotNull(services);
            Assert.Equal(services.Count(), 1);
            Assert.NotNull(services.First() as TestDecorator1);
            Assert.NotNull(services.First().DecoratedService as TestDecoratorImpl1);
        }

        [Fact]
        public void MultipleComponentsCanBeDecoratedWithASingleDecorator() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDecoratorImpl1>(),
                WithDependency<TestDecoratorImpl2>(),
                WithDependency<TestDecoratorImpl3>(),
                WithDependency<TestDecorator1>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);
             
            var services = shellContainer.Resolve<IEnumerable<ITestDecorator>>().ToArray();

            Assert.NotNull(services);
            Assert.Equal(services.Count(), 3);

            foreach (var service in services)
            {
                Assert.NotNull(service.GetType().BaseType == typeof(TestDecorator1));
            }

            Assert.NotNull(services[0].DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl1));
            Assert.NotNull(services[1].DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl2));
            Assert.NotNull(services[2].DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl3));
        }

        [Fact]
        public void ASingleComponentCanBeDecoratedWithMultipleDecorators() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                WithDependency<TestDecoratorImpl1>(),
                WithDependency<TestDecorator1>(),
                WithDependency<TestDecorator2>(),
                WithDependency<TestDecorator3>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var services = shellContainer.Resolve<IEnumerable<ITestDecorator>>().ToArray();

            Assert.NotNull(services);
            Assert.Equal(services.Count(),  1);

            var service = services[0];

            Assert.NotNull(service.GetType().BaseType == typeof(TestDecorator3) );
            Assert.NotNull(service.DecoratedService.GetType().BaseType == typeof(TestDecorator2) );
            Assert.NotNull(service.DecoratedService.DecoratedService.GetType().BaseType == typeof(TestDecorator1) );
            Assert.NotNull(service.DecoratedService.DecoratedService.DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl1) );
        }

        [Fact]
        public void MultipleComponentsCanBeDecoratedWithMultipleDecorators() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                    WithDependency<TestDecoratorImpl1>(),
                    WithDependency<TestDecoratorImpl2>(),
                    WithDependency<TestDecoratorImpl3>(),
                    WithDependency<TestDecorator1>(),
                    WithDependency<TestDecorator2>(),
                    WithDependency<TestDecorator3>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();
            var shellContainer = factory.CreateContainer(settings, blueprint);

            var services = shellContainer.Resolve<IEnumerable<ITestDecorator>>().ToArray();

            Assert.NotNull(services);
            Assert.Equal(services.Count(),  3);

            foreach (var service in services)
            {
                Assert.NotNull(service.GetType().BaseType == typeof(TestDecorator3));
                Assert.NotNull(service.DecoratedService.GetType().BaseType == typeof(TestDecorator2));
                Assert.NotNull(service.DecoratedService.DecoratedService.GetType().BaseType == typeof(TestDecorator1));
            }

            Assert.NotNull(services[0].DecoratedService.DecoratedService.DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl1));
            Assert.NotNull(services[1].DecoratedService.DecoratedService.DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl2));
            Assert.NotNull(services[2].DecoratedService.DecoratedService.DecoratedService.GetType().BaseType == typeof(TestDecoratorImpl3));
        }

        [Fact]
        public void RegisteringDecoratorsWithoutConcreteThrowsFatalException() {
            var settings = CreateSettings();
            var blueprint = CreateBlueprint(
                    WithDependency<TestDecorator1>(),
                    WithDependency<TestDecorator2>(),
                    WithDependency<TestDecorator3>()
            );

            var factory = _container.Resolve<IShellContainerFactory>();

            Assert.Throws<SystemFatalException>(delegate {
                factory.CreateContainer(settings, blueprint);
            });
        }

    }
}