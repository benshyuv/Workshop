using System;
using DomainLayer.Stores.Discounts;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class DiscountTypeNotAllowedException : Exception
    {
        public DiscountTypeNotAllowedException() : base("DiscountType Already not allowed")
        {
        }

        public DiscountTypeNotAllowedException(DiscountType type):
            base(String.Format("Discount type {0} is not allowed", type.ToString()))
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
