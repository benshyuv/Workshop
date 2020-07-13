using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class InsufficientItemAmountException : Exception
    {
        public InsufficientItemAmountException() : base("not enough amount of items in store inventory")
        {
        }

        public InsufficientItemAmountException(string message) : base(message)
        {
        }

        public InsufficientItemAmountException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InsufficientItemAmountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}