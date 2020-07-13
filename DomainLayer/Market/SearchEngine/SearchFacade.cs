using CustomLogger;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Stores;
using DomainLayer.Stores.Inventory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DomainLayer.Market.SearchEngine
{
    public class SearchFacade
    {
        public readonly StoreHandler storeHandler;
        private Spelling spelling;
        public SearchFacade(StoreHandler storeHandler)
        {
            this.storeHandler = storeHandler;
            spelling = null; // will initialzed only when first needed;
        }

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
        /// <returns>dictionary of <storeID, reaonly collection of items></storeID> if there are, otherwise empty dictionary</returns>
        public Dictionary<Guid, ReadOnlyCollection<Item>> SearchItems(
            MarketDbContext context,
            double? filterItemRank,
            double? filterMinPrice,
            double? filterMaxPrice,
            double? filterStoreRank,
            string name = null,
            string category = null,
            List<string> keywords = null
            )
        {
            List<SearchFilter> filters = new List<SearchFilter>();

            if (filterItemRank is double itemRank)
            {
                Logger.writeEvent(string.Format("SearchFacade: Filtering by Item Rank- {0}", itemRank));
                FilterByItemRank fItemRank = new FilterByItemRank(itemRank);
                filters.Add(fItemRank);
            }

            if(filterMinPrice.HasValue || filterMaxPrice.HasValue) // if at least one has a value there is a filter price
            {
                Logger.writeEvent(string.Format("SearchFacade: Filtering by price, Max- {0} Min- {1}", 
                                                        filterMaxPrice.HasValue ? string.Format("N2",filterMaxPrice) : "None", 
                                                        filterMinPrice.HasValue ? string.Format("N2", filterMinPrice) : "None"));
                FilterByPrice fPrice = new FilterByPrice(filterMinPrice, filterMaxPrice);
                filters.Add(fPrice);
            }

            if(filterStoreRank is double storeRank)
            {
                Logger.writeEvent(string.Format("SearchFacade: Filtering by Store Rank- {0}", storeRank));
                FilterByStoreRank fStoreRank = new FilterByStoreRank(storeRank);
                filters.Add(fStoreRank);
            }

            if(filters.Count == 0)
            {
                filters = null;
            }

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context, name: name, category: category, keywords: keywords, filters: filters);

            if(name != null && results.Keys.Count == 0)
            {
                Logger.writeEvent(string.Format("SearchFacade: No results found, trying to fix spelling of \'{0}\'", name));
                string tryRepairName = TryCorrectNameBySpellChecking(name);
                if (!tryRepairName.Equals(name))
                {
                    results = storeHandler.SearchItems(context, name: tryRepairName, category: category, keywords: keywords, filters: filters);
                }
            }

            return results;
        }

        internal Guid GetStoreIDByName(string name, MarketDbContext context)
        {
            try
            {
                Store s = storeHandler.GetStoreByName(name, context);
                return s.Id;
            }
            catch (StoreNotFoundException)
            {
                return Guid.Empty;
            }
        }

        internal bool StoreExistByID(Guid storeID, MarketDbContext context)
        {
            try
            {
                Store s = storeHandler.GetStoreById(storeID, context);
                return s != null;
            }
            catch (StoreNotFoundException)
            {
                return false;
            }

        }

        /// <summary>
        /// Returns all the stores data
        /// If no data returns empty list.
        /// </summary>
        /// <returns>ReadOnlyCollection<Store></returns>
        public ReadOnlyCollection<Store> GetAllStoresInformation(MarketDbContext context) => storeHandler.Stores(context);

        private string TryCorrectNameBySpellChecking(string name)
        {
            if(spelling == null)
            {
                spelling = new Spelling();
            }

            string result = spelling.CorrectSentence(name);
            result.Trim(); // spelling returns " " if no answer

            return result;
        }

        /// <summary>
        /// Return requested item.
        /// If item was not found ItemNotFoundException is thrown
        /// </summary>
        /// <param name="storeID"></param>
        /// <param name="itemId"></param>
        /// <returns> The requested item</returns>
        public Item GetItemByIdFromStore(Guid storeID, Guid itemId, MarketDbContext context)
        {
            IStoreInventoryManager store = storeHandler.GetStoreInventoryManager(storeID, context);
            return store.GetItemById(itemId);
        }

        public void CheckItemAmount(Guid storeID, Guid itemId, int amount, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("SearchFacade: CheckItemAmount on store {0}, item {1}, amount {2}", storeID, itemId, amount));
            Item item = GetItemByIdFromStore(storeID, itemId, context);
            if (item.Amount < amount)
            {
                Logger.writeEvent(string.Format("SearchFacade: CheckItemAmount requested amount is higher than existing- {0}", item.Amount));
                throw new ItemAmountException(string.Format("Invalid Item amount, can't be higher than {0}", item.Amount));
            }
            Logger.writeEvent(string.Format("SearchFacade: CheckItemAmount requested amount ok. existing- {0}", item.Amount));
        }

        internal Store GetStoreInformationByID(Guid storeID, MarketDbContext context)
        {
            return storeHandler.GetStoreById(storeID, context);
        }

        internal bool StoreExistByName(string name, MarketDbContext context)
        {
            try
            {
                Store s = storeHandler.GetStoreByName(name, context);
                return s != null;
            }
            catch (StoreNotFoundException)
            {
                return false;
            }
        }
    }
}
