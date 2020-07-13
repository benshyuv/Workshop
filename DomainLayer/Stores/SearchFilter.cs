using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Stores.Inventory
{
    public interface SearchFilter
    {
        bool DoesItemStandInFilter(Item item);
        bool DoesStoreStandInFilter(Store store);
    }
}
