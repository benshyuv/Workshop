using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class ItemAlreadyExistsException : Exception
    {
        public ItemAlreadyExistsException()
        {

        }

        public ItemAlreadyExistsException(string itemName)
            : base(string.Format("Item with name {0} already exists", itemName))
        {

        }

        protected ItemAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
