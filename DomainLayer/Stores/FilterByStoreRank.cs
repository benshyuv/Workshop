using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Stores.Inventory
{
    public class FilterByStoreRank : SearchFilter
    {
        public double Rank { get; set; }
        public FilterByStoreRank(double rank)
        {
            Rank = rank;
        }

        //not an item filter, default implementation of true inorder keep logic "And" correct
        public bool DoesItemStandInFilter(Item item)
        {
            return true;
        }

        public bool DoesStoreStandInFilter(Store store)
        {
            return (store.Rank >= Rank) ? true : false;
        }
    }
}
