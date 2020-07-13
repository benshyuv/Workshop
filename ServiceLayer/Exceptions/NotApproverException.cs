
using System;
using System.Runtime.Serialization;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class NotApproverException : Exception
    {
        public NotApproverException() : base("Not approver exception")
        {
        }

        public NotApproverException(string message) : base(message)
        {
        }

        public NotApproverException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotApproverException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}