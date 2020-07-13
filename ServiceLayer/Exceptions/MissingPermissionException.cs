using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class MissingPermissionException : Exception
    {
        public MissingPermissionException() : base("already doesnt has this permission")
        {
        }

        public MissingPermissionException(string message) : base(message)
        {
        }

        public MissingPermissionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingPermissionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}