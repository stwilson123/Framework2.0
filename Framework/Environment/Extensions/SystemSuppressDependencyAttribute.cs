using System;

namespace Framework.Environment.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SystemSuppressDependencyAttribute : Attribute {
        public SystemSuppressDependencyAttribute(string fullName) {
            FullName = fullName;
        }

        public string FullName { get; set; }
    }
}