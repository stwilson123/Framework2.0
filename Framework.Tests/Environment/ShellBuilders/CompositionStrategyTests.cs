using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor.Models;
using Framework.Environment.Extensions;
using Framework.Environment.Extensions.Models;
using Framework.Environment.ShellBuilder;
using Framework.Logging;
using Framework.Tests.Environment.TestDependencies;
using Framework.Utility.Extensions;
using Moq;
using Xunit;

namespace Framework.Tests.Environment.ShellBuilders
{
    public class CompositionStrategyTests : ContainerTestBase {
        private CompositionStrategy _compositionStrategy;
        private Mock<IExtensionManager> _extensionManager;
        private IEnumerable<ExtensionDescriptor> _availableExtensions;
        private IEnumerable<Feature> _installedFeatures;
        private Mock<ILogger> _loggerMock;

        public CompositionStrategyTests() : base()
        {
            
        }
        
        protected override void Register(ContainerBuilder builder) {
            _extensionManager = new Mock<IExtensionManager>();
            _loggerMock = new Mock<ILogger>();

            builder.RegisterType<CompositionStrategy>().AsSelf();
            builder.RegisterInstance(_extensionManager.Object);
            builder.RegisterInstance(_loggerMock.Object);
        }

        protected override void Resolve(ILifetimeScope container) {
            _compositionStrategy = container.Resolve<CompositionStrategy>();
            _compositionStrategy.Logger = container.Resolve<ILogger>();

            var alphaExtension = new ExtensionDescriptor {
                Id = "Alpha",
                Name = "Alpha",
                ExtensionType = "Module"
            };

            var alphaFeatureDescriptor = new FeatureDescriptor {
                Id = "Alpha",
                Name = "Alpha",
                Extension = alphaExtension
            };

            var betaFeatureDescriptor = new FeatureDescriptor {
                Id = "Beta",
                Name = "Beta",
                Extension = alphaExtension,
                Dependencies = new List<string> {
                    "Alpha"
                }
            };

            alphaExtension.Features = new List<FeatureDescriptor> {
                alphaFeatureDescriptor,
                betaFeatureDescriptor
            };

            _availableExtensions = new[] {
                alphaExtension
            };

            _installedFeatures = new List<Feature> {
                new Feature {
                    Descriptor = alphaFeatureDescriptor,
                    ExportedTypes = new List<Type> {
                        typeof(AlphaDependency)
                    }
                },
                new Feature {
                    Descriptor = betaFeatureDescriptor,
                    ExportedTypes = new List<Type> {
                        typeof(BetaDependency)
                    }
                }
            };

            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

            _extensionManager.Setup(x => x.AvailableExtensions()).Returns(() => _availableExtensions);

            _extensionManager.Setup(x => x.AvailableFeatures()).Returns(() =>
                _extensionManager.Object.AvailableExtensions()
                .SelectMany(ext => ext.Features)
                .ToReadOnlyCollection());

            _extensionManager.Setup(x => x.LoadFeatures(It.IsAny<IEnumerable<FeatureDescriptor>>())).Returns(() => _installedFeatures);
        }

        [Fact]
        public void ComposeReturnsBlueprintWithExpectedDependencies() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("Alpha", "Beta");
            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            Assert.Equal(shellBlueprint.Dependencies.Count(x => x.Type == typeof(AlphaDependency)),  1);
            Assert.Equal(shellBlueprint.Dependencies.Count(x => x.Type == typeof(BetaDependency)),  (1));
        }

        [Fact]
        public void ComposeReturnsBlueprintWithAutoEnabledDependencyFeatures() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("Beta"); // Beta has a dependency on Alpha, but is not enabled initially.
            var shellBlueprint = _compositionStrategy.Compose(shellSettings, shellDescriptor);

            Assert.Equal(shellBlueprint.Dependencies.Count(x => x.Type == typeof(AlphaDependency)), 1);
            Assert.Equal(shellDescriptor.Features.Count(x => x.Name == "Alpha"), 1);
        }

        [Fact]
        public void ComposeDoesNotThrowWhenFeatureStateRecordDoesNotExist() {
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("MyFeature");

            _compositionStrategy.Compose(shellSettings, shellDescriptor);
            _loggerMock.Verify(x => x.Log(LogLevel.Warning, null, It.IsAny<string>(), It.IsAny<object[]>()));
        }

        [Fact]
        public void ComposeThrowsWhenAutoEnabledDependencyDoesNotExist() {
            var myModule = _availableExtensions.First();

            myModule.Features = myModule.Features.Concat(new[] {
                new FeatureDescriptor {
                    Extension = myModule,
                    Name = "MyFeature",
                    Id = "MyFeature",
                    Dependencies = new[] { "NonExistingFeature" }
                }
            });
            
            var shellSettings = CreateShell();
            var shellDescriptor = CreateShellDescriptor("MyFeature");

            Assert.Throws<Framework.Exceptions.Exception.SystemException>(() => _compositionStrategy.Compose(shellSettings, shellDescriptor));
        }

        private ShellSettings CreateShell() {
            return new ShellSettings();
        }

        private ShellDescriptor CreateShellDescriptor(params string[] enabledFeatures) {
            var shellDescriptor = new ShellDescriptor {
                Features = enabledFeatures.Select(x => new ShellFeature {
                    Name = x
                })
            };

            return shellDescriptor;
        }
    }
}