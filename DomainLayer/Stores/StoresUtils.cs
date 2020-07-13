using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Stores
{
    public class StoresUtils
    {
        public enum ItemEditDetails
        {
            name,
            amount,
            categories,
            rank,
            price,
            keyWords
        }

        public enum StoreEditContactDetails
        {
            name,
            email,
            address,
            phone,
            bankAccountNumber,
            bank,
            description
        }
    }
}
