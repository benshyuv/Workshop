using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Stores.Inventory
{
    public class FilterByItemRank : SearchFilter
    {
        public double Rank { get; set; }
        public FilterByItemRank(double rank)
        {
            Rank = rank;
        }

        public bool DoesItemStandInFilter(Item item)
        {
            return (item.Rank >= Rank) ? true : false;
        }

        //not a store filter, default implementation of true inorder keep logic "And" correct
        public bool DoesStoreStandInFilter(Store store)
        {
            return true;
        }
    }
}
