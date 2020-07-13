using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class AlreadyOwnerException : Exception
    {
        public AlreadyOwnerException() : base("already an owner")
        {
        }

        public AlreadyOwnerException(string message) : base(message)
        {
        }

        public AlreadyOwnerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AlreadyOwnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}