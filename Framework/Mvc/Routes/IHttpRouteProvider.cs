using System.Collections.Generic;

namespace Framework.Mvc.Routes
{
    public interface IHttpRouteProvider : IDependency {
        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}