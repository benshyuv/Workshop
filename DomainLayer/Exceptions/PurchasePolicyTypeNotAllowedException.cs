
using System;
using System.Runtime.Serialization;
using DomainLayer.Stores;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class PurchasePolicyTypeNotAllowedException : Exception
    {
        public PurchasePolicyTypeNotAllowedException() : base("PurchasePolicyType Already not allowed")
        {
        }

        public PurchasePolicyTypeNotAllowedException(PurchasePolicyType type) :
            base(String.Format("PurchasePolicyType type {0} is not allowed", type.ToString()))
        {

        }

        public PurchasePolicyTypeNotAllowedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PurchasePolicyTypeNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
