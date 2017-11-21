using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Framework.Caching;
using Framework.Environment;
using Framework.Environment.Configuration;
using Framework.Environment.Extensions;
using Framework.Mvc;
using Framework.Mvc.Routes;
using Framework.Tests.Stub;
using Framework.Tests.Utility.Container;
using Moq;
using Xunit;

namespace Framework.Tests.Mvc.Routes
{
    public class ShellRouteTests {
        private RouteCollection _routes;
        private ILifetimeScope _containerA;
        private ILifetimeScope _containerB;
        private ShellSettings _settingsA;
        private ShellSettings _settingsB;
        private IContainer _rootContainer;

        
        public void Init() {
            _settingsA = new ShellSettings { Name = "Alpha" };
            _settingsB = new ShellSettings { Name = "Beta", };
            _routes = new RouteCollection();

            var rootBuilder = new ContainerBuilder();
            rootBuilder.Register(ctx => _routes);
            rootBuilder.RegisterType<ShellRoute>().InstancePerDependency();
            rootBuilder.RegisterType<RunningShellTable>().As<IRunningShellTable>().SingleInstance();
            rootBuilder.RegisterModule(new WorkContextModule());
            rootBuilder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>().InstancePerMatchingLifetimeScope("shell");
            rootBuilder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            rootBuilder.RegisterType<ExtensionManager>().As<IExtensionManager>();
            rootBuilder.RegisterType<StubCacheManager>().As<ICacheManager>();
            rootBuilder.RegisterType<StubAsyncTokenProvider>().As<IAsyncTokenProvider>();
            rootBuilder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();

            rootBuilder.Register<Func<RouteBase, ShellRoute>>(c => {
                var context = c.Resolve<IComponentContext>();
                return new Func<RouteBase, ShellRoute>(routeBase => 
                    new ShellRoute(
                        routeBase, 
                        _settingsA,
                        context.Resolve<IWorkContextAccessor>(),
                        context.Resolve<IRunningShellTable>(), objects => { return null; }));
            });

            _rootContainer = rootBuilder.Build();

            _containerA = _rootContainer.BeginLifetimeScope(
                "shell",
                builder => {
                    builder.Register(ctx => _settingsA);
                    builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerMatchingLifetimeScope("shell");
                });

            _containerB = _rootContainer.BeginLifetimeScope(
                "shell",
                builder => {
                    builder.Register(ctx => _settingsB);
                    builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerMatchingLifetimeScope("shell");
                });
        }

        public ShellRouteTests()
        {
            Init();
        }
        
        [Fact]
        public void FactoryMethodWillCreateShellRoutes() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ShellRoute>().InstancePerDependency();
            builder.RegisterAutoMocking();

            var container = builder.Build();
            var buildShellRoute = new Func<RouteBase, ShellRoute>(routeBase => 
                    new ShellRoute(
                        routeBase, 
                        _settingsA,
                        container.Resolve<IWorkContextAccessor>(),
                        container.Resolve<IRunningShellTable>(), 
                        objects => { return null; }));

            var routeA = new Route("foo", new MvcRouteHandler());
            var route1 = buildShellRoute(routeA);

            var routeB = new Route("bar", new MvcRouteHandler()) {
                DataTokens = new RouteValueDictionary { { "area", _settingsB.Name } }
            };
            var route2 = buildShellRoute(routeB);

            Assert.NotSame(route1, (route2));

            Assert.Equal(route1.ShellSettingsName, (_settingsA.Name));
            Assert.Null(route1.Area);

            Assert.Equal(route2.ShellSettingsName, (_settingsA.Name));
            Assert.Equal(route2.Area, (_settingsB.Name));
        }


        [Fact]
        public void RoutePublisherReplacesOnlyNamedShellsRoutes() {

            var routeA = new Route("foo", new MvcRouteHandler());
            var routeB = new Route("bar", new MvcRouteHandler());
            var routeC = new Route("quux", new MvcRouteHandler());

            _containerA.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Priority = 0, Route = routeA } });

            _containerB.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Priority = 0, Route = routeB } });

            // routes are grouped by name/priority/area
            Assert.Equal(_routes.Count(), (1));

            _containerA.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Priority = 0, Route = routeC } });

            Assert.Equal(_routes.Count(), (1));

            _containerB.Resolve<IRoutePublisher>().Publish(
                new[] {
                          new RouteDescriptor {Priority = 0, Route = routeA},
                          new RouteDescriptor {Priority = 0, Route = routeB},
                      });

            Assert.Equal(_routes.Count(), (1));
        }

        [Fact]
        public void RoutePublisherGroupsShellRoutesByName() {

            var routeA = new Route("foo", new MvcRouteHandler());
            var routeB = new Route("bar", new MvcRouteHandler());
            var routeC = new Route("quux", new MvcRouteHandler());

            _containerA.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Name = "1", Priority = 0, Route = routeA } });
            
            _containerA.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Name = "2", Priority = 0, Route = routeB } });

            Assert.Equal(_routes.Count(), (2));

            _containerB.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Name = "2", Priority = 0, Route = routeC } });

            Assert.Equal(_routes.Count(), (2));
        }

        [Fact]
        public void MatchingRouteToActiveShellTableWillLimitTheAbilityToMatchRoutes() {

            var routeFoo = new Route("foo", new MvcRouteHandler());

            _settingsA.RequestUrlHost = "a.example.com";
            _containerA.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Priority = 0, Route = routeFoo } });
            _rootContainer.Resolve<IRunningShellTable>().Add(_settingsA);
            
            _settingsB.RequestUrlHost = "b.example.com";
            _containerB.Resolve<IRoutePublisher>().Publish(
                new[] { new RouteDescriptor { Priority = 0, Route = routeFoo } });
            _rootContainer.Resolve<IRunningShellTable>().Add(_settingsB);

            var httpContext = new StubHttpContext("~/foo");
            var routeData = _routes.GetRouteData(httpContext);
            Assert.Null(routeData);

            var httpContextA = new StubHttpContext("~/foo", "a.example.com");
            var routeDataA = _routes.GetRouteData(httpContextA);
            Assert.NotNull(routeDataA);
            Assert.True(routeDataA.DataTokens.ContainsKey("IWorkContextAccessor"));
            var workContextAccessorA = (IWorkContextAccessor)routeDataA.DataTokens["IWorkContextAccessor"];
            var workContextScopeA = workContextAccessorA.CreateWorkContextScope(httpContextA);

            Assert.Same(workContextScopeA.Resolve<IRoutePublisher>(), (_containerA.Resolve<IRoutePublisher>()));
            Assert.NotSame(workContextScopeA.Resolve<IRoutePublisher>(), (_containerB.Resolve<IRoutePublisher>()));

            var httpContextB = new StubHttpContext("~/foo", "b.example.com");
            var routeDataB = _routes.GetRouteData(httpContextB);
            Assert.NotNull(routeDataB);
            Assert.True(routeDataB.DataTokens.ContainsKey("IWorkContextAccessor"));
            var workContextAccessorB = (IWorkContextAccessor)routeDataB.DataTokens["IWorkContextAccessor"];
            var workContextScopeB = workContextAccessorB.CreateWorkContextScope(httpContextB);

            Assert.Same(workContextScopeB.Resolve<IRoutePublisher>(), (_containerB.Resolve<IRoutePublisher>()));
            Assert.NotSame(workContextScopeB.Resolve<IRoutePublisher>(), (_containerA.Resolve<IRoutePublisher>()));

        }

        [Fact]
        public void RequestUrlPrefixAdjustsMatchingAndPathGeneration() {
            var settings = new ShellSettings { RequestUrlPrefix = "~/foo" };

            var builder = new ContainerBuilder();
            builder.RegisterType<ShellRoute>().InstancePerDependency();
            builder.RegisterAutoMocking();
            builder.Register(ctx => settings);

            var container = builder.Build();
            container.Mock<IRunningShellTable>()
                .Setup(x => x.Match(It.IsAny<HttpContextBase>()))
                .Returns(settings);


            var shellRouteFactory = new Func<RouteBase, ShellRoute>(routeBase =>
                    new ShellRoute(
                        routeBase,
                        settings,
                        container.Resolve<IWorkContextAccessor>(),
                        container.Resolve<IRunningShellTable>(), 
                        objects => { return null; }));

            var helloRoute = shellRouteFactory(new Route(
                "hello",
                new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" } },
                new MvcRouteHandler()));

            var tagsRoute = shellRouteFactory(new Route(
                "tags/{tagName}",
                new RouteValueDictionary { { "controller", "tags" }, { "action", "show" } },
                new MvcRouteHandler()));

            var defaultRoute = shellRouteFactory(new Route(
                "{controller}/{action}",
                new RouteValueDictionary { { "controller", "home" }, { "action", "index" } },
                new MvcRouteHandler()));

            var routes = new RouteCollection { helloRoute, tagsRoute, defaultRoute };

            var helloRouteData = routes.GetRouteData(new StubHttpContext("~/Foo/Hello"));
            Assert.NotNull(helloRouteData);
            Assert.Equal(helloRouteData.Values.Count(), (2));
            Assert.Equal(helloRouteData.GetRequiredString("controller"), ("foo"));
            Assert.Equal(helloRouteData.GetRequiredString("action"), ("bar"));

            var tagsRouteData = routes.GetRouteData(new StubHttpContext("~/Foo/Tags/my-tag-name"));
            Assert.NotNull(tagsRouteData);
            Assert.Equal(tagsRouteData.Values.Count(), (3));
            Assert.Equal(tagsRouteData.GetRequiredString("controller"), ("tags"));
            Assert.Equal(tagsRouteData.GetRequiredString("action"), ("show"));
            Assert.Equal(tagsRouteData.GetRequiredString("tagName"), ("my-tag-name"));

            var defaultRouteData = routes.GetRouteData(new StubHttpContext("~/Foo/Alpha/Beta"));
            Assert.NotNull(defaultRouteData);
            Assert.Equal(defaultRouteData.Values.Count(), (2));
            Assert.Equal(defaultRouteData.GetRequiredString("controller"), ("Alpha"));
            Assert.Equal(defaultRouteData.GetRequiredString("action"), ("Beta"));

            var defaultRouteData2 = routes.GetRouteData(new StubHttpContext("~/Foo/Alpha"));
            Assert.NotNull(defaultRouteData2);
            Assert.Equal(defaultRouteData2.Values.Count(), (2));
            Assert.Equal(defaultRouteData2.GetRequiredString("controller"), ("Alpha"));
            Assert.Equal(defaultRouteData2.GetRequiredString("action"), ("index"));

            var defaultRouteData3 = routes.GetRouteData(new StubHttpContext("~/Foo/"));
            Assert.NotNull(defaultRouteData3);
            Assert.Equal(defaultRouteData3.Values.Count(), (2));
            Assert.Equal(defaultRouteData3.GetRequiredString("controller"), ("home"));
            Assert.Equal(defaultRouteData3.GetRequiredString("action"), ("index"));

            var defaultRouteData4 = routes.GetRouteData(new StubHttpContext("~/Foo"));
            Assert.NotNull(defaultRouteData4);
            Assert.Equal(defaultRouteData4.Values.Count(), (2));
            Assert.Equal(defaultRouteData4.GetRequiredString("controller"), ("home"));
            Assert.Equal(defaultRouteData4.GetRequiredString("action"), ("index"));

            var requestContext = new RequestContext(new StubHttpContext("~/Foo/Alpha/Beta"), defaultRouteData);
            var helloVirtualPath = routes.GetVirtualPath(requestContext, helloRouteData.Values);
            Assert.NotNull(helloVirtualPath);
            Assert.Equal(helloVirtualPath.VirtualPath, ("~/foo/hello"));

            var defaultVirtualPath4 = routes.GetVirtualPath(requestContext, defaultRouteData4.Values);
            Assert.NotNull(defaultVirtualPath4);
            Assert.Equal(defaultVirtualPath4.VirtualPath, ("~/foo/"));
        }
    }
}