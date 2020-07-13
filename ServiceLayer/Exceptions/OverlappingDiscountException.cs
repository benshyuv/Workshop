using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class OverlappingDiscountException : Exception
    {
        public OverlappingDiscountException() : base("Discount not valid - overlapping with exsisting")
        {
        }

        public OverlappingDiscountException(string message) : base(message)
        {
        }

        public OverlappingDiscountException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OverlappingDiscountException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}