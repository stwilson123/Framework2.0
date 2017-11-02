using Framework.Events;

namespace Framework.Environment.Configuration
{
    public interface IShellSettingsManagerEventHandler : IEventHandler {
        void Saved(ShellSettings settings);
    }
}