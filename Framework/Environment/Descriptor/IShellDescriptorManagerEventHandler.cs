using Framework.Environment.Descriptor.Models;
using Framework.Events;

namespace Framework.Environment.Descriptor
{
    public interface IShellDescriptorManagerEventHandler : IEventHandler {
        void Changed(ShellDescriptor descriptor, string tenant);
    }
}