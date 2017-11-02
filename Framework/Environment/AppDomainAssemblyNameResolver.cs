using System;
using System.Linq;
using Framework.Environment.Assemblys;

namespace Framework.Environment
{
    public class AppDomainAssemblyNameResolver : IAssemblyNameResolver {
        public int Order { get { return 10; } }

        public string Resolve(string shortName) {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => StringComparer.OrdinalIgnoreCase.Equals(shortName, AssemblyLoaderExtensions.ExtractAssemblyShortName(a.FullName)))
                .Select(a => a.FullName)
                .SingleOrDefault();
        }
    }
}