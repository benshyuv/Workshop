using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class OrderNotValidException : Exception
    {
        public OrderNotValidException(string message)
            : base(message)
        {
        }

        protected OrderNotValidException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
