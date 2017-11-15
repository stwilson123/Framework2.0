using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Framework.Environment.Configuration;
using Framework.Environment.Extensions;
using Framework.Environment.Extensions.Attributes;
using Framework.Environment.Extensions.Models;
using Framework.Environment.ShellBuilder;
using Framework.Tests.Environment.Utility;
using Framework.Tests.Records;
using Framework.Tests.Utility.Container;
using Moq;
using Xunit;

namespace Framework.Tests.Environment
{
     public class DefaultCompositionStrategyTests {

        private IContainer _container;

        private IEnumerable<ExtensionDescriptor> _extensionDescriptors;
        private IDictionary<string, IEnumerable<Type>> _featureTypes;

        
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>();
            builder.RegisterAutoMocking(MockBehavior.Strict);
            _container = builder.Build();

            _extensionDescriptors = Enumerable.Empty<ExtensionDescriptor>();
            _featureTypes = new Dictionary<string, IEnumerable<Type>>();

            _container.Mock<IExtensionManager>()
                .Setup(x => x.AvailableExtensions())
                .Returns(() => _extensionDescriptors);

            _container.Mock<IExtensionManager>()
                .Setup(x => x.AvailableFeatures())
                .Returns(() => _extensionDescriptors.SelectMany(ext => ext.Features));

            _container.Mock<IExtensionManager>()
                .Setup(x => x.LoadFeatures(It.IsAny<IEnumerable<FeatureDescriptor>>()))
                .Returns((IEnumerable<FeatureDescriptor> x) => StubLoadFeatures(x));
        }

        private IEnumerable<Feature> StubLoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors) {
            return featureDescriptors.Select(featureDescriptor => new Feature {
                Descriptor = featureDescriptor,
                ExportedTypes = _featureTypes[featureDescriptor.Id]
            });
        }

        private static ShellSettings BuildDefaultSettings() {
            return new ShellSettings { Name = ShellSettings.DefaultName };
        }

         public DefaultCompositionStrategyTests()
         {
             Init();
             
         }
         

        [Fact]
        public void BlueprintIsNotNull() {
            var descriptor = Build.ShellDescriptor();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            Assert.NotNull(blueprint);
        }


        [Fact]
        public void DependenciesFromFeatureArePutIntoBlueprint() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo", "Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar"),
            };

            _featureTypes["Foo"] = new[] { typeof(FooService1) };
            _featureTypes["Bar"] = new[] { typeof(BarService1) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            Assert.NotNull(blueprint);
            Assert.Equal(blueprint.Dependencies.Count(), 2);

            var foo = blueprint.Dependencies.SingleOrDefault(t => t.Type == typeof(FooService1));
            Assert.NotNull(foo);
            Assert.Equal(foo.Feature.Descriptor.Id, "Foo");

            var bar = blueprint.Dependencies.SingleOrDefault(t => t.Type == typeof(BarService1));
            Assert.NotNull(bar);
            Assert.Equal(bar.Feature.Descriptor.Id, "Bar");
        }

        public interface IFooService : IDependency {
        }

        public class FooService1 : IFooService {
        }

        public interface IBarService : IDependency {
        }

        public class BarService1 : IBarService {
        }


        [Fact]
        public void DependenciesAreGivenParameters() {
            var descriptor = Build.ShellDescriptor()
                .WithFeatures("Foo")
                .WithParameter<FooService1>("one", "two")
                .WithParameter<FooService1>("three", "four");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
            };

            _featureTypes["Foo"] = new[] { typeof(FooService1) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var foo = blueprint.Dependencies.SingleOrDefault(t => t.Type == typeof(FooService1));
            Assert.NotNull(foo);
            Assert.Equal(foo.Parameters.Count(), 2);
            Assert.Equal(foo.Parameters.Single(x => x.Name == "one").Value, "two");
            Assert.Equal(foo.Parameters.Single(x => x.Name == "three").Value, "four");
        }

        [Fact]
        public void ModulesArePutIntoBlueprint() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo", "Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Foo"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar"),
            };

            _featureTypes["Foo"] = new[] { typeof(AlphaModule) };
            _featureTypes["Bar"] = new[] { typeof(BetaModule) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var alpha = blueprint.Dependencies.Single(x => x.Type == typeof(AlphaModule));
            var beta = blueprint.Dependencies.Single(x => x.Type == typeof(BetaModule));

            Assert.Equal(alpha.Feature.Descriptor.Id, "Foo");
            Assert.Equal(beta.Feature.Descriptor.Id, "Bar");
        }

        public class AlphaModule : Module {
        }

        public class BetaModule : IModule {
            public void Configure(IComponentRegistry componentRegistry) {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void ControllersArePutIntoBlueprintWithAreaAndControllerName() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo Plus", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("MyCompany.Foo", "Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(GammaController) };
            _featureTypes["Bar"] = Enumerable.Empty<Type>();
            _featureTypes["Bar Minus"] = new[] { typeof(DeltaController), typeof(EpsilonController) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var gamma = blueprint.Controllers.Single(x => x.Type == typeof(GammaController));
            var delta = blueprint.Controllers.Single(x => x.Type == typeof(DeltaController));
            var epsilon = blueprint.Controllers.Single(x => x.Type == typeof(EpsilonController));

            Assert.Equal(gamma.Feature.Descriptor.Id, "Foo Plus");
            Assert.Equal(gamma.AreaName, "MyCompany.Foo");
            Assert.Equal(gamma.ControllerName, "Gamma");

            Assert.Equal(delta.Feature.Descriptor.Id, "Bar Minus");
            Assert.Equal(delta.AreaName, "Bar");
            Assert.Equal(delta.ControllerName, "Delta");

            Assert.Equal(epsilon.Feature.Descriptor.Id, "Bar Minus");
            Assert.Equal(epsilon.AreaName, "Bar");
            Assert.Equal(epsilon.ControllerName, "Epsilon");
        }


        public class GammaController : Controller {
        }

        public class DeltaController : ControllerBase {
            protected override void ExecuteCore() {
                throw new NotImplementedException();
            }
        }

        public class EpsilonController : IController {
            public void Execute(RequestContext requestContext) {
                throw new NotImplementedException();
            }
        }


        [Fact]
        public void RecordsArePutIntoBlueprintWithTableName() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo Plus", "Bar", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("MyCompany.Foo", "Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(FooRecord) };
            _featureTypes["Bar"] = new[] { typeof(BarRecord) };
            _featureTypes["Bar Minus"] = Enumerable.Empty<Type>();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            var foo = blueprint.Records.Single(x => x.Type == typeof(FooRecord));
            var bar = blueprint.Records.Single(x => x.Type == typeof(BarRecord));

            Assert.Equal(foo.Feature.Descriptor.Id, "Foo Plus");
            Assert.Equal(foo.TableName, "MyCompany_Foo_FooRecord");

            Assert.Equal(bar.Feature.Descriptor.Id, "Bar");
            Assert.Equal(bar.TableName, "Bar_BarRecord");
        }

        [Fact]
        public void CoreRecordsAreAddedAutomatically() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Orchard.Framework");

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

//            var ct = blueprint.Records.Single(x => x.Type == typeof(ContentTypeRecord));
//            var ci = blueprint.Records.Single(x => x.Type == typeof(ContentItemRecord));
//            var civ = blueprint.Records.Single(x => x.Type == typeof(ContentItemVersionRecord));
//
//            Assert.Equal(ct.Feature.Descriptor.Id, "Orchard.Framework");
//            Assert.Equal(ct.TableName, "Orchard_Framework_ContentTypeRecord");
//
//            Assert.Equal(ci.Feature.Descriptor.Id, "Orchard.Framework");
//            Assert.Equal(ci.TableName, "Orchard_Framework_ContentItemRecord");
//
//            Assert.Equal(civ.Feature.Descriptor.Id, "Orchard.Framework");
//            Assert.Equal(civ.TableName, "Orchard_Framework_ContentItemVersionRecord");
        }

        [Fact]
        public void DataPrefixChangesTableName() {
            var settings = BuildDefaultSettings();
            settings.DataTablePrefix = "Yadda";
            var descriptor = Build.ShellDescriptor().WithFeatures("Foo Plus", "Bar", "Bar Minus");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("MyCompany.Foo", "Foo").WithFeatures("Foo", "Foo Plus"),
                Build.ExtensionDescriptor("Bar").WithFeatures("Bar", "Bar Minus"),
            };

            _featureTypes["Foo"] = Enumerable.Empty<Type>();
            _featureTypes["Foo Plus"] = new[] { typeof(FooRecord) };
            _featureTypes["Bar"] = new[] { typeof(BarRecord) };
            _featureTypes["Bar Minus"] = Enumerable.Empty<Type>();

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(settings, descriptor);

            var foo = blueprint.Records.Single(x => x.Type == typeof(FooRecord));
            var bar = blueprint.Records.Single(x => x.Type == typeof(BarRecord));

            Assert.Equal(foo.Feature.Descriptor.Id, "Foo Plus");
            Assert.Equal(foo.TableName, "Yadda_MyCompany_Foo_FooRecord");

            Assert.Equal(bar.Feature.Descriptor.Id, "Bar");
            Assert.Equal(bar.TableName, "Yadda_Bar_BarRecord");
        }

        [Fact]
        public void FeatureReplacement() {
            var descriptor = Build.ShellDescriptor().WithFeatures("Bar");

            _extensionDescriptors = new[] {
                Build.ExtensionDescriptor("Foo").WithFeatures("Bar"),
            };

            _featureTypes["Bar"] = new[] { typeof(ReplacedStubType), typeof(StubType), typeof(ReplacedStubNestedType), typeof(StubNestedType) };

            var compositionStrategy = _container.Resolve<ICompositionStrategy>();
            var blueprint = compositionStrategy.Compose(BuildDefaultSettings(), descriptor);

            Assert.Equal(blueprint.Dependencies.Count(), 2);
            Assert.NotNull(blueprint.Dependencies.FirstOrDefault(dependency => dependency.Type.Equals(typeof(StubType))));
            Assert.NotNull(blueprint.Dependencies.FirstOrDefault(dependency => dependency.Type.Equals(typeof(StubNestedType))));
        }

        [SystemSuppressDependency("ystem.Tests.Environment.DefaultCompositionStrategyTests+ReplacedStubNestedType")]
        internal class StubNestedType : IDependency {}

        internal class ReplacedStubNestedType : IDependency {}
    }

    [SystemSuppressDependency("ystem.Tests.Environment.ReplacedStubType")]
    internal class StubType : IDependency { }

    internal class ReplacedStubType : IDependency { }
}