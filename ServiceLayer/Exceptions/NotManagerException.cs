using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotManagerException : Exception
    {
        public NotManagerException() : base("not a manager")
        {
        }

        public NotManagerException(string message) : base(message)
        {
        }

        public NotManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotManagerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}