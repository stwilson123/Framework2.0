using System;
using Framework.Caching;

namespace Framework.Environment.Extensions
{
    public interface IExtensionMonitoringCoordinator {
        void MonitorExtensions(Action<IVolatileToken> monitor);
    }
}