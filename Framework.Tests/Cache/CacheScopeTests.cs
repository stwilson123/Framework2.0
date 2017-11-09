using Autofac;
using Autofac.Features.GeneratedFactories;
using Framework.Caching;
using Framework.Environment;
using Framework.FileSystems.AppData;
using Framework.FileSystems.WebSite;
using Framework.Services;
using Xunit;

namespace Framework.Tests.Cache
{
    public class CacheScopeTests {
        private IContainer _hostContainer;

        
        public void Init() {
            _hostContainer = SystemStarter.CreateHostContainer(builder => {
                builder.RegisterType<Alpha>().InstancePerDependency();
            });

        }

        public class Alpha {
            public ICacheManager CacheManager { get; set; }

            public Alpha(ICacheManager cacheManager) {
                CacheManager = cacheManager;
            }
        }

        public CacheScopeTests()
        {
           
        }
        [Fact]
        public void ComponentsAtHostLevelHaveAccessToCache() {
            Init();
            var alpha = _hostContainer.Resolve<Alpha>();
            Assert.NotNull(alpha.CacheManager);
        }

        [Fact]
        public void HostLevelHasAccessToGlobalVolatileProviders() {
            Assert.NotNull(_hostContainer.Resolve<IWebSiteFolder>());
            Assert.NotNull(_hostContainer.Resolve<IAppDataFolder>());
            Assert.NotNull(_hostContainer.Resolve<IClock>());
        }

    }
}