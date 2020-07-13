using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class StoreIDDoesntExistException : Exception
    {
        public StoreIDDoesntExistException() : base("StoreID not exists")
        {
        }

        public StoreIDDoesntExistException(string message) : base(message)
        {
        }

        public StoreIDDoesntExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StoreIDDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}