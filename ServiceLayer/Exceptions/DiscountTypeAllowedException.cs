using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class DiscountTypeAllowedException : Exception
    {
        public DiscountTypeAllowedException() : base("DiscountType Already allowed")
        {
        }

        public DiscountTypeAllowedException(string message) : base(message)
        {
        }

        public DiscountTypeAllowedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DiscountTypeAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}