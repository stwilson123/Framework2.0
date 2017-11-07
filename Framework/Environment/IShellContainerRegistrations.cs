using System;
using Autofac;

namespace Framework.Environment
{
    public interface IShellContainerRegistrations {
        Action<ContainerBuilder> Registrations { get; }
    }
}