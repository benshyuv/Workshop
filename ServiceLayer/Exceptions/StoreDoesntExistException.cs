using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class StoreDoesntExistException : Exception
    {
        public StoreDoesntExistException() : base("Store doesnt exist")
        {
        }

        public StoreDoesntExistException(string message) : base(message)
        {
        }

        public StoreDoesntExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StoreDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}