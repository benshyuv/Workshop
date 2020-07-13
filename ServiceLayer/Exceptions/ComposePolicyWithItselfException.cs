
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class ComposePolicyWithItselfException : Exception
    {
        public ComposePolicyWithItselfException() : base("Not possible to compose purchase policy with same policy")
        {
        }

        public ComposePolicyWithItselfException(string message) : base(message)
        {
        }

        public ComposePolicyWithItselfException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ComposePolicyWithItselfException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

