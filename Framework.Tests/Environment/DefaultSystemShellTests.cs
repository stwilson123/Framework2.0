using Framework.Mvc.ModelBinders;
using Framework.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Framework.Environment;
using Moq;
using Xunit;
using IModelBinderProvider = Framework.Mvc.ModelBinders.IModelBinderProvider;

namespace Framework.Tests.Environment
{
    public class DefaultSystemShellTests
    {
        static RouteDescriptor Desc(string name, string url)
        {
            return new RouteDescriptor { Name = name, Route = new Route(url, new System.Web.Mvc.MvcRouteHandler()) };
        }

        static ModelBinderDescriptor BinderDesc(Type type, IModelBinder modelBinder)
        {
            return new ModelBinderDescriptor { Type = type, ModelBinder = modelBinder };
        }

//        [Fact]
//        public void ActivatingRuntimeCausesRoutesAndModelBindersToBePublished() {
//
//            var provider1 = new StubRouteProvider(new[] { Desc("foo1", "foo1"), Desc("foo2", "foo2") });
//            var provider2 = new StubRouteProvider(new[] { Desc("foo1", "foo1"), Desc("foo2", "foo2") });
//            var publisher = new StubRoutePublisher();
//
//            var modelBinderProvider1 = new StubModelBinderProvider(new[] { BinderDesc(typeof(object), null), BinderDesc(typeof(string), null) });
//            var modelBinderProvider2 = new StubModelBinderProvider(new[] { BinderDesc(typeof(int), null), BinderDesc(typeof(long), null) });
//            var modelBinderPublisher = new StubModelBinderPublisher();
//
//            var runtime = new DefaultSystemShell(
//                new[] { provider1, provider2 },
//                publisher,
//                new[] { modelBinderProvider1, modelBinderProvider2 },
//                modelBinderPublisher,
//                new ViewEngineCollection { new WebFormViewEngine() },
//                new Mock<ISystemShellEvents>().Object);
//
//            runtime.Activate();
//
//            Assert.Equal(publisher.Routes.Count(),  4 );
//            Assert.Equal(modelBinderPublisher.ModelBinders.Count(),  4);
//        }

        public class StubRouteProvider : IRouteProvider
        {
            private readonly IEnumerable<RouteDescriptor> _routes;

            public StubRouteProvider(IEnumerable<RouteDescriptor> routes)
            {
                _routes = routes;
            }

            public void GetRoutes(ICollection<RouteDescriptor> routes)
            {
                foreach (var routeDescriptor in _routes)
                    routes.Add(routeDescriptor);
            }
        }

        public class StubRoutePublisher : IRoutePublisher
        {
            public void Publish(IEnumerable<RouteDescriptor> routes)
            {
                Routes = routes;
            }
            public IEnumerable<RouteDescriptor> Routes { get; set; }
            public void Publish(IEnumerable<RouteDescriptor> routes, Func<IDictionary<string, object>, Task> pipeline)
            {
            }
        }

        public class StubModelBinderProvider : IModelBinderProvider
        {
            private readonly IEnumerable<ModelBinderDescriptor> _binders;

            public StubModelBinderProvider(IEnumerable<ModelBinderDescriptor> routes)
            {
                _binders = routes;
            }

            public IEnumerable<ModelBinderDescriptor> GetModelBinders()
            {
                return _binders;
            }
        }

        public class StubModelBinderPublisher : IModelBinderPublisher
        {
            public void Publish(IEnumerable<ModelBinderDescriptor> modelBinders)
            {
                ModelBinders = modelBinders;
            }
            public IEnumerable<ModelBinderDescriptor> ModelBinders { get; set; }
        }
    }
}
