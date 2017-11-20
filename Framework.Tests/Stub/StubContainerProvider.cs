using Autofac;
using Autofac.Integration.Web;

namespace Framework.Tests.Stub
{
    public class StubContainerProvider : IContainerProvider {
        public StubContainerProvider(IContainer applicationContainer, ILifetimeScope requestContainer) {
            ApplicationContainer = applicationContainer;
            RequestLifetime = requestContainer;
        }

        public void EndRequestLifetime() {
            RequestLifetime.Dispose();
        }

        public ILifetimeScope ApplicationContainer { get; set; }

        public ILifetimeScope RequestLifetime { get; set; }
    }
}