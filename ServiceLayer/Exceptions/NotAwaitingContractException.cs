using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotAwaitingContractException : Exception
    {
        public NotAwaitingContractException() : base("Not awaiting contract approval")
        {
        }

        public NotAwaitingContractException(string message) : base(message)
        {
        }

        public NotAwaitingContractException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotAwaitingContractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}