using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using Effort;
using Effort.Provider;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests
{
    public class StoreHandlerTests
    {
        private Guid Owner;
        StoreHandler storeHandler;
        EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();
            storeHandler = new StoreHandler();
            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            context.SaveChanges();
            Owner = owner.ID;
        }

        [Test]
        public void OpenStore_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store s = null;
            Assert.DoesNotThrow(() => s = storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context));

            Assert.AreEqual("store", s.ContactDetails.Name);
            Assert.AreEqual("store@gmail.com", s.ContactDetails.Email);
            Assert.AreEqual("Address", s.ContactDetails.Address);
            Assert.AreEqual("0544444444", s.ContactDetails.Phone);
            Assert.AreEqual("888-444444/34", s.ContactDetails.BankAccountNumber);
            Assert.AreEqual("Leumi", s.ContactDetails.Bank);
            Assert.AreEqual("Store description", s.ContactDetails.Description);

        }

        [Test]
        public void OpenStore_AndTryAnotherSameOne_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Assert.DoesNotThrow(() => storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context));

            Assert.Throws(Is.TypeOf<StoreAlreadyExistException>()
            .And.Message.EqualTo(string.Format("Store with name {0} already exists", "store")),
            () => storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context));
        }

        [Test]
        public void GetStoreInventoryManager_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            StoreContactDetails storeContactDetails = DataForTests.CreateTestContactDetails();

            storeHandler.OpenStore(storeContactDetails, Owner, context);

            Store store = storeHandler.GetStoreByName("store", context);
            IStoreInventoryManager sim = storeHandler.GetStoreInventoryManager(store.Id, context);

            Assert.AreEqual(store, sim);
        }

        [Test]
        public void GetStoreInventoryManager_NoSuchStore_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Store id: {0}", fakeId)),
           () => storeHandler.GetStoreInventoryManager(fakeId, context));
        }

        [Test]
        public void GetStoreCertificationManager_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            StoreContactDetails storeContactDetails = DataForTests.CreateTestContactDetails();

            storeHandler.OpenStore(storeContactDetails, Owner, context);

            Store store = storeHandler.GetStoreByName("store", context);
            IStoreCertificationManager scm = storeHandler.GetStoreCertificationManager(store.Id, context);

            Assert.AreEqual(store, scm);
        }

        [Test]
        public void GetStoreCertificationManager_NoSuchStore_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Store id: {0}", fakeId)),
           () => storeHandler.GetStoreCertificationManager(fakeId, context));
        }

        [Test]
        public void EditStoreContactDetails_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context);

            Store store = storeHandler.GetStoreByName("store", context);

            Dictionary<StoresUtils.StoreEditContactDetails, object> newDetails = new Dictionary<StoresUtils.StoreEditContactDetails, object>()
            {
                {StoresUtils.StoreEditContactDetails.name, "newStore"},
                {StoresUtils.StoreEditContactDetails.email, "new email"},
                {StoresUtils.StoreEditContactDetails.address, "newAddress"},
                {StoresUtils.StoreEditContactDetails.bankAccountNumber, "888-222222/20"},
                {StoresUtils.StoreEditContactDetails.bank, "Poalim"},
            };

            Assert.IsTrue(storeHandler.EditStoreContactDetails(store.Id, newDetails, context));
            store = storeHandler.GetStoreById(store.Id, context);

            StoreContactDetails storeContactDetails = store.ContactDetails;

            Assert.AreEqual(storeContactDetails.Name, "newStore");
            Assert.AreEqual(storeContactDetails.Email, "new email");
            Assert.AreEqual(storeContactDetails.Address, "newAddress");
            Assert.AreEqual(storeContactDetails.Phone, "0544444444");
            Assert.AreEqual(storeContactDetails.BankAccountNumber, "888-222222/20");
            Assert.AreEqual(storeContactDetails.Bank, "Poalim");
            Assert.AreEqual(storeContactDetails.Description, "Store description");
        }

        [Test]
        public void EditStoreContactDetails_EditNameToExistingStoreName_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            StoreContactDetails storeContactDetails1 = DataForTests.CreateTestContactDetails();
            storeHandler.OpenStore(storeContactDetails1, Owner, context);

            StoreContactDetails storeContactDetails2 = DataForTests.CreateTestContactDetails();
            storeContactDetails2.Name = "store2";
            storeHandler.OpenStore(storeContactDetails2, Owner, context);

            Store store = storeHandler.GetStoreByName("store", context);

            Dictionary<StoresUtils.StoreEditContactDetails, object> newDetails = new Dictionary<StoresUtils.StoreEditContactDetails, object>()
            {
                {StoresUtils.StoreEditContactDetails.name, "store2"},
            };

            Assert.Throws(Is.TypeOf<StoreAlreadyExistException>()
            .And.Message.EqualTo(string.Format("Store with name {0} already exists", "store2")),
            () => storeHandler.EditStoreContactDetails(store.Id, newDetails, context));
        }

        [Test]
        public void EditStoreContactDetails_NoSuchStore_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Store id: {0}", fakeId)),
           () => storeHandler.EditStoreContactDetails(
                       fakeId,
                       new Dictionary<StoresUtils.StoreEditContactDetails, object>()
                       {
                        {StoresUtils.StoreEditContactDetails.name, "store2"},
                       },
                       context)
           );
        }

        [Test]
        public void GetStoreById_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context);

            StoreContactDetails storeContactDetails2 = DataForTests.CreateTestContactDetails();
            storeContactDetails2.Name = "store2";
            storeHandler.OpenStore(storeContactDetails2, Owner, context);

            Store store = storeHandler.GetStoreByName("store", context);

            Assert.AreEqual(store, storeHandler.GetStoreById(store.Id, context));
        }

        [Test]
        public void GetStoreById_NoSuchStore_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context); //just for initialization

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Store id: {0}", fakeId)),
           () => storeHandler.GetStoreById(fakeId, context));
        }

        [Test]
        public void GetStoreByName_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context);

            StoreContactDetails storeContactDetails2 = DataForTests.CreateTestContactDetails();
            storeContactDetails2.Name = "store2";
            storeHandler.OpenStore(storeContactDetails2, Owner, context);

            Store store = storeHandler.GetStoreByName("store", context);

            Assert.IsNotNull(store);
            Assert.AreEqual("store", store.ContactDetails.Name);
        }

        [Test]
        public void GetStoreByName_NoSuchStore_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context); //just for initialization

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid store name: {0}", "NoStoreName")),
            () => storeHandler.GetStoreByName("NoStoreName", context));
        }

        [Test]
        public void GetStoreRankById_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context);
            Store store = storeHandler.GetStoreByName("store", context);

            Assert.AreEqual(store.Rank, storeHandler.GetStoreRankById(store.Id, context));
            
            store.Rank = 6;
            context.SaveChanges();

            Assert.AreEqual(store.Rank, storeHandler.GetStoreRankById(store.Id, context));
        }

        [Test]
        public void GetStoreRankById_NoSuchStore_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Store id: {0}", fakeId)),
           () => storeHandler.GetStoreRankById(fakeId, context));
        }

        [Test]
        public void SetStoreRankById_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context);
            Store store = storeHandler.GetStoreByName("store", context);

            storeHandler.SetStoreRankById(store.Id, 3.33, context);

            store = storeHandler.GetStoreByName("store", context);
            Assert.AreEqual(3.33, store.Rank);
        }

        [Test]
        public void SetStoreRankById_NoSuchStore_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<StoreNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Store id: {0}", fakeId, context)),
           () => storeHandler.SetStoreRankById(fakeId, 4, context));
        }

        [Test]
        public void GetStores_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            StoreContactDetails storeContactDetails = DataForTests.CreateTestContactDetails();
            storeHandler.OpenStore(storeContactDetails, Owner, context);
            Store store = storeHandler.GetStoreByName(storeContactDetails.Name, context);

            Assert.IsTrue(storeHandler.Stores(context).Contains(store));
            Assert.AreEqual(1, storeHandler.Stores(context).Count);

            StoreContactDetails storeContactDetails2 = DataForTests.CreateTestContactDetails();
            storeContactDetails2.Name = "store2";
            storeHandler.OpenStore(storeContactDetails2, Owner, context);
            Store store2 = storeHandler.GetStoreByName(storeContactDetails.Name, context);

            Assert.IsTrue(storeHandler.Stores(context).Contains(store));
            Assert.IsTrue(storeHandler.Stores(context).Contains(store2));
            Assert.AreEqual(2, storeHandler.Stores(context).Count);
        }

        [Test]
        public void GetStores_NoStores_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Assert.AreEqual(0, storeHandler.Stores(context).Count);
        }

        [Test]
        public void SearchItems_RequestAllItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context);

            Assert.AreEqual(4, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];
            ReadOnlyCollection<Item> resultStore1 = results[stores[1].Id];
            ReadOnlyCollection<Item> resultStore2 = results[stores[2].Id];
            ReadOnlyCollection<Item> resultStore3 = results[stores[3].Id];

            Assert.AreEqual(3, resultStore.Count);
            Assert.AreEqual(4, resultStore1.Count);
            Assert.AreEqual(5, resultStore2.Count);
            Assert.AreEqual(1, resultStore3.Count);
        }

        [Test]
        public void SearchItems_ByName_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context, name: "item one");

            Assert.AreEqual(3, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];
            ReadOnlyCollection<Item> resultStore1 = results[stores[1].Id];
            ReadOnlyCollection<Item> resultStore2 = results[stores[2].Id];

            Assert.AreEqual("item one", resultStore[0].Name);
            Assert.AreEqual("item one", resultStore1[0].Name);
            Assert.AreEqual("item one", resultStore2[0].Name);

            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(1, resultStore1.Count);
            Assert.AreEqual(1, resultStore2.Count);
        }

        [Test]
        public void SearchItems_ByName_PriceFilter_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            SearchFilter priceFilter = new FilterByPrice(null, 20.4);
            List<SearchFilter> filters = new List<SearchFilter>() { priceFilter };

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context,
                name: "item one",
                filters: filters);

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];
            ReadOnlyCollection<Item> resultStore1 = results[stores[1].Id];

            Assert.AreEqual("item one", resultStore[0].Name);
            Assert.AreEqual("item one", resultStore1[0].Name);

            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(1, resultStore1.Count);
        }

        [Test]
        public void SearchItems_ByCategory_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context, category: "cat2");

            Assert.AreEqual(3, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];
            ReadOnlyCollection<Item> resultStore1 = results[stores[1].Id];
            ReadOnlyCollection<Item> resultStore2 = results[stores[2].Id];

            Assert.AreEqual("item two", resultStore[0].Name);
            Assert.AreEqual("item three", resultStore[1].Name);

            Assert.AreEqual("item two", resultStore1[0].Name);
            Assert.AreEqual("item four", resultStore1[1].Name);
            Assert.AreEqual("item five", resultStore1[2].Name);

            Assert.AreEqual("item two", resultStore2[0].Name);
            Assert.AreEqual("item three", resultStore2[1].Name);
            Assert.AreEqual("item four", resultStore2[2].Name);
            Assert.AreEqual("item five", resultStore2[3].Name);


            Assert.AreEqual(2, resultStore.Count);
            Assert.AreEqual(3, resultStore1.Count);
            Assert.AreEqual(4, resultStore2.Count);
        }

        [Test]
        public void SearchItems_ByCategory_StoreRankFilterItemRankFilter_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            SearchFilter storeRankFilter = new FilterByStoreRank(4.5);
            SearchFilter itemRankFilter = new FilterByItemRank(6.5);
            List<SearchFilter> filters = new List<SearchFilter>() { storeRankFilter, itemRankFilter };

            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            storeHandler.SetStoreRankById(stores[0].Id, 4.5, context);
            storeHandler.SetStoreRankById(stores[1].Id, 3, context);

            //update rank of item2 of "store"
            ReadOnlyCollection<Item> items_Store  = stores[0].GetStoreItems();
            Guid itemId2 = items_Store[1].Id;

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails_item2 = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.rank, 6.5}
            };

            IStoreInventoryManager storeInventoryManager = storeHandler.GetStoreInventoryManager(stores[0].Id, context);
            storeInventoryManager.EditItem(itemId2, newDetails_item2, context);

            //update rank of item5 of "store1"
            ReadOnlyCollection<Item> items_Store1 = stores[1].GetStoreItems();
            Guid itemId5 = items_Store1[3].Id;

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails_item5 = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.rank, 6.9}
            };

            IStoreInventoryManager storeInventoryManager1 = storeHandler.GetStoreInventoryManager(stores[1].Id, context);
            storeInventoryManager1.EditItem(itemId5, newDetails_item5, context);

            //start search test
            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context, category: "cat2",
                filters: filters);

            Assert.AreEqual(1, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];

            Assert.AreEqual("item two", resultStore[0].Name);

            Assert.AreEqual(1, resultStore.Count);
        }

        [Test]
        public void SearchItems_ByKeyWords_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context,
                keywords: new List<string>() { "word50" });

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore1 = results[stores[1].Id];
            ReadOnlyCollection<Item> resultStore3 = results[stores[3].Id];

            Assert.AreEqual("item five", resultStore1[0].Name);
            Assert.AreEqual("item twenty", resultStore3[0].Name);

            Assert.AreEqual(1, resultStore1.Count);
            Assert.AreEqual(1, resultStore3.Count);
        }

        [Test]
        public void SearchItems_ByName_NoSuchName_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results = storeHandler.SearchItems(context: context, name: "noSuchName");

            Assert.AreEqual(0, results.Keys.Count);
        }
    }
}
