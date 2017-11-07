using System;
using System.Configuration;

namespace Framework.Data
{
    public interface ISessionConfigurationCache {
        Configuration GetConfiguration(Func<Configuration> builder);
    }
}