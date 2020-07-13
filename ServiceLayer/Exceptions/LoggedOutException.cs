using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class LoggedOutException : Exception
    {
        public LoggedOutException() : base("not logged in")
        {
        }

        public LoggedOutException(string message) : base(message)
        {
        }

        public LoggedOutException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LoggedOutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}