using System.Collections.Generic;

namespace Framework.Mvc.Routes
{
    public interface IRouteProvider : IDependency {
        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}