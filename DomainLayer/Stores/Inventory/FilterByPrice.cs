using DomainLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Stores.Inventory
{
    public class FilterByPrice : SearchFilter
    {
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public FilterByPrice(double? minPrice, double? maxPrice)
        {
            if(minPrice is double valueOfMinPrice && maxPrice is double valueOfMaxPrice)
            {
                if(valueOfMinPrice > valueOfMaxPrice)
                {
                    throw new InvalidSearchFilterException("FilterByPrice : minPrice bigger than maxPrice");
                }
            }
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }

        public bool DoesItemStandInFilter(Item item)
        {
             if (MinPrice is double valueOfMinPrice)
            {
                if (item.Price < valueOfMinPrice)
                {
                    return false;
                }
            }

             if ( MaxPrice is double valueOfMaxPrice)
            {
                if(item.Price > valueOfMaxPrice)
                {
                    return false;
                }
            }

            return true;
        }

        //not a store filter, default implementation of true inorder keep logic "And" correct
        public bool DoesStoreStandInFilter(Store store)
        {
            return true;
        }
    }
}
