using System;
using Autofac;
using Framework.Caching;
using Framework.Services;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.Cache
{
   public class ClockCachingTests {
        private IContainer _container;
        private ICacheManager _cacheManager;
        private StubClock _clock;

       public ClockCachingTests()
       {
           Init();
       }
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new CacheModule());
            builder.RegisterType<DefaultCacheManager>().As<ICacheManager>();
            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultCacheContextAccessor>().As<ICacheContextAccessor>();
            builder.RegisterInstance<IClock>(_clock = new StubClock());
            _container = builder.Build();
            _cacheManager = _container.Resolve<ICacheManager>(new TypedParameter(typeof(Type), GetType()));
        }

        [Fact]
        public void WhenAbsoluteShouldHandleAbsoluteTime() {
            var inOneSecond = _clock.UtcNow.AddSeconds(1);
            var cached = 0;

            // each call after the specified datetime will be reevaluated
            Func<int> retrieve = ()
                => _cacheManager.Get("testItem",
                        ctx => {
                            ctx.Monitor(_clock.WhenUtc(inOneSecond));
                            return ++cached;
                        });

            Assert.Equal(retrieve(), 1);

            for ( var i = 0; i < 10; i++ ) {
                Assert.Equal(retrieve(), 1);
            }

            _clock.Advance(TimeSpan.FromSeconds(1));

           Assert.Equal(retrieve(), 2);
           Assert.Equal(retrieve(), 3);
           Assert.Equal(retrieve(), 4);
        }

        [Fact]
        public void WhenAbsoluteShouldHandleAbsoluteTimeSpan() {
            var cached = 0;

            // each cached value has a lifetime of the specified duration
            Func<int> retrieve = ()
                => _cacheManager.Get("testItem",
                        ctx => {
                            ctx.Monitor(_clock.When(TimeSpan.FromSeconds(1)));
                            return ++cached;
                        });

           Assert.Equal(retrieve(), 1);

            for ( var i = 0; i < 10; i++ ) {
               Assert.Equal(retrieve(), 1);
            }

            _clock.Advance(TimeSpan.FromSeconds(1));

            for ( var i = 0; i < 10; i++ ) {
               Assert.Equal(retrieve(), 2);
            }

            _clock.Advance(TimeSpan.FromSeconds(1));

            for ( var i = 0; i < 10; i++ ) {
               Assert.Equal(retrieve(), 3);
            }
        }
    }
}