using System.Collections.Generic;
using Framework.Environment.Extensions.Models;

namespace Framework.Environment.Extensions.Folders
{
    public interface IExtensionHarvester {
        IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional);
    }
}