using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(Guid itemId)
            : base(string.Format("Invalid Item id: {0}", itemId))
        {

        }

        public ItemNotFoundException(string name)
           : base(string.Format("Invalid Item name: {0}", name))
        {

        }

        protected ItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}