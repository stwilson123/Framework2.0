using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Framework.Environment;
using Framework.Logging;
using Framework.Tests.Environment;
using Xunit;

namespace Framework.Tests.Logging
{
    public class LoggingModuleTests
    {
         [Fact]
        public void LoggingModuleWillSetLoggerProperty() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();
            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.Equal(thing.Logger, NullLogger.Instance);
        }

        [Fact]
        public void LoggerFactoryIsPassedTheTypeOfTheContainingInstance() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CollectionOrderModule());
            builder.RegisterModule(new LoggingModule());
            builder.RegisterType<Thing>();
            var stubFactory = new StubFactory();
            builder.RegisterInstance(stubFactory).As<ILoggerFactory>();

            var container = builder.Build();
            var thing = container.Resolve<Thing>();
            Assert.NotEqual(thing.Logger, null);
            Assert.Equal(stubFactory.CalledType, typeof(Thing));
        }

        internal class CollectionOrderModule : IModule {
            public void Configure(IComponentRegistry componentRegistry) {
                componentRegistry.Registered += (sender, registered) => {
                    // only bother watching enumerable resolves
                    var limitType = registered.ComponentRegistration.Activator.LimitType;
                    if (typeof(IEnumerable).IsAssignableFrom(limitType)) {
                        registered.ComponentRegistration.Activated += (sender2, activated) => {
                            // Autofac's IEnumerable feature returns an Array
                            if (activated.Instance is Array) {
                                // Orchard needs FIFO, not FILO, component order
                                Array.Reverse((Array)activated.Instance);
                            }
                        };
                    }
                };
            }
        }
        public class StubFactory : ILoggerFactory {
            public ILogger CreateLogger(Type type) {
                CalledType = type;
                return StubLogger.Instance;
            }

            public Type CalledType { get; set; }
        }
        public class StubLogger : ILogger
        {
            private static readonly ILogger _instance = new StubLogger();
            public static ILogger Instance {
                get { return _instance; }
            }
            public bool IsEnabled(LogLevel level)
            {
                throw new NotImplementedException();
            }

            public void Log(LogLevel level, Exception exception, string format, params object[] args)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void DefaultLoggerConfigurationUsesCastleLoggerFactoryOverTraceSource() {
//            var builder = new ContainerBuilder();
//            builder.RegisterModule(new LoggingModule());
//            builder.RegisterType<Thing>();
//            builder.RegisterType<StubHostEnvironment>().As<IHostEnvironment>();
//            var container = builder.Build();
////
//            log4net.Config.BasicConfigurator.Configure(new MemoryAppender());
//
//            var thing = container.Resolve<Thing>();
//            Assert.That(thing.Logger, Is.Not.Null);
//
//            MemoryAppender.Messages.Clear();
//            thing.Logger.Error("-boom{0}-", 42);
//            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("-boom42-"));
//
//            MemoryAppender.Messages.Clear();
//            thing.Logger.Warning(new ApplicationException("problem"), "crash");
//            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("problem"));
//            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("crash"));
//            Assert.That(MemoryAppender.Messages, Has.Some.StringContaining("ApplicationException"));
        }
    }

    public class Thing {
        public ILogger Logger { get; set; }
    }
//
//    public class MemoryAppender : IAppender {
//        static MemoryAppender() {
//            Messages = new List<string>();
//        }
//
//        public static List<string> Messages { get; set; }
//
//        public void DoAppend(LoggingEvent loggingEvent) {
//            if (loggingEvent.ExceptionObject != null) {
//                lock (Messages) Messages.Add(string.Format("{0} {1} {2}",
//                    loggingEvent.ExceptionObject.GetType().Name,
//                    loggingEvent.ExceptionObject.Message,
//                    loggingEvent.RenderedMessage));
//            } else lock (Messages) Messages.Add(loggingEvent.RenderedMessage); 
//        }
//
//        public void Close() { }
//        public string Name { get; set; }
//    }
   
}