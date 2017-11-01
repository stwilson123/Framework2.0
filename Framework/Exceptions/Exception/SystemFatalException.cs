using System;
using System.Runtime.Serialization;
using Framework.Localization;

namespace Framework.Exceptions.Exception
{
    [Serializable]
    public class SystemFatalException : System.Exception {
        private readonly LocalizedString _localizedMessage;

        public SystemFatalException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public SystemFatalException(LocalizedString message, System.Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        protected SystemFatalException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}