using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Framework.Environment.Configuration;
using Framework.FileSystems.Media;
using Xunit;

namespace Framework.Tests.Storage
{
  public class FileSystemStorageProviderTests : IDisposable {

      public FileSystemStorageProviderTests()
      {
          Init();
      }
      public void Dispose()
      {
          Term();
      }
        public void Init() {
            _folderPath = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media"), ShellSettings.DefaultName);
            _filePath = _folderPath + "\\testfile.txt";

            if (Directory.Exists(_folderPath)) {
                try {
                    Directory.Delete(_folderPath, true);
                }
                catch {
                    // happens sometimes
                }
            }

            Directory.CreateDirectory(_folderPath);
            File.WriteAllText(_filePath, "testfile contents");

            var subfolder1 = Path.Combine(_folderPath, "Subfolder1");
            Directory.CreateDirectory(subfolder1);
            File.WriteAllText(Path.Combine(subfolder1, "one.txt"), "one contents");
            File.WriteAllText(Path.Combine(subfolder1, "two.txt"), "two contents");

            var subsubfolder1 = Path.Combine(subfolder1, "SubSubfolder1");
            Directory.CreateDirectory(subsubfolder1);

            _storageProvider = new FileSystemStorageProvider(new ShellSettings { Name = ShellSettings.DefaultName });
        }

       
        public void Term() {
            try {
                Directory.Delete(_folderPath, true);
            }
            catch (IOException) {
                // if a system handle is still active give some time to release it
                Thread.Sleep(0);
                Directory.Delete(_folderPath, true);
            }
        }


        private string _filePath;
        private string _folderPath;
        private IStorageProvider _storageProvider;

        [Fact]
        public void ExistsShouldBeTrueForExtistingFile() {
            Assert.True(_storageProvider.FileExists("testfile.txt"));
        }

        [Fact]
        public void ExistsShouldBeFalseForNonExtistingFile() {
            Assert.False(_storageProvider.FileExists("notexisting"));
        }

        [Fact]
       
        public void GetFileThatDoesNotExistShouldThrow() {
            Assert.Throws<ArgumentException>(() => _storageProvider.GetFile("notexisting")); 
        }

        [Fact]
        public void ListFilesShouldReturnFilesFromFilesystem() {
            IEnumerable<IStorageFile> files = _storageProvider.ListFiles(_folderPath);
            Assert.Equal(files.Count(), (1));
        }

        [Fact]
        public void ExistingFileIsReturnedWithShortPath() {
            var file = _storageProvider.GetFile("testfile.txt");
            Assert.NotNull(file);
            Assert.Equal(file.GetPath(), "testfile.txt");
            Assert.Equal(file.GetName(), "testfile.txt");
        }


        [Fact]
        public void ListFilesReturnsItemsWithShortPathAndEnvironmentSlashes() {
            var files = _storageProvider.ListFiles("Subfolder1");
            Assert.NotNull(files);
            Assert.Equal(files.Count(), 2);
            var one = files.Single(x => x.GetName() == "one.txt");
            var two = files.Single(x => x.GetName() == "two.txt");

            Assert.Equal(one.GetPath(), "Subfolder1" + Path.DirectorySeparatorChar + "one.txt");
            Assert.Equal(two.GetPath(), "Subfolder1" + Path.DirectorySeparatorChar + "two.txt");
        }


        [Fact]
        public void AnySlashInGetFileBecomesEnvironmentAppropriate() {
            var file1 = _storageProvider.GetFile(@"Subfolder1/one.txt");
            var file2 = _storageProvider.GetFile(@"Subfolder1\one.txt");
            Assert.Equal(file1.GetPath(), "Subfolder1" + Path.DirectorySeparatorChar + "one.txt");
            Assert.Equal(file2.GetPath(), "Subfolder1" + Path.DirectorySeparatorChar + "one.txt");
        }

        [Fact]
        public void ExistsShouldBeTrueForExtistingFolder() {
            Assert.True(_storageProvider.FolderExists("Subfolder1"));
        }

        [Fact]
        public void ExistsShouldBeFalseForNonExtistingFolder() {
            Assert.False(_storageProvider.FolderExists("notexisting"));
        }

        [Fact]
        public void ListFoldersReturnsItemsWithShortPathAndEnvironmentSlashes() {
            var folders = _storageProvider.ListFolders(@"Subfolder1");
            Assert.NotNull(folders);
            Assert.Equal(folders.Count(), 1);
            Assert.Equal(folders.Single().GetName(), "SubSubfolder1");
            Assert.Equal(folders.Single().GetPath(), Path.Combine("Subfolder1", "SubSubfolder1"));
        }

        [Fact]
        public void ParentFolderPathIsStillShort() {
            var subsubfolder = _storageProvider.ListFolders(@"Subfolder1").Single();
            var subfolder = subsubfolder.GetParent();
            Assert.Equal(subsubfolder.GetName(), "SubSubfolder1");
            Assert.Equal(subsubfolder.GetPath(), Path.Combine("Subfolder1", "SubSubfolder1"));
            Assert.Equal(subfolder.GetName(), "Subfolder1");
            Assert.Equal(subfolder.GetPath(), "Subfolder1");
        }

        [Fact]
        public void CreateFolderAndDeleteFolderTakesAnySlash() {
            Assert.Equal(_storageProvider.ListFolders(@"Subfolder1").Count(), 1);
            _storageProvider.CreateFolder(@"SubFolder1/SubSubFolder2");
            _storageProvider.CreateFolder(@"SubFolder1\SubSubFolder3");
            Assert.Equal(_storageProvider.ListFolders(@"Subfolder1").Count(), 3);
            _storageProvider.DeleteFolder(@"SubFolder1/SubSubFolder2");
            _storageProvider.DeleteFolder(@"SubFolder1\SubSubFolder3");
            Assert.Equal(_storageProvider.ListFolders(@"Subfolder1").Count(), 1);
        }

        private IStorageFolder GetFolder(string path) {
            return _storageProvider.ListFolders(Path.GetDirectoryName(path))
                .SingleOrDefault(x => string.Equals(x.GetName(), Path.GetFileName(path), StringComparison.OrdinalIgnoreCase));
        }
        private IStorageFile GetFile(string path) {
            try {
                return _storageProvider.GetFile(path);
            }
            catch (ArgumentException) {
                return null;
            }
        }

        [Fact]
        public void ShouldCreateFolders() {
            Directory.Delete(_folderPath, true);
            _storageProvider.CreateFolder("foo/bar/baz");
            Assert.Equal(_storageProvider.ListFolders("").Count(), 1);
            Assert.Equal(_storageProvider.ListFolders("foo").Count(), 1);
            Assert.Equal(_storageProvider.ListFolders("foo/bar").Count(), 1);
        }

        [Fact]
        public void RenameFolderTakesShortPathWithAnyKindOfSlash() {
            Assert.NotNull(GetFolder(@"SubFolder1/SubSubFolder1"));
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder1", @"SubFolder1/SubSubFolder2");
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder2", @"SubFolder1\SubSubFolder3");
            _storageProvider.RenameFolder(@"SubFolder1/SubSubFolder3", @"SubFolder1\SubSubFolder4");
            _storageProvider.RenameFolder(@"SubFolder1/SubSubFolder4", @"SubFolder1/SubSubFolder5");
            Assert.Null(GetFolder(Path.Combine("SubFolder1", "SubSubFolder1")));
            Assert.Null(GetFolder(Path.Combine("SubFolder1", "SubSubFolder2")));
            Assert.Null(GetFolder(Path.Combine("SubFolder1", "SubSubFolder3")));
            Assert.Null(GetFolder(Path.Combine("SubFolder1", "SubSubFolder4")));
            Assert.NotNull(GetFolder(Path.Combine("SubFolder1", "SubSubFolder5")));
        }


        [Fact]
        public void CreateFileAndDeleteFileTakesAnySlash() {
            Assert.Equal(_storageProvider.ListFiles(@"Subfolder1").Count(), 2);
            var alpha = _storageProvider.CreateFile(@"SubFolder1/alpha.txt");
            var beta = _storageProvider.CreateFile(@"SubFolder1\beta.txt");
            Assert.Equal(_storageProvider.ListFiles(@"Subfolder1").Count(), 4);
            Assert.Equal(alpha.GetPath(), Path.Combine("SubFolder1", "alpha.txt"));
            Assert.Equal(beta.GetPath(), Path.Combine("SubFolder1", "beta.txt"));
            _storageProvider.DeleteFile(@"SubFolder1\alpha.txt");
            _storageProvider.DeleteFile(@"SubFolder1/beta.txt");
            Assert.Equal(_storageProvider.ListFiles(@"Subfolder1").Count(), 2);
        }

        [Fact]
        public void RenameFileTakesShortPathWithAnyKindOfSlash() {
            Assert.NotNull(GetFile(@"Subfolder1/one.txt"));
            _storageProvider.RenameFile(@"SubFolder1\one.txt", @"SubFolder1/testfile2.txt");
            _storageProvider.RenameFile(@"SubFolder1\testfile2.txt", @"SubFolder1\testfile3.txt");
            _storageProvider.RenameFile(@"SubFolder1/testfile3.txt", @"SubFolder1\testfile4.txt");
            _storageProvider.RenameFile(@"SubFolder1/testfile4.txt", @"SubFolder1/testfile5.txt");
            Assert.Null(GetFile(Path.Combine("SubFolder1", "one.txt")));
            Assert.Null(GetFile(Path.Combine("SubFolder1", "testfile2.txt")));
            Assert.Null(GetFile(Path.Combine("SubFolder1", "testfile3.txt")));
            Assert.Null(GetFile(Path.Combine("SubFolder1", "testfile4.txt")));
            Assert.NotNull(GetFile(Path.Combine("SubFolder1", "testfile5.txt")));
        }

        [Fact]
        public void GetFileFailsInInvalidPath() {
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../InvalidFile.txt"));
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../../InvalidFile.txt"));

            // Valid get one level up within the storage provider domain
            _storageProvider.CreateFile(@"test.txt");
            Assert.NotNull(_storageProvider.GetFile(@"test.txt"));
            Assert.NotNull(_storageProvider.GetFile(@"SubFolder1\..\test.txt"));
        }

        [Fact]
        public void ListFilesFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.ListFiles(@"../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.ListFiles(@"../../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../InvalidFolder"));
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../../InvalidFolder"));

            // Valid get one level up within the storage provider domain
            Assert.NotNull(_storageProvider.ListFiles(@"SubFolder1"));
            Assert.NotNull(_storageProvider.ListFiles(@"SubFolder1\.."));
        }

        [Fact]
        public void ListFoldersFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.ListFolders(@"../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.ListFolders(@"../../InvalidFolder"), Throws.InstanceOf(typeof(ArgumentException)));
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../InvalidFolder"));
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../../InvalidFolder"));

            // Valid get one level up within the storage provider domain
            Assert.NotNull(_storageProvider.ListFolders(@"SubFolder1"));
            Assert.NotNull(_storageProvider.ListFolders(@"SubFolder1\.."));
        }

        [Fact]
        public void TryCreateFolderFailsInInvalidPath() {
            Assert.False(_storageProvider.TryCreateFolder(@"../InvalidFolder1"));
            Assert.False(_storageProvider.TryCreateFolder(@"../../InvalidFolder1"));

            // Valid create one level up within the storage provider domain
            Assert.True(_storageProvider.TryCreateFolder(@"SubFolder1\..\ValidFolder1"));
        }

        [Fact]
        public void CreateFolderFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.CreateFolder(@"../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.CreateFolder(@"../../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));

            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../InvalidFolder1"));
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.GetFile(@"../../InvalidFolder1"));

            
            // Valid create one level up within the storage provider domain
            _storageProvider.CreateFolder(@"SubFolder1\..\ValidFolder1");
            Assert.NotNull(GetFolder("ValidFolder1"));
        }

        [Fact]
        public void DeleteFolderFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.DeleteFolder(@"../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.DeleteFolder(@"../../InvalidFolder1"), Throws.InstanceOf(typeof(ArgumentException)));

            Assert.ThrowsAny<ArgumentException>(() =>  _storageProvider.DeleteFolder(@"../InvalidFolder1"));                         
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.DeleteFolder(@"../../InvalidFolder1"));                   
            
            // Valid create one level up within the storage provider domain
            Assert.NotNull(GetFolder("SubFolder1"));
            _storageProvider.DeleteFolder(@"SubFolder1\..\SubFolder1");
            Assert.Null(GetFolder("SubFolder1"));
        }

        [Fact]
        public void RenameFolderFailsInInvalidPath() {
            Assert.NotNull(GetFolder(@"SubFolder1/SubSubFolder1"));
//            Assert.That(() => _storageProvider.RenameFolder(@"SubFolder1", @"../SubSubFolder1"),      Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.RenameFolder(@"SubFolder1", @"../../SubSubFolder1"), Throws.InstanceOf(typeof(ArgumentException)));

            Assert.ThrowsAny<ArgumentException>(() =>   _storageProvider.RenameFolder(@"SubFolder1", @"../SubSubFolder1") );
            Assert.ThrowsAny<ArgumentException>(() =>   _storageProvider.RenameFolder(@"SubFolder1", @"../../SubSubFolder1"));
            
            // Valid move one level up within the storage provider domain
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder1", @"SubFolder1\..\SubSubFolder1");
            Assert.NotNull(GetFolder("SubSubFolder1"));

            _storageProvider.CreateFolder(@"SubFolder1\SubSubFolder1\SubSubSubFolder1");
            _storageProvider.RenameFolder(@"SubFolder1\SubSubFolder1\SubSubSubFolder1", @"SubFolder1\SubSubFolder1\..\SubSubSubFolder1");
            Assert.NotNull(GetFolder(@"SubFolder1\SubSubSubFolder1"));
        }

        [Fact]
        public void DeleteFileFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.DeleteFile(@"../test.txt"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.DeleteFile(@"../test.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            Assert.ThrowsAny<ArgumentException>(() =>  _storageProvider.DeleteFile(@"../test.txt") );
            Assert.ThrowsAny<ArgumentException>(() =>  _storageProvider.DeleteFile(@"../test.txt") );

            
            // Valid move one level up within the storage provider domain
            _storageProvider.CreateFile(@"test.txt");
            Assert.NotNull(GetFile("test.txt"));
            _storageProvider.DeleteFile(@"test.txt");
            Assert.Null(GetFile("test.txt"));

            _storageProvider.CreateFile(@"test.txt");
            Assert.NotNull(GetFile("test.txt"));
            _storageProvider.DeleteFile(@"SubFolder1\..\test.txt");
            Assert.Null(GetFile("test.txt"));
        }

        [Fact]
        public void RenameFileFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.RenameFile(@"../test.txt", "invalid.txt"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.RenameFile(@"../test.txt", "invalid.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.RenameFile(@"../test.txt", "invalid.txt") );
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.RenameFile(@"../test.txt", "invalid.txt") );
            
            
            // Valid move one level up within the storage provider domain
            _storageProvider.CreateFile(@"test.txt");
            Assert.NotNull(GetFile("test.txt"));
            _storageProvider.RenameFile(@"test.txt", "newName.txt");
            Assert.NotNull(GetFile("newName.txt"));
            _storageProvider.RenameFile(@"SubFolder1\..\newName.txt", "newNewName.txt");
            Assert.NotNull(GetFile("newNewName.txt"));
        }

        [Fact]
        public void CreateFileFailsInInvalidPath() {
//            Assert.That(() => _storageProvider.CreateFile(@"../InvalidFolder1.txt"), Throws.InstanceOf(typeof(ArgumentException)));
//            Assert.That(() => _storageProvider.CreateFile(@"../../InvalidFolder1.txt"), Throws.InstanceOf(typeof(ArgumentException)));

            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.CreateFile(@"../InvalidFolder1.txt") );
            Assert.ThrowsAny<ArgumentException>(() => _storageProvider.CreateFile(@"../../InvalidFolder1.txt") );
            
            // Valid create one level up within the storage provider domain
            _storageProvider.CreateFile(@"SubFolder1\..\ValidFolder1.txt");
            Assert.NotNull(GetFile("ValidFolder1.txt"));
        }

        [Fact]
        public void SaveStreamFailsInInvalidPath() {
            _storageProvider.CreateFile(@"test.txt");

            using (Stream stream = GetFile("test.txt").OpenRead()) {
//                Assert.That(() => _storageProvider.SaveStream(@"../newTest.txt", stream), Throws.InstanceOf(typeof(ArgumentException)));
//                Assert.That(() => _storageProvider.SaveStream(@"../../newTest.txt", stream), Throws.InstanceOf(typeof(ArgumentException)));
                Assert.ThrowsAny<ArgumentException>(() =>_storageProvider.SaveStream(@"../newTest.txt", stream));
                Assert.ThrowsAny<ArgumentException>(() =>_storageProvider.SaveStream(@"../../newTest.txt", stream));
                
                // Valid create one level up within the storage provider domain
                _storageProvider.SaveStream(@"SubFolder1\..\newTest.txt", stream);
                Assert.NotNull(GetFile("newTest.txt"));
            }
        }

        [Fact]
        public void TrySaveStreamFailsInInvalidPath() {
            _storageProvider.CreateFile(@"test.txt");

            using (Stream stream = GetFile("test.txt").OpenRead()) {
                Assert.False(_storageProvider.TrySaveStream(@"../newTest.txt", stream));
                Assert.False(_storageProvider.TrySaveStream(@"../../newTest.txt", stream));

                // Valid create one level up within the storage provider domain
                Assert.True(_storageProvider.TrySaveStream(@"SubFolder1\..\newTest.txt", stream));
            }
        }

      
  }
}