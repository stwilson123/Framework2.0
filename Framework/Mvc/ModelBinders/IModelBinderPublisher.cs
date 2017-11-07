using System.Collections.Generic;

namespace Framework.Mvc.ModelBinders
{
    public interface IModelBinderPublisher : IDependency {
        void Publish(IEnumerable<ModelBinderDescriptor> binders);
    }
}