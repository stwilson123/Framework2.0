using Framework.Events;

namespace Framework.Environment
{public interface ISystemShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}