using System;

namespace Framework.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type type);
    }
}