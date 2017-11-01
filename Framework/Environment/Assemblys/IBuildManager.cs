using System.Collections.Generic;

namespace Framework.Environment.Assemblys
{
    public interface IBuildManager : IDependency {
        IEnumerable<System.Reflection.Assembly> GetReferencedAssemblies();
        bool HasReferencedAssembly(string name);
        System.Reflection.Assembly GetReferencedAssembly(string name);
        System.Reflection.Assembly GetCompiledAssembly(string virtualPath);
    }
}