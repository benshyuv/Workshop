
using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class MinHigherThanMaxException : Exception
    {
        public MinHigherThanMaxException() : base("Min Amount cannot be higher than Max Amount")
        {
        }

        public MinHigherThanMaxException(string message) : base(message)
        {
        }

        public MinHigherThanMaxException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MinHigherThanMaxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
