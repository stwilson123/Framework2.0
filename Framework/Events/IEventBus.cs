using System.Collections;
using System.Collections.Generic;

namespace Framework.Events
{
    public interface IEventBus : IDependency {
        IEnumerable Notify(string messageName, IDictionary<string, object> eventData);
    }
}