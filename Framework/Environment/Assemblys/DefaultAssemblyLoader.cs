using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Exceptions;
using Framework.Logging;

namespace Framework.Environment.Assemblys
{
   public class DefaultAssemblyLoader : IAssemblyLoader {
        private readonly IEnumerable<IAssemblyNameResolver> _assemblyNameResolvers;
        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        public DefaultAssemblyLoader(IEnumerable<IAssemblyNameResolver> assemblyNameResolvers) {
            _assemblyNameResolvers = assemblyNameResolvers.OrderBy(l => l.Order);
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public System.Reflection.Assembly Load(string assemblyName) {
            try {
                return _loadedAssemblies.GetOrAdd(this.ExtractAssemblyShortName(assemblyName), shortName => LoadWorker(shortName, assemblyName));
            }
            catch (Exception ex) {
                if (ex.IsFatal()) {
                    throw;
                }
                Logger.Error(ex, "Error loading assembly '{0}'", assemblyName);
                return null;
            }
        }

        private Assembly LoadWorker(string shortName, string fullName) {
            Assembly result;

            // Try loading with full name first (if there is a full name)
            if (fullName != shortName) {
                result = TryAssemblyLoad(fullName);
                if (result != null)
                    return result;
            }

            // Try loading with short name
            result = TryAssemblyLoad(shortName);
            if (result != null)
                return result;

            // Try resolving the short name to a full name
            var resolvedName = _assemblyNameResolvers.Select(r => r.Resolve(shortName)).FirstOrDefault(f => f != null);
            if (resolvedName != null) {
                return Assembly.Load(resolvedName);
            }

            // Try again so that we get the exception this time
            return Assembly.Load(fullName);
        }

        private static System.Reflection.Assembly TryAssemblyLoad(string name) {
            try {
                return Assembly.Load(name);
            }
            catch {
                return null;
            }
        }
    }
}