using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Framework.Events;
using Framework.Exceptions;
using Xunit;

namespace Framework.Tests.Event
{
   public class EventTests {
        private IContainer _container;
        private IEventBus _eventBus;
        private StubEventHandler _eventHandler;

        public void Init() {
            _eventHandler = new StubEventHandler();

            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>();
            builder.RegisterType<StubExceptionPolicy>().As<IExceptionPolicy>();

            builder.RegisterType<StubEventHandler2>()
                .Named(typeof(ITestEventHandler).Name, typeof(IEventHandler))
                .Named(typeof(IEventHandler).Name, typeof(IEventHandler))
                .WithMetadata("Interfaces", typeof(StubEventHandler2).GetInterfaces().ToDictionary(i => i.Name));
            builder.RegisterInstance(_eventHandler)
                .Named(typeof(ITestEventHandler).Name, typeof(IEventHandler))
                .Named(typeof(IEventHandler).Name, typeof(IEventHandler))
                .WithMetadata("Interfaces", typeof(StubEventHandler).GetInterfaces().ToDictionary(i => i.Name));

            _container = builder.Build();
            _eventBus = _container.Resolve<IEventBus>();
        }

       public EventTests()
       {
           Init();
       }

        [Fact]
        public void EventsAreCorrectlyDispatchedToEventHandlers() {
            Assert.Equal(_eventHandler.Count, 0);
            _eventBus.Notify("ITestEventHandler.Increment", new Dictionary<string, object>());
            Assert.Equal(_eventHandler.Count, 1);
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToEventHandlers() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 5200;
            arguments["b"] = 2600;
            _eventBus.Notify("ITestEventHandler.Substract", arguments);
            Assert.Equal(_eventHandler.Result, 2600);
        }

        [Fact]
        public void EventParametersArePassedInCorrectOrderToEventHandlers() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 2600;
            arguments["b"] = 5200;
            _eventBus.Notify("ITestEventHandler.Substract", arguments);
            Assert.Equal(_eventHandler.Result, -2600);
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToMatchingMethod() {
            Assert.Null(_eventHandler.Summary);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = "a";
            arguments["b"] = "b";
            arguments["c"] = "c";
            _eventBus.Notify("ITestEventHandler.Concat", arguments);
            Assert.Equal(_eventHandler.Summary, "abc");
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethod() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["b"] = 100;
            arguments["c"] = 10;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 1110);
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToBestMatchingMethodAndExtraParametersAreIgnored() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["b"] = 100;
            arguments["c"] = 10;
            arguments["e"] = 1;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 1110);
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToBestMatchingMethodAndExtraParametersAreIgnored2() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["e"] = 1;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 3000);
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethodWhenThereIsOne() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            arguments["b"] = 100;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 2200);
        }

        [Fact]
        public void EventParametersAreCorrectlyPassedToExactlyMatchingMethodWhenThereIsOne2() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["a"] = 1000;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 3000);
        }

        [Fact]
        public void EventHandlerWontBeCalledWhenNoParameterMatchExists() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            arguments["e"] = 1;
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 0);
        }

        [Fact]
        public void EventHandlerWontBeCalledWhenNoParameterMatchExists2() {
            Assert.Equal(_eventHandler.Result, 0);
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            _eventBus.Notify("ITestEventHandler.Sum", arguments);
            Assert.Equal(_eventHandler.Result, 0);
        }

        [Fact]
        public void EventHandlerWontThrowIfMethodDoesNotExists() {
            Dictionary<string, object> arguments = new Dictionary<string, object>();
            _eventBus.Notify("ITestEventHandler.NotExisting", arguments);
            // Assert.DoesNotThrow(() => _eventBus.Notify("ITestEventHandler.NotExisting", arguments));
        }

        [Fact]
        public void EventBusThrowsIfMessageNameIsNotCorrectlyFormatted() {
            Assert.Throws<ArgumentException>(() => _eventBus.Notify("StubEventHandlerIncrement", new Dictionary<string, object>()));
        }

        [Fact]
        public void InterceptorCanCoerceResultingCollection() {
            var data = new object[]{"5","18","2"};
            var adjusted = EventsInterceptor.Adjust(data, typeof(IEnumerable<string>));
            Assert.NotNull(data as IEnumerable<object>);
            Assert.Null(data as IEnumerable<string>);
            Assert.NotNull(adjusted as IEnumerable<string>);
        }

        [Fact]
        public void EnumerableResultsAreTreatedLikeSelectMany() {
            var results = _eventBus.Notify("ITestEventHandler.Gather", new Dictionary<string, object> { { "a", 42 }, { "b", "alpha" } }).Cast<string>();
            Assert.Equal(results.Count(), 3);
            Assert.Contains(results, t => t == "42");
            Assert.Contains(results,  t => t == "alpha");
            Assert.Contains(results,  t => t == "[42,alpha]");
        }

        [Fact]
        public void StringResultsAreTreatedLikeSelect() {
            var results = _eventBus.Notify("ITestEventHandler.GetString", new Dictionary<string, object>()).Cast<string>();
            Assert.Equal(results.Count(), 2);
            Assert.Contains(results, t => t == "Foo");
            Assert.Contains(results, t => t == "Bar");
        }

        [Fact]
        public void NonStringNonEnumerableResultsAreTreatedLikeSelect() {
            var results = _eventBus.Notify("ITestEventHandler.GetInt", new Dictionary<string, object>()).Cast<int>();
            Assert.Equal(results.Count(), 2);
            Assert.Contains(results, t => t == 1);
            Assert.Contains(results, t => t == 2);
        }
        
        public interface ITestEventHandler : IEventHandler {
            void Increment();
            void Sum(int a);
            void Sum(int a, int b);
            void Sum(int a, int b, int c);
            void Substract(int a, int b);
            void Concat(string a, string b, string c);
            IEnumerable<string> Gather(int a, string b);
            string GetString();
            int GetInt();
        }

        public class StubEventHandler : ITestEventHandler {
            public int Count { get; set; }
            public int Result { get; set; }
            public string Summary { get; set; }

            public void Increment() {
                Count++;
            }

            public void Sum(int a) {
                Result = 3 * a;
            }

            public void Sum(int a, int b) {
                Result = 2 * (a + b);
            }

            public void Sum(int a, int b, int c) {
                Result = a + b + c;
            }

            public void Substract(int a, int b) {
                Result = a - b;
            }

            public void Concat(string a, string b, string c) {
                Summary = a + b + c;
            }

            public IEnumerable<string> Gather(int a, string b) {
                yield return String.Format("[{0},{1}]", a, b);
            }

            public string GetString() {
                return "Foo";
            }

            public int GetInt() {
                return 1;
            }
        }
        public class StubEventHandler2 : ITestEventHandler {
            public void Increment() {
            }

            public void Sum(int a) {
            }

            public void Sum(int a, int b) {
            }

            public void Sum(int a, int b, int c) {
            }

            public void Substract(int a, int b) {
            }

            public void Concat(string a, string b, string c) {
            }

            public IEnumerable<string> Gather(int a, string b) {
                return new[] { a.ToString(), b };
            }

            public string GetString() {
                return "Bar";
            }

            public int GetInt() {
                return 2;
            }
        }
    }

    class StubExceptionPolicy : IExceptionPolicy {
        public bool HandleException(object sender, Exception exception) {
            return true;
        }
    }
}