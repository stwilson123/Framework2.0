using System.CodeDom;
using System.Reflection;
using System.Web.Compilation;

namespace Framework.Environment.Assemblys
{
    public class AspNetAssemblyBuilder : IAssemblyBuilder {
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly BuildProvider _buildProvider;

        public AspNetAssemblyBuilder(AssemblyBuilder assemblyBuilder, BuildProvider buildProvider) {
            _assemblyBuilder = assemblyBuilder;
            _buildProvider = buildProvider;
        }

        public void AddCodeCompileUnit(CodeCompileUnit compileUnit) {
            _assemblyBuilder.AddCodeCompileUnit(_buildProvider, compileUnit);
        }

        public void AddAssemblyReference(Assembly assembly) {
            _assemblyBuilder.AddAssemblyReference(assembly);
        }
    }
}