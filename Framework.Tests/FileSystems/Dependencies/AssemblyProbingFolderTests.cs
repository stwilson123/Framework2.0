using System;
using System.Linq;
using Framework.Environment.Assemblys;
using Framework.FileSystems.Dependencies;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.FileSystems.Dependencies
{
    public class AssemblyProbingFolderTests
    {

        [Fact]
        public void FolderShouldBeEmptyByDefault()
        {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder,
                new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.Equal(dependenciesFolder.AssemblyExists("foo"), false);
        }

        [Fact]
        public void LoadAssemblyShouldNotThrowIfAssemblyNotFound()
        {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder,
                new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.Equal(dependenciesFolder.LoadAssembly("foo"), null);
        }

        [Fact]
        public void GetAssemblyDateTimeUtcShouldThrowIfAssemblyNotFound()
        {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder,
                new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            Assert.ThrowsAny<Exception>(() =>
            {
                  dependenciesFolder.GetAssemblyDateTimeUtc("foo");
            });
        }

        [Fact]
        public void DeleteAssemblyShouldNotThrowIfAssemblyNotFound()
        {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);
            var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder,
                new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));

            dependenciesFolder.DeleteAssembly("foo");
        }

        [Fact]
        public void StoreAssemblyShouldCopyFile()
        {
            var clock = new StubClock();
            var appDataFolder = new StubAppDataFolder(clock);

            var assembly = GetType().Assembly;
            var name = assembly.GetName().Name;

            {
                var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder,
                    new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));
                dependenciesFolder.StoreAssembly(name, assembly.Location);
            }

            {
                var dependenciesFolder = new DefaultAssemblyProbingFolder(appDataFolder,
                    new DefaultAssemblyLoader(Enumerable.Empty<IAssemblyNameResolver>()));
                Assert.Equal(dependenciesFolder.AssemblyExists(name), true);
                Assert.Equal(dependenciesFolder.LoadAssembly(name), GetType().Assembly);
                {
                    //shouldn't throw Exception
                    dependenciesFolder.DeleteAssembly(name);
                }
              
                Assert.Equal(dependenciesFolder.LoadAssembly(name), null);
            }
        }
    }
}