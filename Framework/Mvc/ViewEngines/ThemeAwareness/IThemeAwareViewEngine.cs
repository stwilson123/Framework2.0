using System.Web.Mvc;

namespace Framework.Mvc.ViewEngines.ThemeAwareness
{
    public interface IThemeAwareViewEngine : IDependency {
        ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache, bool useDeepPaths);
        ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, bool useDeepPaths);
    }
}