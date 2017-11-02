using System.Web.Hosting;

namespace Framework.FileSystems.VirtualPath
{
    public interface ICustomVirtualPathProvider {
        VirtualPathProvider Instance { get; }
    }
}