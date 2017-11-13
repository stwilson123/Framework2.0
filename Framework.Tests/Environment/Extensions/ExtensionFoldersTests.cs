using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Framework.Environment.Extensions;
using Framework.Environment.Extensions.Folders;
using Framework.Tests.Stub;
using Moq;
using Xunit;

namespace Framework.Tests.Environment.Extensions
{

    public class ExtensionFoldersTests : IDisposable
    {
        private const string DataPrefix = "Framework.Tests.Environment.Extensions.FoldersData.";
        private string _tempFolderName;


        public void Init()
        {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().Assembly;
            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.StartsWith(DataPrefix))
                {
                    var text = "";
                    using (var stream = assembly.GetManifestResourceStream(name))
                    {
                        using (var reader = new StreamReader(stream))
                            text = reader.ReadToEnd();

                    }

                    var relativePath = name
                        .Substring(DataPrefix.Length)
                        .Replace(".txt", ":txt")
                        .Replace('.', Path.DirectorySeparatorChar)
                        .Replace(":txt", ".txt");

                    var targetPath = Path.Combine(_tempFolderName, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    using (var stream = new FileStream(targetPath, FileMode.Create))
                    {
                        using (var writer = new StreamWriter(stream))
                        {
                            writer.Write(text);
                        }
                    }
                }
            }
        }


        public void Term()
        {
            Directory.Delete(_tempFolderName, true);
        }

        public ExtensionFoldersTests()
        {
            Init();
        }

        public void Dispose()
        {
            Term();
        }

        [Fact]
        public void IdsFromFoldersWithModuleTxtShouldBeListed()
        {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(),
                new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] {_tempFolderName}, harvester);
            var ids = folders.AvailableExtensions().Select(d => d.Id);
            Assert.Equal(ids.Count(), 5);
            Assert.Contains(ids, t => t == "Sample1"); // Sample1 - obviously
            Assert.Contains(ids, t => t == "Sample3"); // Sample3
            Assert.Contains(ids, t => t == "Sample4"); // Sample4
            Assert.Contains(ids, t => t == "Sample6"); // Sample6
            Assert.Contains(ids, t => t == "Sample7"); // Sample7
        }

        [Fact]
        public void ModuleTxtShouldBeParsedAndReturnedAsYamlDocument()
        {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(),
                new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] {_tempFolderName}, harvester);
            var sample1 = folders.AvailableExtensions().Single(d => d.Id == "Sample1");
            Assert.NotEmpty(sample1.Id);
            Assert.Equal(sample1.Author, "Bertrand Le Roy"); // Sample1
        }

        [Fact]
        public void NamesFromFoldersWithModuleTxtShouldFallBackToIdIfNotGiven()
        {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(),
                new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] {_tempFolderName}, harvester);
            var names = folders.AvailableExtensions().Select(d => d.Name);
            Assert.Equal(names.Count(), 5);
            Assert.Contains(names, t => t == "Le plug-in français"); // Sample1
            Assert.Contains(names, t => t == "This is another test.txt"); // Sample3
            Assert.Contains(names, t => t == "Sample4"); // Sample4
            Assert.Contains(names, t => t == "SampleSix"); // Sample6
            Assert.Contains(names, t => t == "Sample7"); // Sample7
        }

        [Fact]
        public void PathsFromFoldersWithModuleTxtShouldFallBackAppropriatelyIfNotGiven()
        {
            var harvester = new ExtensionHarvester(new StubCacheManager(), new StubWebSiteFolder(),
                new Mock<ICriticalErrorProvider>().Object);
            IExtensionFolders folders = new ModuleFolders(new[] {_tempFolderName}, harvester);
            var paths = folders.AvailableExtensions().Select(d => d.Path);
            Assert.Equal(paths.Count(), 5);
            Assert.Contains(paths, t => t == "Sample1"); // Sample1 - Id, Name invalid URL segment
            Assert.Contains(paths, t => t == "Sample3"); // Sample3 - Id, Name invalid URL segment
            Assert.Contains(paths, t => t == "ThisIs.Sample4"); // Sample4 - Path
            Assert.Contains(paths, t => t == "SampleSix"); // Sample6 - Name, no Path
            Assert.Contains(paths, t => t == "Sample7"); // Sample7 - Id, no Name or Path



        }
    }
}