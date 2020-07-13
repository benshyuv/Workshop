using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class StoreNameTakenException : Exception
    {
        public StoreNameTakenException() : base("store name already exists")
        {
        }

        public StoreNameTakenException(string message) : base(message)
        {
        }

        public StoreNameTakenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StoreNameTakenException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}