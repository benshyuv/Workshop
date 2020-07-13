
using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class OverlappingPolicyException : Exception
    {
        public OverlappingPolicyException() : base("Purchase Policy not valid - overlapping with exsisting")
        {
        }

        public OverlappingPolicyException(string message) : base(message)
        {
        }

        public OverlappingPolicyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OverlappingPolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}