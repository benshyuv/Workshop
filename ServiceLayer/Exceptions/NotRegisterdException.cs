using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotRegisterdException : Exception
    {
        public NotRegisterdException() : base("not a registered user")
        {
        }

        public NotRegisterdException(string message) : base(message)
        {
        }

        public NotRegisterdException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotRegisterdException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}