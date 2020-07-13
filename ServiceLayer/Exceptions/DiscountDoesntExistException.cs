using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class DiscountDoesntExistException : Exception
    {
        public DiscountDoesntExistException() : base("DiscountID doesnt exist")
        {
        }

        public DiscountDoesntExistException(string message) : base(message)
        {
        }

        public DiscountDoesntExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DiscountDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}