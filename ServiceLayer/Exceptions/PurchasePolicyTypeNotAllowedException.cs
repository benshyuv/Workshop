
using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class PurchasePolicyTypeNotAllowedException : Exception
    {
        public PurchasePolicyTypeNotAllowedException() : base("PurchasePolicyType Already  not allowed")
        {
        }

        public PurchasePolicyTypeNotAllowedException(string message) : base(message) { 
         
        }

        public PurchasePolicyTypeNotAllowedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PurchasePolicyTypeNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}