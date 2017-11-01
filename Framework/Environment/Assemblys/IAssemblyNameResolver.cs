namespace Framework.Environment.Assemblys
{
    public interface IAssemblyNameResolver {
        int Order { get; }

        /// <summary>
        /// Resolve a short assembly name to a full name
        /// </summary>
        string Resolve(string shortName);
    }
}