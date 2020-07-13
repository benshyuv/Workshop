using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class ItemDoesntExistException : Exception
    {
        public ItemDoesntExistException() : base("item doesnt exist")
        {
        }

        public ItemDoesntExistException(string message) : base(message)
        {
        }

        public ItemDoesntExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ItemDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}