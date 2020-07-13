using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class InvalidSearchFilterException : Exception
    {
        public InvalidSearchFilterException()
        {

        }

        public InvalidSearchFilterException(string message)
            : base(message)
        {

        }

        protected InvalidSearchFilterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
