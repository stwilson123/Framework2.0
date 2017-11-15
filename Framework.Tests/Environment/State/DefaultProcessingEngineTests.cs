using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Framework.Environment;
using Framework.Environment.Configuration;
using Framework.Environment.Descriptor.Models;
using Framework.Environment.ShellBuilder;
using Framework.Environment.State;
using Framework.Events;
using Framework.Mvc;
using Framework.Tests.Stub;
using Framework.Tests.Utility.Container;
using Moq;
using Xunit;

namespace Framework.Tests.Environment.State
{
    public class DefaultProcessingEngineTests : IDisposable {
        private IContainer _container;
        private ShellContext _shellContext;

        
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultProcessingEngine>().As<IProcessingEngine>();
            builder.RegisterModule(new WorkContextModule());
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterAutoMocking(MockBehavior.Loose);
            _container = builder.Build();

            _shellContext = new ShellContext {
                Descriptor = new ShellDescriptor(),
                Settings = new ShellSettings(),
                LifetimeScope = _container.BeginLifetimeScope(),
            };

            var httpContext = new StubHttpContext();

            _container.Mock<IShellContextFactory>()
                .Setup(x => x.CreateDescribedContext(_shellContext.Settings, _shellContext.Descriptor))
                .Returns(_shellContext);
            _container.Mock<IHttpContextAccessor>()
                .Setup(x=>x.Current())
                .Returns(httpContext);
        }

       
        public void CleanTasks() {
            // clear the previous values
            try {
                var engine = _container.Resolve<IProcessingEngine>();
                if (engine != null)
                    while (engine.AreTasksPending()) engine.ExecuteNextTask();
            }
            catch {
                
            }
        }

        public DefaultProcessingEngineTests()
        {
            Init();
        }
        [Fact]
        public void NoTasksPendingByDefault() {
            var engine = _container.Resolve<IProcessingEngine>();
            var pending = engine.AreTasksPending();
            Assert.False(pending);
        }

        [Fact]
        public void ExecuteTaskIsSafeToCallWhenItDoesNothing() {
            var engine = _container.Resolve<IProcessingEngine>();
            var pending1 = engine.AreTasksPending();
            engine.ExecuteNextTask();
            var pending2 = engine.AreTasksPending();
            Assert.False(pending1);
            Assert.False(pending2);
        }

        [Fact]
        public void CallingAddTaskReturnsResultIdentifierAndCausesPendingToBeTrue() {
            var engine = _container.Resolve<IProcessingEngine>();
            var pending1 = engine.AreTasksPending();
            var resultId = engine.AddTask(new ShellSettings { Name = ShellSettings.DefaultName }, null, null, null);
            var pending2 = engine.AreTasksPending();
            Assert.False(pending1);
            Assert.NotNull(resultId);
            Assert.NotEmpty(resultId);
            Assert.True(pending2);
        }

        [Fact]
        public void CallingExecuteCausesEventToFireAndPendingFlagToBeCleared() {
            _container.Mock<IEventBus>()
                .Setup(x => x.Notify(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(Enumerable.Empty<object>());

            var engine = _container.Resolve<IProcessingEngine>();
            var pending1 = engine.AreTasksPending();
            engine.AddTask(_shellContext.Settings, _shellContext.Descriptor, "foo", null);
            var pending2 = engine.AreTasksPending();
            engine.ExecuteNextTask();
            var pending3 = engine.AreTasksPending();
            Assert.False(pending1);
            Assert.True(pending2);
            Assert.False(pending3);

            _container.Mock<IEventBus>()
                .Verify(x => x.Notify("foo", null));
        }


        public void Dispose()
        {
            CleanTasks();
        }
    }
}