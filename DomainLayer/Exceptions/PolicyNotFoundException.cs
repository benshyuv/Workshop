using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    internal class PolicyNotFoundException : Exception
    {

        public PolicyNotFoundException()
        {
        }

        public PolicyNotFoundException(Guid policyID) : base(string.Format("Invalid Discount id: {0}", policyID))
        {
            
        }

        public PolicyNotFoundException(string message) : base(message)
        {
        }

        public PolicyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PolicyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
