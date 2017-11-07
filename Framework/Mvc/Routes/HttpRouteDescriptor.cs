namespace Framework.Mvc.Routes
{
    public class HttpRouteDescriptor : RouteDescriptor {
        public string RouteTemplate { get; set; }
        public object Defaults { get; set; }
        public object Constraints { get; set; }
    }
}