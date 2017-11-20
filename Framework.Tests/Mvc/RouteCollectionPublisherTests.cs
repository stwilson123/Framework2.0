using System;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Framework.Environment.Configuration;
using Framework.Mvc.Routes;
using Framework.Tests.Utility.Container;
using Xunit;

namespace Framework.Tests.Mvc
{
    public class RouteCollectionPublisherTests {
        private IContainer _container;
        private RouteCollection _routes;

        static RouteDescriptor Desc(string name, string url) {
            return new RouteDescriptor { Name = name, Route = new Route(url, new MvcRouteHandler()) };
        }

        
        public void Init() {
            _routes = new RouteCollection();

            var builder = new ContainerBuilder();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>();
            builder.RegisterType<ShellRoute>().InstancePerDependency();
            builder.Register(ctx => _routes);
            builder.Register(ctx => new ShellSettings { Name = ShellSettings.DefaultName });
            builder.RegisterAutoMocking();
            _container = builder.Build();
        }

        public RouteCollectionPublisherTests()
        {
            Init();
        }
        [Fact]
        public void RoutesCanHaveNullOrEmptyNames() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            publisher.Publish(new[] { Desc(null, "bar"), Desc(string.Empty, "quux") });

            Assert.Equal(_routes.Count(), (3));
        }

        [Fact]
        public void SameNameTwiceCausesExplosion() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            publisher.Publish(new[] { Desc("yarg", "bar"), Desc("yarg", "quux") });

            Assert.Throws<ArgumentException>(() => _routes.Count() == (2));
        }


        [Fact]
        public void ExplosionLeavesOriginalRoutesIntact() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            try {
                publisher.Publish(new[] { Desc("yarg", "bar"), Desc("yarg", "quux") });
            }
            catch (ArgumentException) {
                Assert.Equal(_routes.Count(), (1));
                Assert.Equal(_routes.OfType<Route>().Single().Url, ("{controller}"));
            }
        }

        [Fact]
        public void WriteBlocksWhileReadIsInEffect() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();

            var readLock = _routes.GetReadLock();

            string where = "init";
            var action = new Action(() => {
                where = "before";
                publisher.Publish(new[] { Desc("barname", "bar"), Desc("quuxname", "quux") });
                where = "after";
            });

            Assert.Equal(where, ("init"));
            var asyncResult = action.BeginInvoke(null, null);
            Thread.Sleep(75);
            Assert.Equal(where, ("before"));
            readLock.Dispose();
            Thread.Sleep(75);
            Assert.Equal(where, ("after"));
            action.EndInvoke(asyncResult);
        }

        [Fact]
        public void RouteDescriptorWithNameCreatesNamedRouteInCollection() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            var routeDescriptor = Desc("yarg", "bar");
            publisher.Publish(new[] { routeDescriptor });

            Assert.NotNull(_routes["yarg"]);
        }
    }
}