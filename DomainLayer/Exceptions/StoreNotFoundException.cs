using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DomainLayer.Exceptions
{
    [Serializable]
    public class StoreNotFoundException :Exception
    {
        public StoreNotFoundException()
        {

        }

        public StoreNotFoundException(Guid storeID)
            : base(string.Format("Invalid Store id: {0}", storeID))
        {

        }

        public StoreNotFoundException(string storeName)
        : base(string.Format("Invalid store name: {0}", storeName))
        {

        }

        protected StoreNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
