using Framework.Caching;

namespace Framework.FileSystems.VirtualPath
{
    /// <summary>
    /// Enable monitoring changes over virtual path
    /// </summary>
    public interface IVirtualPathMonitor : IVolatileProvider {
        IVolatileToken WhenPathChanges(string virtualPath);
    }
}