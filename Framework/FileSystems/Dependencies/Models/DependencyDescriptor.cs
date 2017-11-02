using System.Collections.Generic;
using System.Linq;

namespace Framework.FileSystems.Dependencies
{
    public class DependencyDescriptor {
        public DependencyDescriptor() {
            References = Enumerable.Empty<DependencyReferenceDescriptor>();
        }
        public string Name { get; set; }
        public string LoaderName { get; set; }
        public string VirtualPath { get; set; }
        public IEnumerable<DependencyReferenceDescriptor> References { get; set; }
    }

}