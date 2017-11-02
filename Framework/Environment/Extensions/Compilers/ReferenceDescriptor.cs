namespace Framework.Environment.Extensions.Compilers
{
    public class ReferenceDescriptor {
        public string SimpleName { get; set; }
        public string FullName { get; set; }
        public string Path { get; set; }
        public ReferenceType ReferenceType { get; set; }
    }
}