using System;
using Framework.Caching;

namespace Framework.Environment
{
    public interface IHostLocalRestart {
        /// <summary>
        /// Monitor changes on the persistent storage.
        /// </summary>
        void Monitor(Action<IVolatileToken> monitor);
    }
}