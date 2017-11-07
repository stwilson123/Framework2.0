using Framework.Caching;
using Framework.FileSystems.VirtualPath;

namespace Framework.Tests.Stub
{
    public class StubVirtualPathMonitor : IVirtualPathMonitor {
        public class Token : IVolatileToken {
            public bool IsCurrent { get; set; }
        }
        public IVolatileToken WhenPathChanges(string virtualPath) {
            return new Token();
        }
    }
}