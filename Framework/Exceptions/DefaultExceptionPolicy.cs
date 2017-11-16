using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using Framework.Environment;
using Framework.Events;
using Framework.Exceptions.Exception;
using Framework.Localization;
using Framework.Logging;
using Framework.Security;
using Framework.UI.Notify;
using SystemException = Framework.Exceptions.Exception.SystemException;

namespace Framework.Exceptions
{
    public class DefaultExceptionPolicy : IExceptionPolicy
    {
        private readonly INotifier _notifier;
        private readonly Work<IAuthorizer> _authorizer;

        public DefaultExceptionPolicy()
        {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public DefaultExceptionPolicy(INotifier notifier, Work<IAuthorizer> authorizer)
        {
            _notifier = notifier;
            _authorizer = authorizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public bool HandleException(object sender, System.Exception exception)
        {
            if (IsFatal(exception))
            {
                return false;
            }

            if (sender is IEventBus && exception is SystemFatalException)
            {
                return false;
            }

            Logger.Error(exception, "An unexpected exception was caught");

            do
            {
                RaiseNotification(exception);
                exception = exception.InnerException;
            } while (exception != null);

            return true;
        }

        private static bool IsFatal(System.Exception exception)
        {
            return
                exception is SystemSecurityException ||
                exception is StackOverflowException ||
                exception is AccessViolationException ||
                exception is AppDomainUnloadedException ||
                exception is ThreadAbortException ||
                exception is SecurityException ||
                exception is SEHException;
        }

        private void RaiseNotification(System.Exception exception)
        {
            if (_notifier == null || _authorizer.Value == null)
            {
                return;
            }
            if (exception is SystemException)
            {
                _notifier.Error((exception as SystemException).LocalizedMessage);
            }
            else if (_authorizer.Value.Authorize(StandardPermissions.SiteOwner))
            {
                _notifier.Error(T(exception.Message));
            }
        }
    }
}