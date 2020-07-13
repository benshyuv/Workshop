using System;
using System.Runtime.Serialization;

namespace DomainLayer.Stores
{
    [Serializable]
    internal class ContractFoundException : Exception
    {
        public ContractFoundException()
        {
        }

        public ContractFoundException(string message) : base(message)
        {
        }

        public ContractFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContractFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}