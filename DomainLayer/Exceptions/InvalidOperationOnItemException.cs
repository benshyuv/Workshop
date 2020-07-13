using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class InvalidOperationOnItemException : Exception
    {
        public InvalidOperationOnItemException()
        {

        }

        public InvalidOperationOnItemException(string message )
            : base(message)
        {

        }

        protected InvalidOperationOnItemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
