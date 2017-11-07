using System.Collections.Generic;
using Framework.Environment.Extensions.Models;

namespace Framework.Environment.Extensions.Folders
{
    public interface IExtensionFolders {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}