using System;
using System.Runtime.Serialization;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class StoreAlreadyExistException : Exception
    {
        public StoreAlreadyExistException(string storeName)
            : base(string.Format("Store with name {0} already exists", storeName))
        {

        }

        protected StoreAlreadyExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
