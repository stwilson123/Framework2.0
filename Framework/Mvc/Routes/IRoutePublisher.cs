using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Mvc.Routes
{
    public interface IRoutePublisher : IDependency {
        void Publish(IEnumerable<RouteDescriptor> routes, Func<IDictionary<string, object>, Task> pipeline = null);
    }
}