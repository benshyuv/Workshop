using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotOwnerException : Exception
    {
        public NotOwnerException() : base("not an owner")
        {
        }

        public NotOwnerException(string message) : base(message)
        {
        }

        public NotOwnerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotOwnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}