using DomainLayer.DbAccess;
using DomainLayer.Stores;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayerTests.UnitTests.Data
{
    public class DataForTests
    {
        public static Store[] CreateStoresForSearchTests(StoreHandler storeHandler, MarketDbContext context)
        {
            RegisteredUser owner = new RegisteredUser("TEMP", new byte[] { });
            context.Users.Add(owner);
            context.SaveChanges();
            Guid Owner = owner.ID;
            Store[] stores = new Store[4];

            StoreContactDetails contactDetails = CreateTestContactDetails();
            contactDetails.Name = "store";
            storeHandler.OpenStore(contactDetails, Owner, context);
            Store store = storeHandler.GetStoreByName(contactDetails.Name, context);
            stores[0] = store;

            StoreContactDetails contactDetails1 = CreateTestContactDetails();
            contactDetails1.Name = "store1";
            storeHandler.OpenStore(contactDetails1, Owner, context);
            Store store1 = storeHandler.GetStoreByName(contactDetails1.Name, context);
            stores[1] = store1;

            StoreContactDetails contactDetails2 = CreateTestContactDetails();
            contactDetails2.Name = "store2";
            storeHandler.OpenStore(contactDetails2, Owner, context);
            Store store2 = storeHandler.GetStoreByName(contactDetails2.Name, context);
            stores[2] = store2;

            StoreContactDetails contactDetails3 = CreateTestContactDetails();
            contactDetails3.Name = "store3";
            storeHandler.OpenStore(contactDetails3, Owner, context);
            Store store3 = storeHandler.GetStoreByName(contactDetails3.Name, context);
            stores[3] = store3;

            //store items:
            IStoreInventoryManager inventoryManager = storeHandler.GetStoreInventoryManager(store.Id, context);
            inventoryManager.AddItem("item one", 20, new HashSet<string>() { "cat1" }, 20.4, context, new HashSet<string>() { "word1" });
            inventoryManager.AddItem("item two", 30, new HashSet<string>() { "cat2" }, 20.4, context, new HashSet<string>() { "word2" });
            inventoryManager.AddItem("item three", 200, new HashSet<string>() { "cat1", "cat2" }, 20.4, context, new HashSet<string>() { "word3" });


            //store1 items:
            IStoreInventoryManager inventoryManager1 = storeHandler.GetStoreInventoryManager(store1.Id, context);
            inventoryManager1.AddItem("item one", 20, new HashSet<string>() { "cat1" }, 20.4, context, new HashSet<string>() { "word1" });
            inventoryManager1.AddItem("item two", 30, new HashSet<string>() { "cat2" }, 10, context, new HashSet<string>() { "word2" });
            inventoryManager1.AddItem("item four", 500, new HashSet<string>() { "cat1", "cat2" }, 20.4, context, new HashSet<string>() { "word3", "word4" });
            inventoryManager1.AddItem("item five", 700, new HashSet<string>() { "cat1", "cat2", "cat3" }, 50, context, new HashSet<string>() { "word4", "word50" });


            //store2 items:
            IStoreInventoryManager inventoryManager2 = storeHandler.GetStoreInventoryManager(store2.Id, context);
            inventoryManager2.AddItem("item one", 30, new HashSet<string>() { "cat1" }, 20.5, context, new HashSet<string>() { "word1" });
            inventoryManager2.AddItem("item two", 300, new HashSet<string>() { "cat2" }, 10, context, new HashSet<string>() { "word2" });
            inventoryManager2.AddItem("item three", 200, new HashSet<string>() { "cat1", "cat2" }, 20.4, context, new HashSet<string>() { "word3" });
            inventoryManager2.AddItem("item four", 3000, new HashSet<string>() { "cat1", "cat2" }, 20.4, context, new HashSet<string>() { "word3", "word4" });
            inventoryManager2.AddItem("item five", 30000, new HashSet<string>() { "cat1", "cat2", "cat5" }, 50, context, new HashSet<string>() { });

            //store3 items
            IStoreInventoryManager inventoryManager3 = storeHandler.GetStoreInventoryManager(store3.Id, context);
            inventoryManager3.AddItem("item twenty", 20, new HashSet<string>() { "cat20" }, 200, context, new HashSet<string>() { "word50" });
            return stores;
        }

        public static StoreContactDetails CreateTestContactDetails()
        {
            return new StoreContactDetails("store", "store@gmail.com", "Address", "0544444444", "888-444444/34", "Leumi", "Store description");
        }
    }
}
