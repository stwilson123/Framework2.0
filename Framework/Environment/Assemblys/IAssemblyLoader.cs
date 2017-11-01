namespace Framework.Environment.Assemblys
{
    public interface IAssemblyLoader {
        System.Reflection.Assembly Load(string assemblyName);
    }
}