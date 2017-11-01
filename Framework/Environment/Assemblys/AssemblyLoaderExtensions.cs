namespace Framework.Environment.Assemblys
{
    public static class AssemblyLoaderExtensions {
        public static string ExtractAssemblyShortName(this IAssemblyLoader assemblyLoader, string fullName) {
            return ExtractAssemblyShortName(fullName);
        }

        public static string ExtractAssemblyShortName(string fullName) {
            int index = fullName.IndexOf(',');
            if (index < 0)
                return fullName;
            return fullName.Substring(0, index);
        }
    }
}