using System;
using Autofac;

namespace Framework.Tests
{
    public class ContainerTestBase : IDisposable {

        protected IContainer _container;

        public ContainerTestBase()
        {
            Init();
        }
        
        public virtual void Init() {
            var builder = new ContainerBuilder();
            Register(builder);
            _container = builder.Build();
            Resolve(_container);
        }

        
        public void Cleanup() {
            if (_container != null)
                _container.Dispose();
        }

#if false
// technically more accurate, and doesn't work
        [SetUp]
        public virtual void Init() {
            var hostBuilder = new ContainerBuilder();
            var hostContainer = hostBuilder.Build();
            var shellContainer = hostContainer.BeginLifetimeScope("shell", shellBuilder => Register(shellBuilder));
            var workContainer = shellContainer.BeginLifetimeScope("work");

            _container = workContainer;
            Resolve(_container);
        }
#endif

        protected virtual void Register(ContainerBuilder builder) { }
        protected virtual void Resolve(ILifetimeScope container) { }
        public void Dispose()
        {
            Cleanup();
        }
    }
}