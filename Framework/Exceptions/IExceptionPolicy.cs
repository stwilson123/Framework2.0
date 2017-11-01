using System;

namespace Framework.Exceptions
{
    public interface IExceptionPolicy : ISingletonDependency {
        /* return false if the exception should be rethrown by the caller */
        bool HandleException(object sender, System.Exception exception);
    }
}