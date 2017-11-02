using System.IO;

namespace Framework.Environment.Extensions.Compilers
{
    public interface IProjectFileParser {
        ProjectFileDescriptor Parse(string virtualPath);
        ProjectFileDescriptor Parse(Stream stream);
    }
}