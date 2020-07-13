using DomainLayer.Exceptions;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Inventory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CustomLogger;
using DomainLayer.Stores.Discounts;
using DomainLayer.NotificationsCenter;
using DomainLayer.NotificationsCenter.NotificationEvents;
using DomainLayer.DbAccess;
using System.Data.Entity;
using System.Collections.Concurrent;
using System.Threading;

[assembly: InternalsVisibleTo("DomainLayerTests")]
namespace DomainLayer.Stores
{
    public class StoreHandler : INotificationSubject
    {
        //private Dictionary<Guid, Store> stores;
        private HashSet<INotificationObserver> observers;
        private Guid Id;

        private ConcurrentDictionary<Guid, Mutex> storeIdToLock;

        public StoreHandler()
        {
            //stores = new Dictionary<Guid, Store>() { };
            this.Id = Guid.NewGuid();
            observers = new HashSet<INotificationObserver>();
            storeIdToLock = new ConcurrentDictionary<Guid, Mutex>();
        }

        public Store OpenStore(StoreContactDetails contactDetails, Guid owner, MarketDbContext context)
        {
            if (TryGetStore(contactDetails.Name, out _, context))
            {
                Logger.writeEvent(string.Format("StoreHandler: OpenStore| store with name \'{0}\' already exists", contactDetails.Name));
                throw new StoreAlreadyExistException(contactDetails.Name);
            }

            Guid storeID = Guid.NewGuid();
            Store newStore = new Store(storeID, contactDetails, new PurchasePolicy(storeID), new DiscountPolicy(storeID), owner, context);

            context.Stores.Add(newStore);
            context.SaveChanges();
            Logger.writeEvent(string.Format("StoreHandler: OpenStore| store \'{0}\' added successfully", contactDetails.Name));
            return newStore;
        }


        /*TODO: next version
         public bool CloseStore()
         {
             return false;
         }*/

        public bool EditStoreContactDetails(Guid storeID, Dictionary<StoresUtils.StoreEditContactDetails, object> detailsToEdit,
                                            MarketDbContext context)
        {
            if(TryGetStore(storeID, out Store store, context))
            {
                if (detailsToEdit.TryGetValue(StoresUtils.StoreEditContactDetails.name, out object val))
                {
                    string nameToChange = (string)val;
                    if (!store.ContactDetails.Name.Equals(nameToChange))
                    {
                        if (TryGetStore(nameToChange, out _, context))
                        {
                            throw new StoreAlreadyExistException(nameToChange);
                        }
                    }

                }
                return store.EditStoreContactDetails(detailsToEdit, context);
            }
            else
            {
                throw new StoreNotFoundException(storeID);
            }
        }



        public Store GetStoreById(Guid storeID, MarketDbContext context)
        {
            if (TryGetStore(storeID, out Store store, context))
            {
                return store;
            }

            throw new StoreNotFoundException(storeID);
        }

        public Store GetStoreByName(string storeName, MarketDbContext context)
        {
            if (TryGetStore(storeName, out Store result, context))
            {
                return result;
            }
            else
            {
                throw new StoreNotFoundException(storeName);
            }
        }

        public double GetStoreRankById(Guid storeID, MarketDbContext context)
        {
            if (TryGetStore(storeID, out Store store, context))
            {
                return store.Rank;
            }

            throw new StoreNotFoundException(storeID);
        }

        /*        //NOT TESTED:
                public double GetStoreRankByName(string name)
                {

                    foreach (KeyValuePair<Guid, Store> entry in stores)
                    {
                        if (entry.Value.ContactDetails.Name.Equals(name))
                        {
                            return entry.Value.Rank;
                        }
                    }

                    throw new StoreNotFoundException(name);
                }*/

        public void SetStoreRankById(Guid storeID, double rank, MarketDbContext context)
        {
            if (!TryGetStore(storeID, out Store store, context))
            {
                throw new StoreNotFoundException(storeID);
            }
            store.Rank = rank;
            context.SaveChanges();
            return;
        }

        /*        //NOT TESTED
                public double GetStoreRankByName(string name)
                {

                    foreach (KeyValuePair<Guid, Store> entry in stores)
                    {
                        if (entry.Value.ContactDetails.Name.Equals(name))
                        {
                            return entry.Value.Rank;
                        }
                    }

                    throw new StoreNotFoundException(name);
                }*/

        public ReadOnlyCollection<Store> Stores(MarketDbContext context)
        {
            List<Store> list = GetAllStores(context);
            Logger.writeEvent("StoreHandler: Pulling data on all stores");
            return list.AsReadOnly();
        }

        /// <summary>
        /// Search for items in all stores by the "And" of following params: 
        /// name (if provided) &
        /// category (if provided) &
        /// keywords (if provided) &
        /// standing in filters (if provided)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="category"></param>
        /// <param name="keywords"></param>
        /// <param name="filters"></param>
        /// <returns>dictionary of <storeID, readonly collection of items found relevant>, if no results readonly dictionary</storeID></returns>
        public Dictionary<Guid, ReadOnlyCollection<Item>> SearchItems(MarketDbContext context, string name = null,
            string category = null, List<string> keywords = null,
            List<SearchFilter> filters = null)
        {
            Logger.writeEvent("StoreHandler: Searching All Stores");
            Dictionary<Guid, ReadOnlyCollection<Item>> searchResultsByStores = new Dictionary<Guid, ReadOnlyCollection<Item>>() { };
            foreach ( Store store in GetAllStores(context))
            {
                ReadOnlyCollection<Item>  items = store.SearchItems(context, name, category, keywords, filters);
                if(items.Count != 0)
                {
                    searchResultsByStores.Add(store.Id, items);
                }

            }

            return searchResultsByStores;
        }

        /// <summary>
        /// if store id does not exist - throws StoreNotFoundException
        /// </summary>
        /// <param name="storeID"></param>
        /// <returns>StoreInventoryManager of the store/returns>
        public IStoreInventoryManager GetStoreInventoryManager(Guid storeID, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("StoreHandler: fetching Store {0} for Inventory Management", storeID));
            if (!TryGetStore(storeID, out Store sim, context))
            {
                Logger.writeEvent(string.Format("StoreHandler: Store {0} not found", storeID));
                throw new StoreNotFoundException(storeID);
                //add log
            }

            Logger.writeEvent(string.Format("StoreHandler: Store {0} found", storeID));
            return sim;
        }

        /// <summary>
        /// if store id does not exist - throws StoreNotFoundException
        /// </summary>
        /// <param name="storeID"></param>
        /// <returns>IStoreDiscountManager of the store/returns>
        internal IStorePolicyManager GetStoreDiscountManager(Guid storeID, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("StoreHandler: fetching Store {0} for Discount Management", storeID));
            if (!TryGetStore(storeID, out Store sdm, context))
            {
                Logger.writeEvent(string.Format("StoreHandler: Store {0} not found", storeID));
                throw new StoreNotFoundException(storeID);
                //add log
            }

            Logger.writeEvent(string.Format("StoreHandler: Store {0} found", storeID));
            return sdm;
        }

        /// <summary>
        /// if store id does not exist - throws StoreNotFoundException
        /// </summary>
        /// <param name="storeID"></param>
        /// <returns>StoreCertificationManager of the store</returns>
        public IStoreCertificationManager GetStoreCertificationManager(Guid storeID, MarketDbContext context)
        {
            Logger.writeEvent(string.Format("StoreHandler: fetching Store {0} for Certification Management", storeID)); 
            if (!TryGetStore(storeID, out Store scm, context))
            {
                Logger.writeEvent(string.Format("StoreHandler: Store {0} not found", storeID));
                throw new StoreNotFoundException(storeID);
                //add log
            }

            Logger.writeEvent(string.Format("StoreHandler: Store {0} found", storeID));
            return scm;
        }

        internal void ReleaseLockOnStore(Guid storeID)
        {
            Mutex storeLock = storeIdToLock.GetOrAdd(storeID, new Mutex());
            storeLock.ReleaseMutex();
        }

        internal void AquireLockOnStore(Guid storeID)
        {
            Mutex storeLock = storeIdToLock.GetOrAdd(storeID, new Mutex()); //should allways exist
            storeLock.WaitOne();
        }

        internal IEnumerable<Guid> AllStoresIDs(MarketDbContext context)
        {
            return GetAllStores(context).ConvertAll(s => s.Id);        
        }


        private Store GetStoreFromDBByName(string name, MarketDbContext context)
        {
            IQueryable<Store> results = context.Stores.Where(s => s.ContactDetails.Name == name);
            return results.Single();
        }

        private Store GetStoreFromDBByID(Guid storeID, MarketDbContext context)
        {
            return context.Stores.Find(storeID);
        }

        private bool TryGetStore(Guid storeID, out Store store, MarketDbContext context)
        {
            store = GetStoreFromDBByID(storeID, context);
            return !(store is null);
        }

        private List<Store> GetAllStores(MarketDbContext context)
        {
            return context.Stores
                            .Include(s => s.storeInventory.StoreItems)
                            //.Include(s => s.StoreOrders.Select(so => so.)
                            .ToList();
        }

        private bool TryGetStore(string name, out Store store, MarketDbContext context)
        {
            try
            {
                store = GetStoreFromDBByName(name, context);
                return true;
            }
            catch
            {
                store = null;
                return false;
            }
        }

        #region INotificationSubject

        public bool RegisterObserver(NotificationObserver notificationObserver)
        {
            return observers.Add(notificationObserver);
        }

        public Guid GetGuid()
        {
            return Id;
        }

        public bool UnregisterObserver(NotificationObserver notificationObserver)
        {
            return observers.Remove(notificationObserver);
        }

        public void notifyEvent(INotificationEvent notificationEvent, MarketDbContext context)
        {
            foreach (INotificationObserver observer in observers)
            {
                observer.NotifyEvent(notificationEvent, context);
            }
        }


        #endregion
    }
}
