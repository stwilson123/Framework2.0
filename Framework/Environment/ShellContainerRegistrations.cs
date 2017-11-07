using System;
using Autofac;

namespace Framework.Environment
{
    public class ShellContainerRegistrations : IShellContainerRegistrations {
        public ShellContainerRegistrations() {
            Registrations = builder => { return; };
        }

        public Action<ContainerBuilder> Registrations { get; private set; }
    }
}