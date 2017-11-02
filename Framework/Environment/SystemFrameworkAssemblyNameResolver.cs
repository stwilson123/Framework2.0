using System;
using System.Linq;
using System.Reflection;
using Framework.Caching;
using Framework.Environment.Assemblys;

namespace Framework.Environment
{
    public class SystemFrameworkAssemblyNameResolver: IAssemblyNameResolver {
        private readonly ICacheManager _cacheManager;

        public SystemFrameworkAssemblyNameResolver(ICacheManager cacheManager) {
            _cacheManager = cacheManager;
        }

        public int Order { get { return 20; } }

        public string Resolve(string shortName) {
            // A few common .net framework assemblies are referenced by the Orchard.Framework assembly.
            // Look into those to see if we can find the assembly we are looking for.
            var orchardFrameworkReferences = _cacheManager.Get(typeof(IAssemblyLoader), true, ctx =>
                ctx.Key.Assembly
                    .GetReferencedAssemblies()
                    .GroupBy(n => AssemblyLoaderExtensions.ExtractAssemblyShortName(n.FullName), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(n => n.Key /*short assembly name*/, g => g.OrderBy(n => n.Version).Last() /* highest assembly version */, StringComparer.OrdinalIgnoreCase));

            AssemblyName assemblyName;
            if (orchardFrameworkReferences.TryGetValue(shortName, out assemblyName)) {
                return assemblyName.FullName;
            }

            return null;
        }
    }
}