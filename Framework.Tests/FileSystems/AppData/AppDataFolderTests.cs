using System;
using System.IO;
using System.Linq;
using Framework.FileSystems.AppData;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.FileSystems.AppData
{
   public class AppDataFolderTests : IDisposable {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;

        public class StubAppDataFolderRoot : IAppDataFolderRoot {
            public string RootPath { get; set; }
            public string RootFolder { get; set; }
        }

        public static IAppDataFolder CreateAppDataFolder(string tempFolder) {
            var folderRoot = new StubAppDataFolderRoot {RootPath = "~/App_Data", RootFolder = tempFolder};
            var monitor = new StubVirtualPathMonitor();
            return new AppDataFolder(folderRoot, monitor);
        }

       public AppDataFolderTests()
       {

           Init();
       }
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha"));
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\beta.txt"), "beta-content");
            File.WriteAllText(Path.Combine(_tempFolder, "alpha\\gamma.txt"), "gamma-content");
            Directory.CreateDirectory(Path.Combine(_tempFolder, "alpha\\omega"));

            _appDataFolder = CreateAppDataFolder(_tempFolder);
        }

      
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

        [Fact]
        public void ListFilesShouldContainSubPathAndFileName() {
            var files = _appDataFolder.ListFiles("alpha");
            Assert.Equal(files.Count(), 2);
            Assert.Contains(files, t => t.Equals("alpha/beta.txt"));
            Assert.Contains(files, t => t.Equals("alpha/gamma.txt"));
        }

        [Fact]
        public void NonExistantFolderShouldListAsEmptyCollection() {
            var files = _appDataFolder.ListFiles("delta");
            Assert.Equal(files.Count(), 0);
        }

        [Fact]
        public void PhysicalPathAddsToBasePathAndDoesNotNeedToExist() {
            var physicalPath = _appDataFolder.MapPath("delta\\epsilon.txt");
            Assert.Contains(physicalPath, Path.Combine(_tempFolder, "delta\\epsilon.txt"));
        }

        [Fact]
        public void ListSubdirectoriesShouldContainFullSubpath() {
            var files = _appDataFolder.ListDirectories("alpha");
            Assert.Equal(files.Count(), 1);
            Assert.Contains(files, t => t.Equals("alpha/omega"));
        }

        [Fact]
        public void ListSubdirectoriesShouldWorkInRoot() {
            var files = _appDataFolder.ListDirectories("");
            Assert.Equal(files.Count(),  1);
            Assert.Contains(files, t => t.Equals("alpha"));
        }


        [Fact]
        public void NonExistantFolderShouldListDirectoriesAsEmptyCollection() {
            var files = _appDataFolder.ListDirectories("delta");
            Assert.Equal(files.Count(), 0);
        }

        [Fact]
        public void CreateFileWillCauseDirectoryToBeCreated() {
            Assert.Equal(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")), false);
            _appDataFolder.CreateFile("alpha\\omega\\foo\\bar.txt", "quux");
            Assert.Equal(Directory.Exists(Path.Combine(_tempFolder, "alpha\\omega\\foo")), true);
        }


        [Fact]
        public void FilesCanBeReadBack() {            
            _appDataFolder.CreateFile("alpha\\gamma\\foo\\bar.txt", @"
this is
a
test");
            var text = _appDataFolder.ReadFile("alpha\\gamma\\foo\\bar.txt");
            Assert.Equal(text, @"
this is
a
test");
        }

        [Fact]
        public void FileExistsReturnsFalseForNonExistingFile() {
            Assert.Equal(_appDataFolder.FileExists("notexisting"), false);
        }

        [Fact]
        public void FileExistsReturnsTrueForExistingFile() {
            _appDataFolder.CreateFile("alpha\\foo\\bar.txt", "");
            Assert.Equal(_appDataFolder.FileExists("alpha\\foo\\bar.txt"), true);
        }

       public void Dispose()
       {
           Term();
       }
   }
}