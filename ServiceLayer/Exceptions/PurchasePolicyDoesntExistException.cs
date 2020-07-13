
using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class PurchasePolicyDoesntExistException : Exception
    {
        public PurchasePolicyDoesntExistException() : base("PolicyID doesnt exist")
        {
        }

        public PurchasePolicyDoesntExistException(string message) : base(message)
        {
        }

        public PurchasePolicyDoesntExistException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PurchasePolicyDoesntExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}