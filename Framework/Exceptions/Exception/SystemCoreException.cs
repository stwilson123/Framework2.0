using System;
using System.Runtime.Serialization;
using Framework.Localization;

namespace Framework.Exceptions.Exception
{
    [Serializable]
    public class SystemCoreException: System.Exception {
        private readonly LocalizedString _localizedMessage;

        public SystemCoreException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public SystemCoreException(LocalizedString message, System.Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        protected SystemCoreException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}