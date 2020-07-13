using System;
using System.Runtime.Serialization;
namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class DiscountTypeNotAllowedException : Exception
    {
        public DiscountTypeNotAllowedException() : base("DiscountType Already not allowed")
        {
        }

        public DiscountTypeNotAllowedException(string message) : base(message)
        {
        }

        public DiscountTypeNotAllowedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DiscountTypeNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

