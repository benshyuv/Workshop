using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotAdminException : Exception
    {
        public NotAdminException() : base("Not admin")
        {
        }

        public NotAdminException(string message) : base(message)
        {
        }

        public NotAdminException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotAdminException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}