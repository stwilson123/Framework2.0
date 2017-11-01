using System;
using System.Runtime.Serialization;
using Framework.Localization;

namespace Framework.Exceptions.Exception
{
    [Serializable]
    public class SystemException: ApplicationException {
        private readonly LocalizedString _localizedMessage;

        public SystemException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public SystemException(LocalizedString message, System.Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        protected SystemException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}