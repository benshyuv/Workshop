using DomainLayer.DbAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DomainLayer.Stores.Inventory
{
    interface IStoreSearch
    {
        /// <summary>
        /// Search for items in store by the "And" of following params: 
        /// name (if provided) &
        /// category (if provided) &
        /// keywords (if provided) &
        /// standing in filters (if provided)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="keywords"></param>
        /// <param name="filters"></param>
        /// <returns>reaonly collection of items if there are, otherwise empty readonly collection</returns>
        ReadOnlyCollection<Item> SearchItems(MarketDbContext context = null, string name = null, string category = null,
            List<string> keywords = null, List<SearchFilter> filters = null);
        /*        ReadOnlyCollection<Item> SearchByName(string name, List<SearchFilter> filters);
                ReadOnlyCollection<Item> SearchByCategory(string category, List<SearchFilter> filters);
                ReadOnlyCollection<Item> SearchByKeyWords(List<string> keywords, List<SearchFilter> filters);*/
    }
}
