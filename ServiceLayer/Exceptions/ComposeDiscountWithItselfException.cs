using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceLayer.Exceptions
{
    [Serializable]
    internal class ComposeDiscountWithItselfException : Exception
    {
        public ComposeDiscountWithItselfException() : base("Not possible to compose discount with same discount")
        {
        }

        public ComposeDiscountWithItselfException(string message) : base(message)
        {
        }

        public ComposeDiscountWithItselfException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ComposeDiscountWithItselfException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
