using Autofac;
using Framework.Environment.Configuration;
using Framework.Environment.ShellBuilder.Models;

namespace Framework.Environment.ShellBuilder
{
    public interface IShellContainerFactory {
        ILifetimeScope CreateContainer(ShellSettings settings, ShellBlueprint blueprint);
    }
}