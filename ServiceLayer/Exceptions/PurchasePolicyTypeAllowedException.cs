
using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class PurchasePolicyTypeAllowedException : Exception
    {
        public PurchasePolicyTypeAllowedException() : base("PurchasePolicyType Already  allowed")
        {
        }

        public PurchasePolicyTypeAllowedException(string message) : base(message)
        {

        }

        public PurchasePolicyTypeAllowedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PurchasePolicyTypeAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}