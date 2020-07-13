using DomainLayer.DbAccess;
using DomainLayer.Market.SearchEngine;
using DomainLayer.Stores;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using Effort;
using Effort.Provider;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DomainLayerTests.UnitTests
{
    public class SearchFacadeTests
    {
        SearchFacade searchFacade;
        StoreHandler storeHandler;
        Guid Owner;
        EffortConnection inMemoryConnection;

        [SetUp]
        public void SetUp()
        {
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();
            storeHandler = new StoreHandler();
            searchFacade = new SearchFacade(storeHandler);
            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            Owner = owner.ID;
            context.SaveChanges();
        }

        [Test]
        public void SearchItems_RequestAllItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: null);

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
        public void SearchItems_ByName_NameCorrect_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(
                                        context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: null,
                    name: "item one");

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
        public void SearchItems_ByCategory_StoreRankFilterItemRankFilter_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            storeHandler.SetStoreRankById(stores[0].Id, 4.5, context);
            storeHandler.SetStoreRankById(stores[1].Id, 3, context);

            //update rank of item2 of "store"
            ReadOnlyCollection<Item> items_Store = stores[0].GetStoreItems();
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
            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: 6.5,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: 4.5,
                    category: "cat2");

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

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: null,
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
        public void SearchItems_ByName_SpellMistake_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: null,
                    name: "ietm one");

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
        public void SearchItems_ByName2_SpellMistake_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: null,
                    name: "itam one");

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
        public void SearchItems_ByNameAndCategoryAndKeyWords_FilterByPrice_SpellMistake_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: 20.4,
                    filterStoreRank: null,
                    name: "itam one",
                    category: "cat1",
                    keywords: new List<string>() { "word1" });

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];
            ReadOnlyCollection<Item> resultStore1 = results[stores[1].Id];

            Assert.AreEqual("item one", resultStore[0].Name);
            Assert.AreEqual("item one", resultStore1[0].Name);

            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(1, resultStore1.Count);
        }

        [Test]
        public void SearchItems_ByNameAndCategory_FilterByPrice_SpellMistake_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: 11,
                    filterMaxPrice: 20.4,
                    filterStoreRank: null,
                    name: "itey two",
                    category: "cat2");

            Assert.AreEqual(1, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];

            Assert.AreEqual("item two", resultStore[0].Name);

            Assert.AreEqual(1, resultStore.Count);
        }

        [Test]
        public void SearchItems_ByNameAndCategory_SpellMistake_2_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            IStoreInventoryManager inventoryManager3 = storeHandler.GetStoreInventoryManager(stores[3].Id, context);
            inventoryManager3.AddItem("apple computer", 300, new HashSet<string>() { "electrical" }, 2000,
                context, new HashSet<string>() { "computers", "electronic" });

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: 11,
                    filterMaxPrice: 2000,
                    filterStoreRank: null,
                    name: "applee compuetr",
                    category: "electrical");

            Assert.AreEqual(1, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[3].Id];

            Assert.AreEqual("apple computer", resultStore[0].Name);

            Assert.AreEqual(1, resultStore.Count);
        }

        [Test]
        public void SearchItems_ByNameAndKeyWords_SpellMistake_3_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            IStoreInventoryManager inventoryManager3 = storeHandler.GetStoreInventoryManager(stores[3].Id, context);
            inventoryManager3.AddItem("apple computer", 300, new HashSet<string>() { "electrical" }, 2000,
                context, new HashSet<string>() { "computers", "electronic" });

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: 11,
                    filterMaxPrice: 2000,
                    filterStoreRank: null,
                    name: "applee compuetr",
                    category: "electrical",
                    keywords: new List<string>() { "computers", "electronic" });

            Assert.AreEqual(1, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[3].Id];

            Assert.AreEqual("apple computer", resultStore[0].Name);

            Assert.AreEqual(1, resultStore.Count);
        }

        [Test]
        public void SearchItems_ByAllConditionsAndFilters_ShouldPass()
        {
            TestAllConditionsAndFilters("item three", "item three");
        }

        [Test]
        public void SearchItems_ByAllConditionsAndFilters_SpellMistake_ShouldPass()
        {
            TestAllConditionsAndFilters("item tthree","item three");
        }


        [Test]
        public void SearchItems_ByNameAndWrongKeyWords_spellMistake_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            IStoreInventoryManager inventoryManager3 = storeHandler.GetStoreInventoryManager(stores[3].Id, context);
            inventoryManager3.AddItem("apple computer", 300, new HashSet<string>() { "electrical" }, 2000,
                context, new HashSet<string>() { "computers", "electronic" });

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(
                                        context: context,
                    filterItemRank: null,
                    filterMinPrice: 11,
                    filterMaxPrice: 2000,
                    filterStoreRank: null,
                    name: "applee compuetr",
                    category: "electrical",
                    keywords: new List<string>() { "computers", "just key word" });

            Assert.AreEqual(0, results.Keys.Count);
        }

        [Test]
        public void SearchItems_ByName_spellMistake_SpellingCannotHelp_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            IStoreInventoryManager inventoryManager3 = storeHandler.GetStoreInventoryManager(stores[3].Id, context);
            inventoryManager3.AddItem("apple computer", 300, new HashSet<string>() { "electrical" }, 2000, context, 
                keyWords: new HashSet<string>() { "computers", "electronic" });

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: 11,
                    filterMaxPrice: 2000,
                    filterStoreRank: null,
                    name: "paplee compuetr",
                    category: "electrical");

            Assert.AreEqual(0, results.Keys.Count);
        }

        [Test]
        public void SearchItems_ByCategory_WithPriceFilter_NoSuchItems_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            SearchFilter priceFilter = new FilterByPrice(null, 3);
            List<SearchFilter> filters = new List<SearchFilter>() { priceFilter };

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: 3,
                    filterStoreRank: null,
                    category: "cat2");

            Assert.AreEqual(0, results.Keys.Count);
        }

        [Test]
        public void SearchItems_ByName_NoSuchName_SpellCheckerShouldNotFindAnswer_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results = 
                searchFacade.SearchItems(context: context,
                    filterItemRank: null,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: null,
                    name: "noSuchName");

            Assert.AreEqual(0, results.Keys.Count);
        }

        [Test]
        public void GetAllStoresInformation_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            ReadOnlyCollection<Store> data = searchFacade.GetAllStoresInformation(context);

            Assert.AreEqual(4, data.Count);

            foreach ( Store store in stores)
            {
                Assert.IsTrue(data.Contains(store));
            }
        }

        [Test]
        public void GetAllStoresInformation_NoStores_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            ReadOnlyCollection<Store> data = searchFacade.GetAllStoresInformation(context);

            Assert.AreEqual(0, data.Count);
        }

        [Test]
        public void GetAllStoresInformation_StoreHasNoItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store store = storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), Owner, context);

            ReadOnlyCollection<Store> data = searchFacade.GetAllStoresInformation(context);

            Assert.AreEqual(1, data.Count);
            Assert.IsTrue(data.Contains(store));
            Assert.AreEqual(0, store.GetStoreItems().Count);
        }

        private void TestAllConditionsAndFilters(string nameToSearch, string nameToFind)
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Store[] stores = DataForTests.CreateStoresForSearchTests(storeHandler, context);

            Dictionary<Guid, ReadOnlyCollection<Item>> results =
                searchFacade.SearchItems(
                                        context: context,
                    filterItemRank: 0,
                    filterMinPrice: 20.4,
                    filterMaxPrice: 20.4,
                    filterStoreRank: 0,
                    name: nameToSearch,
                    category: "cat2",
                    keywords: new List<string>() { "word3" });

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[stores[0].Id];
            ReadOnlyCollection<Item> resultStore2 = results[stores[2].Id];

            Assert.AreEqual(nameToFind, resultStore[0].Name);
            Assert.AreEqual(nameToFind, resultStore2[0].Name);

            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(1, resultStore2.Count);
        }
    }
}
