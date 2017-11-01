using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Framework.Security;

namespace Framework.Exceptions
{
    public static class ExceptionExtensions {
        public static bool IsFatal(this System.Exception ex) {
            return ex is SystemSecurityException ||
                   ex is StackOverflowException ||
                   ex is OutOfMemoryException ||
                   ex is AccessViolationException ||
                   ex is AppDomainUnloadedException ||
                   ex is ThreadAbortException ||
                   ex is SecurityException ||
                   ex is SEHException;
        }
    }
}