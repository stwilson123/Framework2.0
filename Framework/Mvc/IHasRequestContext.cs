using System.Web.Routing;

namespace Framework.Mvc
{
    public interface IHasRequestContext {
        RequestContext RequestContext { get; }
    }
}