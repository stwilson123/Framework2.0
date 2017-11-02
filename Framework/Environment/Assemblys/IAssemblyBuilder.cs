using System.CodeDom;
using System.Reflection;

namespace Framework.Environment.Assemblys
{
    public interface IAssemblyBuilder {
        void AddCodeCompileUnit(CodeCompileUnit compileUnit);
        void AddAssemblyReference(Assembly assembly);
    }
}