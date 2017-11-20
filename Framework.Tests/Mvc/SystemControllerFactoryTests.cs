using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Web;
using Framework.Environment.Extensions.Models;
using Framework.Mvc;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.Mvc
{
     public class SystemControllerFactoryTests {
        private SystemControllerFactory _controllerFactory;
        private IWorkContextAccessor _workContextAccessor;
        private StubContainerProvider _containerProvider;

         
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<FooController>()
                .Keyed<IController>("/foo")
                .Keyed<IController>(typeof(FooController))
                .WithMetadata("ControllerType", typeof(FooController))
                .InstancePerDependency();

            builder.RegisterType<BarController>()
                .Keyed<IController>("/bar")
                .Keyed<IController>(typeof(BarController))
                .WithMetadata("ControllerType", typeof(BarController))
                .InstancePerDependency();

            builder.RegisterType<ReplacementFooController>()
                .Keyed<IController>("/foo")
                .Keyed<IController>(typeof(ReplacementFooController))
                .WithMetadata("ControllerType", typeof(ReplacementFooController))
                .InstancePerDependency();

            var container = builder.Build();
            _containerProvider = new StubContainerProvider(container, container.BeginLifetimeScope());

            var workContext = new TestWorkContext
            {
                CurrentTheme = new ExtensionDescriptor { Id = "Hello" },
                ContainerProvider = _containerProvider
            };
            _workContextAccessor = new TestWorkContextAccessor(workContext);

            _controllerFactory = new SystemControllerFactory();
            InjectKnownControllerTypes(_controllerFactory, typeof(ReplacementFooController), typeof (FooController), typeof (BarController));
        }

         public SystemControllerFactoryTests()
         {
             Init();
         }
        [Fact]
        public void IContainerProvidersRequestContainerFromRouteDataShouldUseTokenWhenPresent() {
            var requestContext = GetRequestContext(_workContextAccessor);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.IsType<ReplacementFooController>(controller);
        }

        [Fact(Skip = "OrchardControllerFactory depends on metadata, calling base when no context is causing errors.")]
        public void WhenNullOrMissingContainerNormalControllerFactoryRulesShouldBeUsedAsFallback() {
            var requestContext = GetRequestContext(null);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.IsType<FooController>(controller);
        }

        [Fact]
        public void WhenContainerIsPresentButNamedControllerIsNotResolvedNormalControllerFactoryRulesShouldBeUsedAsFallback() {
            var requestContext = GetRequestContext(_workContextAccessor);
            var controller = _controllerFactory.CreateController(requestContext, "bar");

            Assert.IsType<BarController>(controller);
        }

        [Fact]
        public void DisposingControllerThatCameFromContainerShouldNotCauseProblemWhenContainerIsDisposed() {
            var requestContext = GetRequestContext(_workContextAccessor);
            var controller = _controllerFactory.CreateController(requestContext, "foo");

            Assert.IsType<ReplacementFooController>(controller );

            _controllerFactory.ReleaseController(controller);
            _containerProvider.EndRequestLifetime();

            // explicitly dispose a few more, just to make sure it's getting hit from all different directions
            ((IDisposable) controller).Dispose();
            ((ReplacementFooController) controller).Dispose();

            Assert.Equal(((ReplacementFooController) controller).Disposals, 4);
        }

        [Fact]
        public void NullServiceKeyReturnsDefault() {
            SystemControllerFactoryAccessor systemControllerFactory = new SystemControllerFactoryAccessor();
            ReplacementFooController fooController;

            Assert.False(systemControllerFactory.TryResolveAccessor(_workContextAccessor.GetContext(), null, out fooController));
            Assert.Null(fooController );
        }

        private static RequestContext GetRequestContext(IWorkContextAccessor workContextAccessor)
        {
            var handler = new MvcRouteHandler();
            var route = new Route("yadda", handler) {
                                                        DataTokens =
                                                            new RouteValueDictionary { { "IWorkContextAccessor", workContextAccessor } }
                                                    };

            var httpContext = new StubHttpContext();
            var routeData = route.GetRouteData(httpContext);
            return new RequestContext(httpContext, routeData);
        }

        public class FooController : Controller { }

        public class BarController : Controller { }

        public class ReplacementFooController : Controller {
            protected override void Dispose(bool disposing) {
                ++Disposals;

                base.Dispose(disposing);
            }

            public int Disposals { get; set; }
        }

        internal class SystemControllerFactoryAccessor : SystemControllerFactory {
            public bool TryResolveAccessor<T>(WorkContext workContext, object serviceKey, out T instance) {
                return TryResolve(workContext, serviceKey, out instance);
            }
        }

        private static void InjectKnownControllerTypes(DefaultControllerFactory controllerFactory,
                                                       params Type[] controllerTypes) {
            // D'oh!!! Hey MVC people, how is this testable? ;)

            // locate the appropriate reflection member info
            var controllerTypeCacheProperty = controllerFactory.GetType()
                .GetProperty("ControllerTypeCache", BindingFlags.Instance | BindingFlags.NonPublic);
            var cacheField = controllerTypeCacheProperty.PropertyType.GetField("_cache",
                                                                               BindingFlags.NonPublic |
                                                                               BindingFlags.Instance);

            // turn the array into the correct collection
            var cache = controllerTypes
                .GroupBy(t => t.Name.Substring(0, t.Name.Length - "Controller".Length), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key,
                              g => g.ToLookup(t => t.Namespace ?? string.Empty, StringComparer.OrdinalIgnoreCase),
                              StringComparer.OrdinalIgnoreCase);

            // execute: controllerFactory.ControllerTypeCache._cache = cache;
            cacheField.SetValue(
                controllerTypeCacheProperty.GetValue(controllerFactory, null),
                cache);
        }


         public class TestWorkContextAccessor : IWorkContextAccessor {
                    private readonly WorkContext _workContext;
                    public TestWorkContextAccessor(WorkContext workContext) {
                        _workContext = workContext;
                    }
        
                    public WorkContext GetContext(HttpContextBase httpContext) {
                        return _workContext;
                    }
        
                    public IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext) {
                        throw new NotImplementedException();
                    }
        
                    public WorkContext GetContext() {
                        return _workContext;
                    }
        
                    public IWorkContextScope CreateWorkContextScope() {
                        throw new NotImplementedException();
                    }
                }
         
         public class TestWorkContext : WorkContext {
             readonly IDictionary<string, object> _state = new Dictionary<string, object>();
             public IContainerProvider ContainerProvider { get; set; }

             public override T Resolve<T>() {
                 if (typeof(T) == typeof(ILifetimeScope)) {
                     return (T)ContainerProvider.RequestLifetime;
                 }

                 throw new NotImplementedException();
             }

             public override object Resolve(Type serviceType) {
                 throw new NotImplementedException();
             }

             public override bool TryResolve<T>(out T service) {
                 throw new NotImplementedException();
             }

             public override bool TryResolve(Type serviceType, out object service) {
                 throw new NotImplementedException();
             }

             public override T GetState<T>(string name) {
                 object value;
                 return _state.TryGetValue(name, out value) ? (T)value : default(T);
             }

             public override void SetState<T>(string name, T value) {
                 _state[name] = value;
             }
         }

   }
}