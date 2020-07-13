using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotGrantorException : Exception
    {
        public NotGrantorException() : base("not grantor")
        {
        }

        public NotGrantorException(string message) : base(message)
        {
        }

        public NotGrantorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotGrantorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}