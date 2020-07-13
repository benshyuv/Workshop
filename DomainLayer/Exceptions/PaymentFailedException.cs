using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    internal class PaymentFailedException : Exception
    {
        public PaymentFailedException()
        {
        }

        public PaymentFailedException(string message) : base(message)
        {
        }

        public PaymentFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PaymentFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}