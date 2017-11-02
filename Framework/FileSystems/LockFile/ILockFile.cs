using System;

namespace Framework.FileSystems.LockFile
{
    public interface ILockFile : IDisposable {
        void Release();
    }
}