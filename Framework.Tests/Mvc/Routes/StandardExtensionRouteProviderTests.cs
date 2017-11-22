using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Framework.Environment.Extensions.Models;
using Framework.Environment.ShellBuilder.Models;
using Framework.Mvc.Routes;
using Xunit;

namespace Framework.Tests.Mvc.Routes
{
   public class StandardExtensionRouteProviderTests {
        [Fact]
        public void ExtensionDisplayNameShouldBeUsedInBothStandardRoutes() {
            var blueprint = new ShellBlueprint {
                Controllers = new[] {
                    new ControllerBlueprint {
                        AreaName ="Long.Name.Foo",
                        Feature =new Feature {
                            Descriptor=new FeatureDescriptor {
                                Extension=new ExtensionDescriptor {
                                    Id="Foo",
                                    Name="A Foo Module",
                                    Path="Foo"
                                }
                            }
                        }
                    },
                    new ControllerBlueprint {
                        AreaName ="Long.Name.Bar",
                        Feature =new Feature {
                            Descriptor=new FeatureDescriptor {
                                Extension=new ExtensionDescriptor {
                                    Id="Bar",
                                    Name="Bar",
                                    Path="BarBar"
                                }
                            }
                        }
                    }
                }
            };
            var routeProvider = new StandardExtensionRouteProvider(blueprint);

            var routes = new List<RouteDescriptor>();
            routeProvider.GetRoutes(routes);

            Assert.Equal(routes.Count,  4);
            var fooAdmin = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Admin/Foo/{action}/{id}");
            var fooRoute = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Foo/{controller}/{action}/{id}");
            var barAdmin = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Admin/BarBar/{action}/{id}");
            var barRoute = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "BarBar/{controller}/{action}/{id}");

            Assert.Equal(fooAdmin.DataTokens["area"], ("Long.Name.Foo"));
            Assert.Equal(fooRoute.DataTokens["area"], ("Long.Name.Foo"));
            Assert.Equal(barAdmin.DataTokens["area"], ("Long.Name.Bar"));
            Assert.Equal(barRoute.DataTokens["area"], ("Long.Name.Bar"));
        }
    }
}