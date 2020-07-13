using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    internal class DeliveryFailedException : Exception
    {
        public DeliveryFailedException()
        {
        }

        public DeliveryFailedException(string message) : base(message)
        {
        }

        public DeliveryFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeliveryFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}