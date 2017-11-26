using System;
using System.Collections.Generic;
using System.Web;
using Framework.Environment.Extensions.Models;

namespace Framework.UI.Resources
{
    public interface IResourceManifest {
        ResourceDefinition DefineResource(string resourceType, string resourceName);
        string Name { get; }
        string BasePath { get; }
        IDictionary<string, ResourceDefinition> GetResources(string resourceType);
    }
}