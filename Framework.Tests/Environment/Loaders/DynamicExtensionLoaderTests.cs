using System.Collections.Generic;
using System.Linq;
using Autofac;
using Framework.Caching;
using Framework.Environment;
using Framework.Environment.Assemblys;
using Framework.Environment.Configuration;
using Framework.Environment.Extensions.Compilers;
using Framework.Environment.Extensions.Loaders;
using Framework.FileSystems.AppData;
using Framework.FileSystems.Dependencies;
using Framework.FileSystems.VirtualPath;
using Framework.Services;
using Framework.Tests.Stub;
using Moq;
using Xunit;

namespace Framework.Tests.Environment.Loaders
{
    public class DynamicExtensionLoaderTests {
        private IContainer _container;
        private Mock<IProjectFileParser> _mockedStubProjectFileParser;
        private Mock<IDependenciesFolder> _mockedDependenciesFolder;

       
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DynamicExtensionLoaderAccessor>().As<DynamicExtensionLoaderAccessor>();

            builder.RegisterType<DefaultBuildManager>().As<IBuildManager>();
            builder.RegisterType<DefaultAssemblyProbingFolder>().As<IAssemblyProbingFolder>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();
            builder.RegisterType<StubVirtualPathMonitor>().As<IVirtualPathMonitor>();
            builder.RegisterType<StubVirtualPathProvider>().As<IVirtualPathProvider>();
            builder.RegisterType<DefaultAssemblyLoader>().As<IAssemblyLoader>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();

            builder.RegisterInstance(new ExtensionLocations());

            _mockedStubProjectFileParser = new Mock<IProjectFileParser>();
            builder.RegisterInstance(_mockedStubProjectFileParser.Object).As<IProjectFileParser>();
            builder.RegisterInstance(new StubFileSystem(new StubClock())).As<StubFileSystem>();

            _mockedDependenciesFolder = new Mock<IDependenciesFolder>();
            builder.RegisterInstance(_mockedDependenciesFolder.Object).As<IDependenciesFolder>();

            _container = builder.Build();
        }

        public DynamicExtensionLoaderTests()
        {
            Init();
        }
        [Fact]
        public void GetDependenciesContainsNoDuplicatesTest() {
            const string pathPrefix = "~/modules/foo";
            const string projectName = "orchard.a.csproj";
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";

            var vpp = _container.Resolve<IVirtualPathProvider>();
            var projectPath = vpp.Combine(pathPrefix, projectName);
//            using (vpp.CreateText(projectPath)) { }

            // Create duplicate source files (invalid situation in reality but easy enough to test)
            _mockedStubProjectFileParser.Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.IsAny<string>())).Returns(
                new ProjectFileDescriptor { SourceFilenames = new[] { fileName1, fileName2, fileName1 } }); // duplicate file

            var extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            var dependencies = extensionLoader.GetDependenciesAccessor(projectPath);

            Assert.True(dependencies.Count() == 3, "3 results should mean no duplicates");
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Equals(projectPath)));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Equals(vpp.Combine(pathPrefix, fileName1))));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Equals(vpp.Combine(pathPrefix, fileName2))));
        }

        [Fact]
        public void GetDependenciesContainsNoDuplicatesEvenIfMultipleProjectsTest() {
            const string path1Prefix = "~/modules/foo";
            const string path2Prefix = "~/modules/bar";
            const string path3Prefix = "~/modules/blah";
            const string project1Name = "orchard.a.csproj";
            const string project2Name = "orchard.b.csproj";
            const string project3Name = "orchard.c.csproj";
            const string fileName1 = "a.cs";
            const string fileName2 = "b.cs";
            const string commonFileName = "c.cs";

            var vpp = _container.Resolve<IVirtualPathProvider>();

            var project1Path = vpp.Combine(path1Prefix, project1Name);
            using (vpp.CreateText(project1Path)) { }

            var project2Path = vpp.Combine(path2Prefix, project2Name);
            using (vpp.CreateText(project2Path)) { }

            var project3Path = vpp.Combine(path3Prefix, project3Name);
            using (vpp.CreateText(project3Path)) { }


            // Project a reference b and c which share a file in common

            // Result for project a
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => virtualPath == project1Path)))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] {fileName1, fileName2},
                        References = new[] {
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = project2Path,
                                FullName = project2Path,
                                Path = "..\\bar\\" + project2Name
                            },
                            new ReferenceDescriptor {
                                ReferenceType = ReferenceType.Project,
                                SimpleName = project3Path,
                                FullName = project3Path,
                                Path = "..\\blah\\" + project3Name
                            }
                        }
                    });

            // Result for project b and c
            _mockedStubProjectFileParser
                .Setup(stubProjectFileParser => stubProjectFileParser.Parse(It.Is<string>(virtualPath => (virtualPath == project2Path || virtualPath == project3Path))))
                .Returns(
                    new ProjectFileDescriptor {
                        SourceFilenames = new[] { commonFileName }
                    });

            var extensionLoader = _container.Resolve<DynamicExtensionLoaderAccessor>();
            var dependencies = extensionLoader.GetDependenciesAccessor(project1Path);

            Assert.True(dependencies.Count() == 7, "7 results should mean no duplicates");

            // Project files
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(project1Path)));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(project2Path)));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(project3Path)));

            // Individual source files
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path1Prefix, fileName1))));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path1Prefix, fileName2))));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path2Prefix, commonFileName))));
            Assert.NotNull(dependencies.FirstOrDefault(dep => dep.Contains(vpp.Combine(path3Prefix, commonFileName))));
        }

        internal class DynamicExtensionLoaderAccessor : DynamicExtensionLoader {
            public DynamicExtensionLoaderAccessor(
                IBuildManager buildManager,
                IVirtualPathProvider virtualPathProvider,
                IVirtualPathMonitor virtualPathMonitor,
                IHostEnvironment hostEnvironment,
                IAssemblyProbingFolder assemblyProbingFolder,
                IDependenciesFolder dependenciesFolder,
                IProjectFileParser projectFileParser,
                ExtensionLocations extensionLocations)
                : base(buildManager, virtualPathProvider, virtualPathMonitor, hostEnvironment, assemblyProbingFolder, dependenciesFolder, projectFileParser, extensionLocations) {}

            public IEnumerable<string> GetDependenciesAccessor(string projectPath) {
                return GetDependencies(projectPath);
            }
        }
    }
}