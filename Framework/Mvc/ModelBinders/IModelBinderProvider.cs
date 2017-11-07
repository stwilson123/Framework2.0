using System.Collections.Generic;

namespace Framework.Mvc.ModelBinders
{
    public interface IModelBinderProvider : IDependency {
        IEnumerable<ModelBinderDescriptor> GetModelBinders();
    }
}