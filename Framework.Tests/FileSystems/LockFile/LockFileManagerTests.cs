﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Framework.FileSystems.AppData;
using Framework.FileSystems.LockFile;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.FileSystems.LockFile
{
     public class LockFileManagerTests : IDisposable {
        private string _tempFolder;
        private IAppDataFolder _appDataFolder;
        private ILockFileManager _lockFileManager;
        private StubClock _clock;

        public class StubAppDataFolderRoot : IAppDataFolderRoot {
            public string RootPath { get; set; }
            public string RootFolder { get; set; }
        }

        public static IAppDataFolder CreateAppDataFolder(string tempFolder) {
            var folderRoot = new StubAppDataFolderRoot {RootPath = "~/App_Data", RootFolder = tempFolder};
            var monitor = new StubVirtualPathMonitor();
            return new AppDataFolder(folderRoot, monitor);
        }

   
        public void Init() {
            _tempFolder = Path.GetTempFileName();
            File.Delete(_tempFolder);
            _appDataFolder = CreateAppDataFolder(_tempFolder);

            _clock = new StubClock();
            _lockFileManager = new DefaultLockFileManager(_appDataFolder, _clock);
        }

       
        public void Term() {
            Directory.Delete(_tempFolder, true);
        }

         public LockFileManagerTests()
         {
             Init();
         }
        [Fact]
        public void LockShouldBeGrantedWhenDoesNotExist() {
            ILockFile lockFile = null;
            var granted = _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            Assert.True(granted);
            Assert.NotNull(lockFile);
            Assert.True(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 1);
        }

        [Fact]
        public void ExistingLockFileShouldPreventGrants() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);
            
            Assert.False(_lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile));
            Assert.True(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 1);
        }

        [Fact]
        public void ReleasingALockShouldAllowGranting() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            using (lockFile) {
                Assert.True(_lockFileManager.IsLocked("foo.txt.lock"));
                Assert.Equal(_appDataFolder.ListFiles("").Count(), 1);
            }

            Assert.False(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(),  0 );
        }

        [Fact]
        public void ReleasingAReleasedLockShouldWork() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);
            
            Assert.True(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(),  1);
            
            lockFile.Release();
            Assert.False(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 0);
            
            lockFile.Release();
            Assert.False(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 0);
        }

        [Fact]
        public void DisposingLockShouldReleaseIt() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            using (lockFile) {
                Assert.True(_lockFileManager.IsLocked("foo.txt.lock"));
                Assert.Equal(_appDataFolder.ListFiles("").Count(), 1);
            }

            Assert.False(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 0);
        }

        [Fact]
        public void ExpiredLockShouldBeAvailable() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            _clock.Advance(DefaultLockFileManager.Expiration);
            Assert.False(_lockFileManager.IsLocked("foo.txt.lock"));
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 1);
        }

        [Fact]
        public void ShouldGrantExpiredLock() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            _clock.Advance(DefaultLockFileManager.Expiration);
            var granted = _lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile);

            Assert.True(granted);
            Assert.Equal(_appDataFolder.ListFiles("").Count(), 1);
        }

        private static int _lockCount;
        private static readonly object _synLock = new object();

        [Fact]
        public void AcquiringLockShouldBeThreadSafe() {
            var threads = new List<Thread>();
            for(var i=0; i<10; i++) {
                var t = new Thread(PlayWithAcquire);
                t.Start();
                threads.Add(t);
            }

            threads.ForEach(t => t.Join());
            Assert.Equal(_lockCount, 0);
        }

        [Fact]
        public void IsLockedShouldBeThreadSafe() {
            var threads = new List<Thread>();
            for (var i = 0; i < 10; i++)
            {
                var t = new Thread(PlayWithIsLocked);
                t.Start();
                threads.Add(t);
            }

            threads.ForEach(t => t.Join());
            Assert.Equal(_lockCount, 0);
        }

        private void PlayWithAcquire() {
            var r = new Random(DateTime.Now.Millisecond); 
            ILockFile lockFile = null;

            // loop until the lock has been acquired
            for (;;) {
                if (!_lockFileManager.TryAcquireLock("foo.txt.lock", ref lockFile)) {
                    continue;
                }

                lock (_synLock) {
                    _lockCount++;
                    Assert.Equal(_lockCount, 1);
                }

                // keep the lock for a certain time
                Thread.Sleep(r.Next(200));
                lock (_synLock) {
                    _lockCount--;
                    Assert.Equal(_lockCount, 0);
                }

                lockFile.Release();
                return;
            }
        }

        private void PlayWithIsLocked() {
            var r = new Random(DateTime.Now.Millisecond); 
            ILockFile lockFile = null;
            const string path = "foo.txt.lock";

            // loop until the lock has been acquired
            for (;;) {
                if(_lockFileManager.IsLocked(path)) {
                    continue;
                }

                if (!_lockFileManager.TryAcquireLock(path, ref lockFile)) {
                    continue;
                }

                lock (_synLock) {
                    _lockCount++;
                    Assert.Equal(_lockCount, 1);
                }

                // keep the lock for a certain time
                Thread.Sleep(r.Next(200));
                lock (_synLock) {
                    _lockCount--;
                    Assert.Equal(_lockCount, 0);
                }

                lockFile.Release();
                return;
            }
        }

         public void Dispose()
         {
             Term();
         }
     }
}