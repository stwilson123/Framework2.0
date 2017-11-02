using System.Collections.Generic;
using Framework.Caching;

namespace Framework.FileSystems.Dependencies
{
    public interface IDependenciesFolder : IVolatileProvider {
        DependencyDescriptor GetDescriptor(string moduleName);
        IEnumerable<DependencyDescriptor> LoadDescriptors();
        void StoreDescriptors(IEnumerable<DependencyDescriptor> dependencyDescriptors);
    }
}