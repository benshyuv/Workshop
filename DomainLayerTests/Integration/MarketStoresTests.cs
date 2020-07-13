using DomainLayer.Exceptions;
using DomainLayer.Market;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using DomainLayer.Stores;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using System.Collections.ObjectModel;
using NotificationsManagment;
using DomainLayer.DbAccess;

namespace DomainLayerTests.Integration
{
    class MarketStoresTests : IMarketFacadeTests
    {
        [SetUp]
        public new void Setup()
        {
            SetupUsers();
            SetupStores();
        }

        [Test]
        public void Store_AddItem_ByOpener_ShouldPass()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            Validate_AddOneItem_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID, item);
        }
        //add test for onwer, not opener

        [Test]
        public void Store_AddItem_ByOwner_NotOpener_ShouldPass()
        {
            AddFirstOwnerToStoreByOpener();
           
            Guid sessionID = Guid.NewGuid();
            LoginSessionSuccess(sessionID, FIRST_OWNER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(sessionID);

            Validate_AddOneItem_FIRST_STORE_LegalUserForOp(sessionID, item);
        }

        [Test]
        public void Store_AddItem_ByNewManager_ShouldFail()
        {
            AppointLoginManager(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OPENER_USERNAME, STORE_MANAGER_USERNAME);

            string jsonAnswer = AddItemError(REGISTERED_SESSION_ID, FIRST_STORE_ID, "newitem", 33, JsonConvert.SerializeObject(stringCategories1), 20.55);

            PermissionException e = JsonConvert.DeserializeObject<PermissionException>(jsonAnswer);
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.ToLower().Contains("not permitted"));

        }

        [Test]
        public void Store_AddItemWithSameNameTwice_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            string jsonAnswer = AddItemError(REGISTERED_SESSION_ID, FIRST_STORE_ID, "newitem", 33, JsonConvert.SerializeObject(stringCategories1), 20.55);
            ItemAlreadyExistsException e = JsonConvert.DeserializeObject<ItemAlreadyExistsException>(jsonAnswer);

            Assert.AreEqual("Item with name newitem already exists", e.Message);
        }

        [Test]
        public void Store_AddItemWithSameNameTwice_ByOwner_NotOpener_ShouldFail()
        {
            AddFirstOwnerToStoreByOpener();

            Guid sessionID = Guid.NewGuid();
            LoginSessionSuccess(sessionID, FIRST_OWNER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(sessionID);

            string jsonAnswer = AddItemError(sessionID, FIRST_STORE_ID, "newitem", 33, JsonConvert.SerializeObject(stringCategories1), 20.55);
            ItemAlreadyExistsException e = JsonConvert.DeserializeObject<ItemAlreadyExistsException>(jsonAnswer);

            Assert.AreEqual("Item with name newitem already exists", e.Message);
        }

        [Test]
        public void Store_DeleteItem_ByOpener_ShouldPass()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            Store_DeleteItem_ShouldPass(REGISTERED_SESSION_ID, item);
        }

        [Test]
        public void Store_DeleteItem_ByOwner_NotOpener_ShouldPass()
        {
            AddFirstOwnerToStoreByOpener();

            Guid sessionID = Guid.NewGuid();
            LoginSessionSuccess(sessionID, FIRST_OWNER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(sessionID);

            Store_DeleteItem_ShouldPass(sessionID, item);
        }

        [Test]
        public void Store_DeleteItem_ByGuest_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);


            Guid sessionID = Guid.NewGuid();


            string jsonAnswer = DeleteItemError(sessionID, FIRST_STORE_ID, item.Id);

            UserStateException e = JsonConvert.DeserializeObject<UserStateException>(jsonAnswer);
            Assert.IsNotNull(e);
            Assert.AreEqual("Illegal action: Get UserID for user state Guest", e.Message);
        }


        [Test]
        public void Store_DeleteItem_Twice_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            Store_DeleteItem_Twice_ShouldFail(REGISTERED_SESSION_ID, item);
        }
        //add test for onwer not opener
        //add test for guest
        //add test for registered not related to store

        [Test]
        public void Store_DeleteItem_NoSuchItem_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Store_DeleteItem_NoSuchItem_ShouldFail(REGISTERED_SESSION_ID);
        }
        //add test for onwer not opener
        //add test for guest
        //add test for registered not related to store


        [Test]
        public void Store_EditItem_JustCategories_ByOpener_ShouldPass()
        {
            string[] newCategories = new string[1] { "newcat" };

            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);


            Store_EditItem_JustValidCategories_LegalUserForOp(REGISTERED_SESSION_ID, item.Id , newCategories);
        }
        //add test for onwer not opener

        [Test]
        public void Store_EditItem_ZeroCategories_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);
            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            string jsonAnswer = EditItemError(REGISTERED_SESSION_ID, FIRST_STORE_ID, item.Id, amount: 3, rank: null, price: 3.44, name: null, categories: JsonConvert.SerializeObject(new string[0]), keyWords: null);

            InvalidOperationOnItemException e = JsonConvert.DeserializeObject<InvalidOperationOnItemException>(jsonAnswer);
            Assert.AreEqual("EditItem: item must have at least one category", e.Message);
        }
        //add test for onwer not opener

        [Test]
        public void Store_EditItem_NoSuchItem_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Guid fakeId = Guid.NewGuid();
            string jsonAnswer = EditItemError(REGISTERED_SESSION_ID, FIRST_STORE_ID, fakeId, amount: 3, rank: null, price: 3.44, name: null, categories: null, keyWords: null);

            ItemNotFoundException e = JsonConvert.DeserializeObject<ItemNotFoundException>(jsonAnswer);
            Assert.AreEqual("Invalid Item id: " + fakeId, e.Message);
        }
        //add test for onwer not opener
        //add test for guest
        //add test for registered not related to store

        [Test]
        public void Store_EditItem_JustAmount_ByOpener_ShouldPass()
        {

            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            EditItemSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, item.Id, amount: 20, rank: null, price: null, name: null, categories: null, keyWords: null);

            SearchItemsSuccess(REGISTERED_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> result, filterItemRank: null, filterMinPrice: null, filterMaxPrice: null, filterStoreRank: null, name: "newitem");

            item = result[FIRST_STORE_ID][0];
            Assert.AreEqual(20, item.Amount);
        }
        //add test for onwer not opener

        [Test]
        public void Store_EditItem_JustNegativeAmount_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item item = AddOneItemTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);

            string jsonAnswer = EditItemError(REGISTERED_SESSION_ID, FIRST_STORE_ID, item.Id, amount: -5, rank: null, price: null, name: null, categories: null, keyWords: null);
            InvalidOperationOnItemException e = JsonConvert.DeserializeObject<InvalidOperationOnItemException>(jsonAnswer);
            Assert.AreEqual("EditItem: item must have an amount >= 0", e.Message);

        }
        //add test for onwer not opener

        [Test]
        public void Store_EditItem_NameAlreadyExist_ByOpener_ShouldFail()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Item[] items = AddTwoItemsTo_FIRST_STORE_LegalUserForOp(REGISTERED_SESSION_ID);


            string jsonAnswer = EditItemError(REGISTERED_SESSION_ID, FIRST_STORE_ID, items[0].Id, amount: null, rank: null, price: null, name: "newitem two", categories: null, keyWords: null);
            InvalidOperationOnItemException e = JsonConvert.DeserializeObject<InvalidOperationOnItemException>(jsonAnswer);
            Assert.AreEqual("Item with name "+"newitem two"+ " already exists", e.Message);

        }
        //add test for onwer not opener

        [Test]
        public void SearchFacade_GetAllStoresInformation_ShouldPass()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Guid second_sessionId = Guid.NewGuid();
            LoginSessionSuccess(second_sessionId, SECOND_OPENER_USERNAME);

            Item[] items = Create2StoresItemsForSearchTests(REGISTERED_SESSION_ID, second_sessionId);

            LogoutSessionSuccess(REGISTERED_SESSION_ID);
            LogoutSessionSuccess(second_sessionId);

            Guid guest_session = Guid.NewGuid();

            Validate_GetAllStoresInformation(guest_session, items);

            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);
            Validate_GetAllStoresInformation(REGISTERED_SESSION_ID, items);
        }
        [Test]
        public void SearchFacade_GetStoresInformationByID_ShouldPass()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Guid second_sessionId = Guid.NewGuid();
            LoginSessionSuccess(second_sessionId, SECOND_OPENER_USERNAME);

            Item[] items = Create2StoresItemsForSearchTests(REGISTERED_SESSION_ID, second_sessionId);

            LogoutSessionSuccess(REGISTERED_SESSION_ID);
            LogoutSessionSuccess(second_sessionId);

            Guid guest_session = Guid.NewGuid();

            Validate_GetStoreInformationByID(guest_session, items);

            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);
            Validate_GetStoreInformationByID(guest_session, items);
        }

        

        [Test]
        public void SearchFacade_GetAllStoresInformation_2Stores_Guest_ShouldPass()
        {
            GetAllStoresInformationSuccess(GUEST_SESSION_ID, out ReadOnlyCollection<Store> stores);

            Assert.AreEqual(2, stores.Count);
        }

        [Test]
        public void SearchFacade_GetAllStoresInformation_2Stores_LoggedInBUYER_ShouldPass()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, BUYER_USERNAME);

            GetAllStoresInformationSuccess(REGISTERED_SESSION_ID, out ReadOnlyCollection<Store> stores);

            Assert.AreEqual(2, stores.Count);
        }

        [Test]
        public void SearchItems_RequestAllItems_Guest_ShouldPass()
        {
            Item[] items = SetUpMarket_ForSearchItemsTests();

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results, filterItemRank: null,
                                    filterMinPrice: null,
                                    filterMaxPrice: null,
                                    filterStoreRank: null);

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[FIRST_STORE_ID];
            ReadOnlyCollection<Item> resultStore1 = results[SECOND_STORE_ID];


            Assert.AreEqual(3, resultStore.Count);
            List<Guid> itemIDs = new List<Item>(resultStore).ConvertAll(i => i.Id);
            Assert.IsTrue(itemIDs.Contains(items[0].Id));
            Assert.IsTrue(itemIDs.Contains(items[1].Id));
            Assert.IsTrue(itemIDs.Contains(items[2].Id));


            Assert.AreEqual(4, resultStore1.Count);
            itemIDs = new List<Item>(resultStore1).ConvertAll(i => i.Id);
            Assert.IsTrue(itemIDs.Contains(items[3].Id));
            Assert.IsTrue(itemIDs.Contains(items[4].Id));
            Assert.IsTrue(itemIDs.Contains(items[5].Id));
            Assert.IsTrue(itemIDs.Contains(items[6].Id));

        }

        [Test]
        public void SearchItems_ByName_NameCorrect_Guest_ShouldPass()
        {
            Item[] items = SetUpMarket_ForSearchItemsTests();

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                                filterItemRank: null,
                                filterMinPrice: null,
                                filterMaxPrice: null,
                                filterStoreRank: null,
                                name: "item one");

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[FIRST_STORE_ID];
            ReadOnlyCollection<Item> resultStore1 = results[SECOND_STORE_ID];


            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(items[0].Id, resultStore[0].Id);

            Assert.AreEqual(1, resultStore1.Count);
            Assert.AreEqual(items[3].Id, resultStore1[0].Id);
        }

        [Test]
        public void SearchItems_ByCategory_StoreRankFilterItemRankFilter_ShouldPass()
        {
            Item[] items = SetUpMarket_ForSearchItemsTests();

            //Edit rank of item from first store
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            EditItemSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, items[1].Id, amount: null, rank: 4.55, price: null, name: null, categories: null, keyWords: null);
            LogoutSessionSuccess(REGISTERED_SESSION_ID);


            //Edit rank of item from first store
            LoginSessionSuccess(REGISTERED_SESSION_ID, SECOND_OPENER_USERNAME);
            EditItemSuccess(REGISTERED_SESSION_ID, SECOND_STORE_ID, items[4].Id, amount: null, rank: 4.55, price: null, name: null, categories: null, keyWords: null);
            LogoutSessionSuccess(REGISTERED_SESSION_ID);

            //start search test

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                    filterItemRank: 4.50,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: 0,
                    category: "cat2");

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[FIRST_STORE_ID];
            ReadOnlyCollection<Item> resultStore1 = results[SECOND_STORE_ID];


            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(items[1].Id, resultStore[0].Id);

            Assert.AreEqual(1, resultStore1.Count);
            Assert.AreEqual(items[4].Id, resultStore1[0].Id);
        }

        [Test]
        public void SearchItems_ByCategory_StoreRankFilterItemRankFilter_ShouldFail()
        {
            Item[] items = SetUpMarket_ForSearchItemsTests();

            //Edit rank of item from first store
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            EditItemSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, items[1].Id, amount: null, rank: 4.55, price: null, name: null, categories: null, keyWords: null);
            LogoutSessionSuccess(REGISTERED_SESSION_ID);

            //start search test

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                    filterItemRank: 6,
                    filterMinPrice: null,
                    filterMaxPrice: null,
                    filterStoreRank: 0,
                    category: "cat2");

            Assert.AreEqual(0, results.Keys.Count);

        }

        [Test]
        public void SearchItems_ByKeyWords_ShouldPass()
        {
            Item[] items = SetUpMarket_ForSearchItemsTests();

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                                        filterItemRank: null,
                                        filterMinPrice: null,
                                        filterMaxPrice: null,
                                        filterStoreRank: null,
                                        keywords: JsonConvert.SerializeObject(new List<string>() { "word50" }));

            Assert.AreEqual(1, results.Keys.Count);
            Assert.AreEqual(items[6].Id, results[SECOND_STORE_ID][0].Id);

        }

        [Test]
        public void SearchItems_ByKeyWords_ShouldFail()
        {
            SetUpMarket_ForSearchItemsTests();

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                                        filterItemRank: null,
                                        filterMinPrice: null,
                                        filterMaxPrice: null,
                                        filterStoreRank: null,
                                        keywords: JsonConvert.SerializeObject(new List<string>() { "unknown keyword" }));

            Assert.AreEqual(0, results.Keys.Count);
        }

        [Test]
        public void SearchItems_ByName_SpellMistake_ShouldPass()
        {
            SetUpMarket_ForSearchItemsTests();

            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                                        filterItemRank: null,
                                        filterMinPrice: null,
                                        filterMaxPrice: null,
                                        filterStoreRank: null,
                                        name: "ietm one");

            Assert.AreEqual(2, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[FIRST_STORE_ID];
            ReadOnlyCollection<Item> resultStore1 = results[SECOND_STORE_ID];


            Assert.AreEqual("item one", resultStore[0].Name);
            Assert.AreEqual("item one", resultStore1[0].Name);

            Assert.AreEqual(1, resultStore.Count);
            Assert.AreEqual(1, resultStore1.Count);
        }

        [Test]
        public void SearchItems_ByAllConditionsAndFilters_ShouldPass()
        {
            TestAllConditionsAndFilters("item four", "item four");
        }

        [Test]
        public void SearchItems_ByAllConditionsAndFilters_SpellMistake_ShouldPass()
        {
            TestAllConditionsAndFilters("itam four", "item four");
        }

        private Item AddOneItemTo_FIRST_STORE_LegalUserForOp(Guid session_id)
        {
            AddItemSuccess(session_id, out Item item, FIRST_STORE.Id, "newitem", 2, JsonConvert.SerializeObject(stringCategories1), 3.55);
            return item;
        }

        private Item[] AddTwoItemsTo_FIRST_STORE_LegalUserForOp(Guid session_id)
        {
            Item item1 = AddOneItemTo_FIRST_STORE_LegalUserForOp(session_id);
            AddItemSuccess(session_id, out Item item2, FIRST_STORE.Id, "newitem two", 30, JsonConvert.SerializeObject(stringCategories1), 50);

            Item[] items = new Item[2] { item1, item2 };
            return items;
        }

        private void Store_DeleteItem_Twice_ShouldFail(Guid session_id, Item item)
        {
            GetAllStoresInformationSuccess(session_id, out ReadOnlyCollection<Store> stores);

            foreach (Store s in stores)
            {
                if (s.Id.Equals(FIRST_STORE_ID))
                {
                    Assert.DoesNotThrow(() => DeleteItemSuccess(session_id, FIRST_STORE_ID, item.Id));

                    string jsonAnswer = DeleteItemError(session_id, FIRST_STORE_ID, item.Id);
                    ItemNotFoundException e = JsonConvert.DeserializeObject<ItemNotFoundException>(jsonAnswer);
                    Assert.AreEqual("Invalid Item id: " + item.Id, e.Message);
                    break;
                }
            }
        }

        private void Store_DeleteItem_NoSuchItem_ShouldFail(Guid session_id)
        {
            Guid fakeId = Guid.NewGuid();

            string jsonAnswer = DeleteItemError(session_id, FIRST_STORE_ID, fakeId);
            ItemNotFoundException e = JsonConvert.DeserializeObject<ItemNotFoundException>(jsonAnswer);
            Assert.AreEqual("Invalid Item id: " + fakeId, e.Message);
        }

        private void Store_DeleteItem_ShouldPass(Guid session_id, Item item)
        {
            GetAllStoresInformationSuccess(session_id, out ReadOnlyCollection<Store> stores);

            foreach (Store s in stores)
            {
                if (s.Id.Equals(FIRST_STORE_ID))
                {
                    ReadOnlyCollection<Item> items = s.GetStoreItems();
                    Assert.IsTrue(items.Count == 1);

                    Assert.DoesNotThrow(() => DeleteItemSuccess(session_id, FIRST_STORE_ID, item.Id));
                    break;
                }
            }

            GetAllStoresInformationSuccess(session_id, out stores);
            foreach (Store s in stores)
            {
                if (s.Id.Equals(FIRST_STORE_ID))
                {
                    Assert.AreEqual(0, s.GetStoreItems().Count);
                    break;
                }
            }
        }

        private void Validate_AddOneItem_FIRST_STORE_LegalUserForOp(Guid session_id, Item item)
        {
            Assert.AreEqual(FIRST_STORE_ID, item.StoreId);
            Assert.AreEqual("newitem", item.Name);
            Assert.AreEqual(hashCategories1.Count, item.Categories.Count);
            foreach (string name in hashCategories1)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(2, item.Amount);
            Assert.AreEqual(3.55, item.Price);
            Assert.AreEqual(0, item.Rank);
            Assert.AreEqual(new HashSet<string>(), item.Keywords);

            GetAllStoresInformationSuccess(session_id, out ReadOnlyCollection<Store> stores);

            foreach (Store s in stores)
            {
                if (s.Id.Equals(FIRST_STORE_ID))
                {
                    Assert.IsTrue(s.GetStoreItems().Contains(item));
                    Assert.AreEqual(1, s.GetStoreItems().Count);
                }
            }
        }

        private void Store_EditItem_JustValidCategories_LegalUserForOp(Guid session_id,Guid item_id, string[] newCategories)
        {
            EditItemSuccess(session_id, FIRST_STORE_ID, item_id, amount: null, rank: null, price: null, name: null, categories: JsonConvert.SerializeObject(newCategories), keyWords: null);

            SearchItemsSuccess(REGISTERED_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> result, filterItemRank: null, filterMinPrice: null, filterMaxPrice: null, filterStoreRank: null, name: "newitem");

            Item item = result[FIRST_STORE_ID][0];


            Assert.AreEqual("newitem", item.Name);
            Assert.AreEqual(2, item.Amount);
            Assert.AreEqual(newCategories.Length, item.Categories.Count);
            foreach (string name in newCategories)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(0, item.Rank);
            Assert.AreEqual(3.55, item.Price);
        }

        private Item[] Create2StoresItemsForSearchTests(Guid session_id1, Guid session_id2)
        {
            Item[] items = new Item[7];
            //store items:
            AddItemSuccess(session_id1, out Item item, FIRST_STORE_ID, "item one", 20, JsonConvert.SerializeObject(new string[] { "cat1" }), 20.4, JsonConvert.SerializeObject(new string[] { "word1" }));
            items[0] = item;
            AddItemSuccess(session_id1, out item, FIRST_STORE_ID, "item two", 30, JsonConvert.SerializeObject(new string[] { "cat2" }), 20.4, JsonConvert.SerializeObject(new string[] { "word2" }));
            items[1] = item;
            AddItemSuccess(session_id1, out item, FIRST_STORE_ID, "item three", 200, JsonConvert.SerializeObject(new string[] { "cat1", "cat2" }), 20.4, JsonConvert.SerializeObject(new string[] { "word3" }));
            items[2] = item;

            //store1 items:
            AddItemSuccess(session_id2, out item, SECOND_STORE_ID, "item one", 20, JsonConvert.SerializeObject(new string[] { "cat1" }), 20.4, JsonConvert.SerializeObject(new string[] { "word1" }));
            items[3] = item;
            AddItemSuccess(session_id2, out item, SECOND_STORE_ID, "item two", 30, JsonConvert.SerializeObject(new string[] { "cat2" }), 10, JsonConvert.SerializeObject(new string[] { "word2" }));
            items[4] = item;
            AddItemSuccess(session_id2, out item, SECOND_STORE_ID, "item four", 500, JsonConvert.SerializeObject(new string[] { "cat1", "cat2" }), 20.4, JsonConvert.SerializeObject(new string[] { "word3", "word4" }));
            items[5] = item;
            AddItemSuccess(session_id2, out item, SECOND_STORE_ID, "item five", 700, JsonConvert.SerializeObject(new string[] { "cat1", "cat2", "3at3" }), 50, JsonConvert.SerializeObject(new string[] { "word4", "word50" }));
            items[6] = item;

            return items;
        }

        private void Validate_GetAllStoresInformation(Guid session_id, Item[] items)
        {
            GetAllStoresInformationSuccess(session_id, out ReadOnlyCollection<Store> stores);

            foreach (Store s in stores)
            {
                if (s.Id.Equals(FIRST_STORE_ID))
                {
                    Assert.AreEqual(FIRST_STORE_NAME, s.ContactDetails.Name);
                    Assert.AreEqual(3, s.GetStoreItems().Count);
                    Assert.NotNull(s.GetItemById(items[0].Id));
                    Assert.NotNull(s.GetItemById(items[1].Id));
                    Assert.NotNull(s.GetItemById(items[2].Id));
                }
                else
                {//SECOND_STORE_IT
                    Assert.AreEqual(SECOND_STORE_NAME, s.ContactDetails.Name);
                    Assert.AreEqual(4, s.GetStoreItems().Count);
                    Assert.NotNull(s.GetItemById(items[3].Id));
                    Assert.NotNull(s.GetItemById(items[4].Id));
                    Assert.NotNull(s.GetItemById(items[5].Id));
                    Assert.NotNull(s.GetItemById(items[6].Id));
                }
            }
        }

        private void Validate_GetStoreInformationByID(Guid session_id, Item[] items)
        {
            GetAllStoresInformationSuccess(session_id, out ReadOnlyCollection<Store> stores);
            foreach (Store s in stores)
            {
                Store storeByID = GetStoreInformationByIDSuccess(session_id, s.Id);
                if (storeByID.Id.Equals(FIRST_STORE_ID))
                {
                    Assert.AreEqual(FIRST_STORE_NAME, storeByID.ContactDetails.Name);
                    Assert.AreEqual(3, storeByID.GetStoreItems().Count);
                    Assert.NotNull(storeByID.GetItemById(items[0].Id));
                    Assert.NotNull(storeByID.GetItemById(items[1].Id));
                    Assert.NotNull(storeByID.GetItemById(items[2].Id));
                }
                else
                {//SECOND_STORE_IT
                    Assert.AreEqual(SECOND_STORE_NAME, s.ContactDetails.Name);
                    Assert.AreEqual(4, storeByID.GetStoreItems().Count);
                    Assert.NotNull(storeByID.GetItemById(items[3].Id));
                    Assert.NotNull(storeByID.GetItemById(items[4].Id));
                    Assert.NotNull(storeByID.GetItemById(items[5].Id));
                    Assert.NotNull(storeByID.GetItemById(items[6].Id));
                }
            }
        }

        

        private Item[] SetUpMarket_ForSearchItemsTests()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);

            Guid second_sessionId = Guid.NewGuid();
            LoginSessionSuccess(second_sessionId, SECOND_OPENER_USERNAME);

            Item[] items = Create2StoresItemsForSearchTests(REGISTERED_SESSION_ID, second_sessionId);

            LogoutSessionSuccess(REGISTERED_SESSION_ID);
            LogoutSessionSuccess(second_sessionId);

            return items;
        }

        private void TestAllConditionsAndFilters(string nameToSearch, string nameToFind)
        {
            Item[] items = SetUpMarket_ForSearchItemsTests();

            Guid GUEST_SESSION_ID = Guid.NewGuid();


            SearchItemsSuccess(GUEST_SESSION_ID, out Dictionary<Guid, ReadOnlyCollection<Item>> results,
                                        filterItemRank: 0,
                                        filterMinPrice: 20.4,
                                        filterMaxPrice: 20.4,
                                        filterStoreRank: 0,
                                        name: nameToSearch,
                                        category: "cat2",
                                        keywords: JsonConvert.SerializeObject(new List<string>() { "word3" })
                                        );

            Assert.AreEqual(1, results.Keys.Count);

            ReadOnlyCollection<Item> resultStore = results[SECOND_STORE_ID];

            Assert.AreEqual(nameToFind, resultStore[0].Name);

            Assert.AreEqual(1, resultStore.Count);
        }


        private void AddFirstOwnerToStoreByOpener()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME);
            AppointOwnerSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_OWNER_USERNAME);
            LogoutSessionSuccess(REGISTERED_SESSION_ID);
        }

    }
}
