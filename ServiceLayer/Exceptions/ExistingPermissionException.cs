using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class ExistingPermissionException : Exception
    {
        public ExistingPermissionException() : base("already has this permission")
        {
        }

        public ExistingPermissionException(string message) : base(message)
        {
        }

        public ExistingPermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExistingPermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}