using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Framework.Caching;
using Framework.Environment.Extensions;
using Framework.Environment.Extensions.Attributes;
using Framework.Environment.Extensions.Folders;
using Framework.Environment.Extensions.Loaders;
using Framework.Environment.Extensions.Models;
using Framework.FileSystems.Dependencies;
using Framework.Tests.Environment.Extensions.ExtensionTypes;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.Environment.Extensions
{
     public class ExtensionLoaderCoordinatorTests {
        private IContainer _container;
        private IExtensionManager _manager;
        private StubFolders _folders;

        
        public void Init() {
            var builder = new ContainerBuilder();
            _folders = new StubFolders(DefaultExtensionTypes.Module);
            builder.RegisterInstance(_folders).As<IExtensionFolders>();
            builder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();

            _container = builder.Build();
            _manager = _container.Resolve<IExtensionManager>();
        }

        public class StubFolders : IExtensionFolders {
            private readonly string _extensionType;

            public StubFolders(string extensionType) {
                _extensionType = extensionType;
                Manifests = new Dictionary<string, string>();
            }

            public IDictionary<string, string> Manifests { get; set; }

            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                foreach (var e in Manifests) {
                    string name = e.Key;
                    yield return ExtensionHarvester.GetDescriptorForExtension("~/", name, _extensionType, Manifests[name]);
                }
            }
        }

        public class StubLoaders : IExtensionLoader {
            #region Implementation of IExtensionLoader

            public int Order {
                get { return 1; }
            }

            public string Name {
                get { return this.GetType().Name; }
            }

            public Assembly LoadReference(DependencyReferenceDescriptor reference) {
                throw new NotImplementedException();
            }

            public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
                throw new NotImplementedException();
            }

            public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
                throw new NotImplementedException();
            }

            public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
                throw new NotImplementedException();
            }

            public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
                return new ExtensionProbeEntry { Descriptor = descriptor, Loader = this };
            }

            public IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor extensionDescriptor) {
                throw new NotImplementedException();
            }

            public ExtensionEntry Load(ExtensionDescriptor descriptor) {
                return new ExtensionEntry { Descriptor = descriptor, ExportedTypes = new[] { typeof(Alpha), typeof(Beta), typeof(Phi) } };
            }

            public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
                throw new NotImplementedException();
            }

            public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
                throw new NotImplementedException();
            }

            public void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
                throw new NotImplementedException();
            }

            public void Monitor(ExtensionDescriptor extension, Action<IVolatileToken> monitor) {
            }

            public IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency) {
                throw new NotImplementedException();
            }

            public IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor dependency) {
                throw new NotImplementedException();
            }

            public bool LoaderIsSuitable(ExtensionDescriptor descriptor) {
                throw new NotImplementedException();
            }

            #endregion
        }

        private ExtensionManager CreateExtensionManager(StubFolders extensionFolder, StubLoaders extensionLoader) {
            return new ExtensionManager(new[] { extensionFolder }, new[] { extensionLoader }, new StubCacheManager(), new StubParallelCacheContext(), new StubAsyncTokenProvider());
        }

         public ExtensionLoaderCoordinatorTests()
         {
             Init();
         }
        [Fact]
        public void AvailableExtensionsShouldFollowCatalogLocations() {
            _folders.Manifests.Add("foo", "Name: Foo");
            _folders.Manifests.Add("bar", "Name: Bar");
            _folders.Manifests.Add("frap", "Name: Frap");
            _folders.Manifests.Add("quad", "Name: Quad");

            var available = _manager.AvailableExtensions();

            Assert.Equal(available.Count(), 4);
            Assert.Contains(available, t => t.Id == "foo");
        }

        [Fact]
        public void ExtensionDescriptorsShouldHaveNameAndVersion() {

            _folders.Manifests.Add("Sample", @"
Name: Sample Extension
Version: 2.x
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.Equal(descriptor.Id, "Sample");
            Assert.Equal(descriptor.Name, "Sample Extension");
            Assert.Equal(descriptor.Version, "2.x");
        }

        [Fact]
        public void ExtensionDescriptorsShouldBeParsedForMinimalModuleTxt() {

            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
SystemVersion: 1
Features:
    SuperWiki: 
        Description: My super wiki module for System.
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.Equal(descriptor.Id, "SuperWiki");
            Assert.Equal(descriptor.Version, "1.0.3");
            Assert.Equal(descriptor.SystemVersion, "1");
            Assert.Equal(descriptor.Features.Count(), 1);
            Assert.Equal(descriptor.Features.First().Id, "SuperWiki");
            Assert.Equal(descriptor.Features.First().Extension.Id, "SuperWiki");
            Assert.Equal(descriptor.Features.First().Description, "My super wiki module for System.");
        }

        [Fact]
        public void ExtensionDescriptorsShouldBeParsedForMinimalModuleTxtWithSimpleFormat() {

            _folders.Manifests.Add("SuperWiki", @"
Name: SuperWiki
Version: 1.0.3
SystemVersion: 1
Description: My super wiki module for System.
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.Equal(descriptor.Id, "SuperWiki");
            Assert.Equal(descriptor.Version, "1.0.3");
            Assert.Equal(descriptor.SystemVersion, "1");
            Assert.Equal(descriptor.Features.Count(), 1);
            Assert.Equal(descriptor.Features.First().Id, "SuperWiki");
            Assert.Equal(descriptor.Features.First().Extension.Id, "SuperWiki");
            Assert.Equal(descriptor.Features.First().Description, "My super wiki module for System.");
        }

        [Fact]
        public void ExtensionDescriptorsShouldBeParsedForCompleteModuleTxt() {

            _folders.Manifests.Add("MyCompany.AnotherWiki", @"
Name: AnotherWiki
Author: Coder Notaprogrammer
Website: http://anotherwiki.codeplex.com
Version: 1.2.3
SystemVersion: 1
Features:
    AnotherWiki: 
        Description: My super wiki module for System.
        Dependencies: Versioning, Search
        Category: Content types
    AnotherWiki Editor:
        Description: A rich editor for wiki contents.
        Dependencies: TinyMce, AnotherWiki
        Category: Input methods
    AnotherWiki DistributionList:
        Description: Sends e-mail alerts when wiki contents gets published.
        Dependencies: AnotherWiki, Email Subscriptions
        Category: Email
    AnotherWiki Captcha:
        Description: Kills spam. Or makes it zombie-like.
        Dependencies: AnotherWiki, reCaptcha
        Category: Spam
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.Equal(descriptor.Id, "MyCompany.AnotherWiki");
            Assert.Equal(descriptor.Name, "AnotherWiki");
            Assert.Equal(descriptor.Author, "Coder Notaprogrammer");
            Assert.Equal(descriptor.WebSite, "http://anotherwiki.codeplex.com");
            Assert.Equal(descriptor.Version, "1.2.3");
            Assert.Equal(descriptor.SystemVersion, "1");
            Assert.Equal(descriptor.Features.Count(), 5);
            foreach (var featureDescriptor in descriptor.Features) {
                switch (featureDescriptor.Id) {
                    case "AnotherWiki":
                        Assert.Same(featureDescriptor.Extension,  descriptor);
                        Assert.Equal(featureDescriptor.Description, "My super wiki module for System.");
                        Assert.Equal(featureDescriptor.Category, "Content types");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("Versioning"));
                        Assert.True(featureDescriptor.Dependencies.Contains("Search"));
                        break;
                    case "AnotherWiki Editor":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        Assert.Equal(featureDescriptor.Description, "A rich editor for wiki contents.");
                        Assert.Equal(featureDescriptor.Category, "Input methods");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("TinyMce"));
                        Assert.True(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        break;
                    case "AnotherWiki DistributionList":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        Assert.Equal(featureDescriptor.Description, "Sends e-mail alerts when wiki contents gets published.");
                        Assert.Equal(featureDescriptor.Category, "Email");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        Assert.True(featureDescriptor.Dependencies.Contains("Email Subscriptions"));
                        break;
                    case "AnotherWiki Captcha":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        Assert.Equal(featureDescriptor.Description, "Kills spam. Or makes it zombie-like.");
                        Assert.Equal(featureDescriptor.Category, "Spam");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        Assert.True(featureDescriptor.Dependencies.Contains("reCaptcha"));
                        break;
                    // default feature.
                    case "MyCompany.AnotherWiki":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        break;
                    default:
                        Assert.True(false,"Features not parsed correctly");
                        break;
                }
            }
        }

        [Fact]
        public void ExtensionDescriptorsShouldBeParsedForCompleteModuleTxtWithSimpleFormat() {

            _folders.Manifests.Add("AnotherWiki", @"
Name: AnotherWiki
Author: Coder Notaprogrammer
Website: http://anotherwiki.codeplex.com
Version: 1.2.3
SystemVersion: 1
Description: Module Description
FeatureDescription: My super wiki module for System.
Dependencies: Versioning, Search
Category: Content types
Features:
    AnotherWiki Editor:
        Description: A rich editor for wiki contents.
        Dependencies: TinyMce, AnotherWiki
        Category: Input methods
    AnotherWiki DistributionList:
        Description: Sends e-mail alerts when wiki contents gets published.
        Dependencies: AnotherWiki, Email Subscriptions
        Category: Email
    AnotherWiki Captcha:
        Description: Kills spam. Or makes it zombie-like.
        Dependencies: AnotherWiki, reCaptcha
        Category: Spam
");

            var descriptor = _manager.AvailableExtensions().Single();
            Assert.Equal(descriptor.Id, "AnotherWiki");
            Assert.Equal(descriptor.Name, "AnotherWiki");
            Assert.Equal(descriptor.Author, "Coder Notaprogrammer");
            Assert.Equal(descriptor.WebSite, "http://anotherwiki.codeplex.com");
            Assert.Equal(descriptor.Version, "1.2.3");
            Assert.Equal(descriptor.SystemVersion, "1");
            Assert.Equal(descriptor.Description, "Module Description");
            Assert.Equal(descriptor.Features.Count(), 4);
            foreach (var featureDescriptor in descriptor.Features) {
                switch (featureDescriptor.Id) {
                    case "AnotherWiki":
                        Assert.Same(featureDescriptor.Extension,  descriptor);
                        Assert.Equal(featureDescriptor.Description, "My super wiki module for System.");
                        Assert.Equal(featureDescriptor.Category, "Content types");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("Versioning"));
                        Assert.True(featureDescriptor.Dependencies.Contains("Search"));
                        break;
                    case "AnotherWiki Editor":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        Assert.Equal(featureDescriptor.Description, "A rich editor for wiki contents.");
                        Assert.Equal(featureDescriptor.Category, "Input methods");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("TinyMce"));
                        Assert.True(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        break;
                    case "AnotherWiki DistributionList":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        Assert.Equal(featureDescriptor.Description, "Sends e-mail alerts when wiki contents gets published.");
                        Assert.Equal(featureDescriptor.Category, "Email");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        Assert.True(featureDescriptor.Dependencies.Contains("Email Subscriptions"));
                        break;
                    case "AnotherWiki Captcha":
                        Assert.Same(featureDescriptor.Extension, descriptor);
                        Assert.Equal(featureDescriptor.Description, "Kills spam. Or makes it zombie-like.");
                        Assert.Equal(featureDescriptor.Category, "Spam");
                        Assert.Equal(featureDescriptor.Dependencies.Count(), 2);
                        Assert.True(featureDescriptor.Dependencies.Contains("AnotherWiki"));
                        Assert.True(featureDescriptor.Dependencies.Contains("reCaptcha"));
                        break;
                    default:
                        Assert.True(false,"Features not parsed correctly");
                        break;
                }
            }
        }

        [Fact]
        public void ExtensionManagerShouldLoadFeatures() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Module);

            extensionFolder.Manifests.Add("TestModule", @"
Name: TestModule
Version: 1.0.3
SystemVersion: 1
Features:
    TestModule: 
        Description: My test module for System.
    TestFeature:
        Description: Contains the Phi type.
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features);

            var features = extensionManager.LoadFeatures(testFeature);
            var types = features.SelectMany(x => x.ExportedTypes);

            Assert.NotEqual(types.Count(), 0);
        }

        [Fact]
        public void ExtensionManagerFeaturesContainNonAbstractClasses() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Module);

            extensionFolder.Manifests.Add("TestModule", @"
Name: TestModule
Version: 1.0.3
SystemVersion: 1
Features:
    TestModule: 
        Description: My test module for System.
    TestFeature:
        Description: Contains the Phi type.
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features);

            var features = extensionManager.LoadFeatures(testFeature);
            var types = features.SelectMany(x => x.ExportedTypes);

            foreach (var type in types) {
                Assert.True(type.IsClass);
                Assert.True(!type.IsAbstract);
            }
        }

        [Fact]
        public void ExtensionManagerTestFeatureAttribute() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Module);

            extensionFolder.Manifests.Add("TestModule", @"
Name: TestModule
Version: 1.0.3
SystemVersion: 1
Features:
    TestModule: 
        Description: My test module for System.
    TestFeature:
        Description: Contains the Phi type.
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features)
                .Single(x => x.Id == "TestFeature");

            foreach (var feature in extensionManager.LoadFeatures(new[] { testFeature })) {
                foreach (var type in feature.ExportedTypes) {
                    foreach (SystemFeatureAttribute featureAttribute in type.GetCustomAttributes(typeof(SystemFeatureAttribute), false)) {
                        Assert.Equal(featureAttribute.FeatureName,"TestFeature");
                    }
                }
            }
        }

        [Fact]
        public void ExtensionManagerLoadFeatureReturnsTypesFromSpecificFeaturesWithFeatureAttribute() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Module);

            extensionFolder.Manifests.Add("TestModule", @"
Name: TestModule
Version: 1.0.3
SystemVersion: 1
Features:
    TestModule: 
        Description: My test module for System.
    TestFeature:
        Description: Contains the Phi type.
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var testFeature = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features)
                .Single(x => x.Id == "TestFeature");

            foreach (var feature in extensionManager.LoadFeatures(new[] { testFeature })) {
                foreach (var type in feature.ExportedTypes) {
                    Assert.True(type == typeof(Phi));
                }
            }
        }

        [Fact]
        public void ExtensionManagerLoadFeatureDoesNotReturnTypesFromNonMatchingFeatures() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Module);

            extensionFolder.Manifests.Add("TestModule", @"
Name: TestModule
Version: 1.0.3
SystemVersion: 1
Features:
    TestModule: 
        Description: My test module for System.
    TestFeature:
        Description: Contains the Phi type.
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var testModule = extensionManager.AvailableExtensions()
                .SelectMany(x => x.Features)
                .Single(x => x.Id == "TestModule");

            foreach (var feature in extensionManager.LoadFeatures(new[] { testModule })) {
                foreach (var type in feature.ExportedTypes) {
                    Assert.True(type != typeof(Phi));
                    Assert.True((type == typeof(Alpha) || (type == typeof(Beta))));
                }
            }
        }

        [Fact]
        public void ModuleNameIsIntroducedAsFeatureImplicitly() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Module);

            extensionFolder.Manifests.Add("Minimalistic", @"
Name: Minimalistic
Version: 1.0.3
SystemVersion: 1
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var minimalisticModule = extensionManager.AvailableExtensions().Single(x => x.Id == "Minimalistic");

            Assert.Equal(minimalisticModule.Features.Count(), 1);
            Assert.Equal(minimalisticModule.Features.Single().Id, "Minimalistic");
        }


        [Fact]
        public void ThemeNameIsIntroducedAsFeatureImplicitly() {
            var extensionLoader = new StubLoaders();
            var extensionFolder = new StubFolders(DefaultExtensionTypes.Theme);

            extensionFolder.Manifests.Add("Minimalistic", @"
Name: Minimalistic
Version: 1.0.3
SystemVersion: 1
");

            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
            var minimalisticModule = extensionManager.AvailableExtensions().Single(x => x.Id == "Minimalistic");

            Assert.Equal(minimalisticModule.Features.Count(), 1);
            Assert.Equal(minimalisticModule.Features.Single().Id, "Minimalistic");
        }
    }
}