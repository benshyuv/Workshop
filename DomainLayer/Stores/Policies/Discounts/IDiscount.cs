using System;
using System.Collections.Generic;
using DomainLayer.Stores.Inventory;

namespace DomainLayer.Stores.Discounts
{
    public interface IDiscount
    {
        public Dictionary<Guid, Tuple<int, double>> GetUpdatedPricesFromCart(Dictionary<Guid,Tuple<int,double>> cartBefore);
    }
}
