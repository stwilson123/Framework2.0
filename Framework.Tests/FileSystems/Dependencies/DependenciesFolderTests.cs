using System;
using System.IO;
using System.Linq;
using Autofac;
using Framework.Caching;
using Framework.FileSystems.AppData;
using Framework.FileSystems.Dependencies;
using Framework.Services;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.FileSystems.Dependencies
{
    public class DependenciesFolderTests {
        public IContainer BuildContainer() {
            var builder = new ContainerBuilder();
            builder.RegisterType<StubClock>().As<IClock>().SingleInstance();
            builder.RegisterType<StubAppDataFolder>().As<IAppDataFolder>().SingleInstance();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>().SingleInstance();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterType<DefaultDependenciesFolder>().As<IDependenciesFolder>();
            return builder.Build();
        }

        [Fact]
        public void LoadDescriptorsShouldReturnEmptyList() {
            var dependenciesFolder = BuildContainer().Resolve<IDependenciesFolder>();

            var e = dependenciesFolder.LoadDescriptors();
            Assert.Empty(e);
        }

        [Fact]
        public void StoreDescriptorsShouldWork() {
            var dependenciesFolder = BuildContainer().Resolve<IDependenciesFolder>();

            var d = new DependencyDescriptor {
                Name = "name",
                LoaderName = "test",
                VirtualPath = "~/bin"
            };

            dependenciesFolder.StoreDescriptors(new[] { d });
            var e = dependenciesFolder.LoadDescriptors();
            Assert.NotNull(e);
            Assert.Equal(e.Count(), 1);
            Assert.Equal(e.First().Name, "name");
            Assert.Equal(e.First().LoaderName, "test");
            Assert.Equal(e.First().VirtualPath, "~/bin");
        }

        [Fact]
        public void StoreDescriptorsShouldNoOpIfNoChanges() {
            var container = BuildContainer();
            var clock = (StubClock)container.Resolve<IClock>();
            var appDataFolder = (StubAppDataFolder)container.Resolve<IAppDataFolder>();
            var dependenciesFolder = container.Resolve<IDependenciesFolder>();

            var d1 = new DependencyDescriptor {
                Name = "name1",
                LoaderName = "test1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                Name = "name2",
                LoaderName = "test2",
                VirtualPath = "~/bin2"
            };

            dependenciesFolder.StoreDescriptors(new[] { d1, d2 });
            var dateTime1 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.json"));
            clock.Advance(TimeSpan.FromMinutes(1));

            dependenciesFolder.StoreDescriptors(new[] { d2, d1 });
            var dateTime2 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.json"));
            Assert.Equal(dateTime1, dateTime2);
        }

        [Fact]
        public void StoreDescriptorsShouldStoreIfChanges() {
            var container = BuildContainer();
            var clock = (StubClock)container.Resolve<IClock>();
            var appDataFolder = (StubAppDataFolder)container.Resolve<IAppDataFolder>();
            var dependenciesFolder = container.Resolve<IDependenciesFolder>();

            var d1 = new DependencyDescriptor {
                Name = "name1",
                LoaderName = "test1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                Name = "name2",
                LoaderName = "test2",
                VirtualPath = "~/bin2"
            };

            dependenciesFolder.StoreDescriptors(new[] { d1, d2 });
            var dateTime1 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.json"));
            clock.Advance(TimeSpan.FromMinutes(1));

            d1.LoaderName = "bar";

            dependenciesFolder.StoreDescriptors(new[] { d2, d1 });
            var dateTime2 = appDataFolder.GetLastWriteTimeUtc(Path.Combine("Dependencies", "Dependencies.json"));
            Assert.Equal(dateTime1 + TimeSpan.FromMinutes(1), dateTime2);
        }

        [Fact]
        public void LoadDescriptorsShouldWorkAcrossInstances() {
            var container = BuildContainer();
            var clock = (StubClock)container.Resolve<IClock>();
            var appDataFolder = (StubAppDataFolder)container.Resolve<IAppDataFolder>();
            var dependenciesFolder = container.Resolve<IDependenciesFolder>();

            var d1 = new DependencyDescriptor {
                Name = "name1",
                LoaderName = "test1",
                VirtualPath = "~/bin1"
            };

            var d2 = new DependencyDescriptor {
                Name = "name2",
                LoaderName = "test2",
                VirtualPath = "~/bin2"
            };

            dependenciesFolder.StoreDescriptors(new[] { d1, d2 });

            // Create a new instance over the same appDataFolder
            var dependenciesFolder2 = container.Resolve<IDependenciesFolder>();
            Assert.NotSame(dependenciesFolder2, dependenciesFolder);

            // Ensure descriptors were persisted properly
            var result = dependenciesFolder2.LoadDescriptors();
            Assert.Equal(result.Count(),  2);
            Assert.Contains( result.Select(p => p.Name), t => t == "name1");
            Assert.Contains(result.Select(p => p.Name), t => t == "name2");
        }
    }
}