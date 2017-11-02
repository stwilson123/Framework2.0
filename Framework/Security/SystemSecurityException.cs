using System;
using System.Runtime.Serialization;
using Framework.Exceptions.Exception;
using Framework.Localization;

namespace Framework.Security
{
    [Serializable]
    public class SystemSecurityException : SystemCoreException {
        public SystemSecurityException(LocalizedString message) : base(message) { }
        public SystemSecurityException(LocalizedString message, Exception innerException) : base(message, innerException) { }
        protected SystemSecurityException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public string PermissionName { get; set; }
        public IUser User { get; set; }
      //  public IContent Content { get; set; }
    }
}