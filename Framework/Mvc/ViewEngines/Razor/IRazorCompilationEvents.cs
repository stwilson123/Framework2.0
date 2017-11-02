using System.Web.Razor.Generator;
using System.Web.WebPages.Razor;

namespace Framework.Mvc.ViewEngines.Razor
{
    public interface IRazorCompilationEvents {
        void CodeGenerationStarted(RazorBuildProvider provider);
        void CodeGenerationCompleted(RazorBuildProvider provider, CodeGenerationCompleteEventArgs e);
    }
}