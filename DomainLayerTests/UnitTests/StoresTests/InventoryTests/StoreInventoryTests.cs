using System;
using System.Collections.Generic;
using NUnit.Framework;
using DomainLayer.Stores.Inventory;
using System.Collections.ObjectModel;
using DomainLayer.Exceptions;
using DomainLayer.Stores;
using System.Data.Entity;
using DomainLayer.DbAccess;
using DomainLayerTests.UnitTests.Data;
using DomainLayer.Stores.Discounts;
using Effort.Provider;
using Effort;
using DomainLayer.Users;

namespace DomainLayerTests.UnitTests.StoresTests.InventoryTests
{
    //TODO: add test for edit category + edit keyword
    class StoreInventoryTests
    {
        HashSet<string> categories1;
        HashSet<string> categories2;
        HashSet<string> categories3;
        HashSet<string> keywords1;
        HashSet<string> keywords2;
        HashSet<string> keywords3;
        StoreInventory storeInventory;
        Guid storeID;
        private EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();
            storeID = Guid.NewGuid();
            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            context.SaveChanges();
            Guid Owner = owner.ID;
            Store store = new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), new DiscountPolicy(storeID), Owner, context);
            context.Stores.Add(store);
            context.SaveChanges();
            storeInventory = store.storeInventory;
            categories1 = new HashSet<string>() { "cat1" };
            categories2 = new HashSet<string>() { "cat1", "cat2" };
            categories3 = new HashSet<string>() { "cat1", "cat2", "cat3" };
            keywords1 = new HashSet<string>() { "word1" };
            keywords2 = new HashSet<string>() { "word1", "word2" };
            keywords3 = new HashSet<string>() { "word1", "word2", "word3" };
        }

        [TearDown]
        public void TearDown()
        {
            new MarketDbContext(inMemoryConnection).Init();
        }

        [Test]
        public void AddItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item result = storeInventory.AddItem("newItem", 2, categories1, 3.55, context: context, keywordNames: new HashSet<string>() { "word1" });
            ReadOnlyCollection<Item> items = storeInventory.GetStoreItems();
            Assert.IsNotNull(result);
            Assert.IsTrue(items.Contains(result));
            Assert.AreEqual(1, items.Count);
        }

        [Test]
        public void AddItemWithSameNameTwice_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item result = storeInventory.AddItem("newItem", 2, categories1, 3.55, context: context);

            ReadOnlyCollection<Item> items = storeInventory.GetStoreItems();
            Assert.IsNotNull(result);
            Assert.IsTrue(items.Contains(result));
            Assert.AreEqual(1, items.Count);
            Assert.Throws(Is.TypeOf<ItemAlreadyExistsException>()
                .And.Message.EqualTo(string.Format("Item with name {0} already exists", "newItem")),
              () => storeInventory.AddItem("newItem", 33, categories1, 20.55, context: context));
        }

        [Test]
        public void ItemsHasSameStoreIdAsInventory_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item1 = storeInventory.AddItem("item1", 2, categories1, 3.55, context: context);
            Item item2 = storeInventory.AddItem("item2", 444, categories1, 30, context: context);
            Assert.AreEqual(item1.StoreId, storeID);
            Assert.AreEqual(item2.StoreId, storeID);
        }

        [Test]
        public void DeleteItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            SetUpInvetoryWithOneItem(context);
            ReadOnlyCollection<Item> items = storeInventory.GetStoreItems();

            Assert.AreEqual(1, items.Count);

            Assert.DoesNotThrow(() => storeInventory.DeleteItem(items[0].Id, context));
            Assert.AreEqual(0, storeInventory.GetStoreItems().Count);
        }

        [Test]
        public void DeleteItem_Twice_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);


            storeInventory.DeleteItem(item.Id, context);

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
               .And.Message.EqualTo(string.Format("Invalid Item id: {0}", item.Id)),
             () => storeInventory.DeleteItem(item.Id, context));
        }

        [Test]
        public void DeleteItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
               .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
             () => storeInventory.DeleteItem(fakeId, context));
        }

        [Test]
        public void EditItem_AllEditableDetails_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<string> newCategories = new List<string>() { "newCat" };
            List<string> newKeyWords = new List<string>() { "word1", "word2" };
            Item item = SetUpInvetoryWithOneItem(context);

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.name, "newName"},
                {StoresUtils.ItemEditDetails.amount, 30 },
                {StoresUtils.ItemEditDetails.categories, newCategories },
                {StoresUtils.ItemEditDetails.rank, (double)10 },
                {StoresUtils.ItemEditDetails.price, (double)3 },
                {StoresUtils.ItemEditDetails.keyWords, newKeyWords }

            };

            Assert.DoesNotThrow(() => storeInventory.EditItem(item.Id, newDetails, context));

            item = storeInventory.GetItemById(item.Id);
            Assert.AreEqual("newName", item.Name);
            Assert.AreEqual(30, item.Amount);
            Assert.AreEqual(newCategories.Count, item.Categories.Count);
            foreach (string name in newCategories)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(10, item.Rank);
            Assert.AreEqual(3, item.Price);
            Assert.AreEqual(newKeyWords.Count, item.Keywords.Count);
            foreach (string word in newKeyWords)
            {
                Assert.IsTrue(item.Keywords.Contains(new Keyword(word)));
            }
        }

        [Test]
        public void EditItem_JustCategories_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<string> newCategories = new List<string>() { "newCat" };
            Item item = SetUpInvetoryWithOneItem(context);

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.categories, newCategories }
            };

            Assert.DoesNotThrow(() => storeInventory.EditItem(item.Id, newDetails, context));
            item = storeInventory.GetItemById(item.Id);


            Assert.AreEqual("item1", item.Name);
            Assert.AreEqual(3, item.Amount);
            Assert.AreEqual(newCategories.Count, item.Categories.Count); // the only new value expected
            foreach (string name in newCategories)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(0, item.Rank);
            Assert.AreEqual(10.5, item.Price);
            Assert.AreEqual(new HashSet<string>() { }, item.Keywords);

        }

        [Test]
        public void EditItem_EmptyEditDetails_NoChangeToItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            // storeInventory.AddItem(item1.StoreId(), item1.Name, item1.Amount, item1.Categories, item1.Rank, item1.Price);
            SetUpInvetoryWithOneItem(context);
            ReadOnlyCollection<Item> items = storeInventory.GetStoreItems();
            Item item = items[0];

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>() {};

            Assert.DoesNotThrow(() => storeInventory.EditItem(item.Id, newDetails, context));
            item = storeInventory.GetItemById(item.Id);

            Assert.AreEqual("item1", item.Name);
            Assert.AreEqual(3, item.Amount);
            Assert.AreEqual(categories1.Count, item.Categories.Count);
            foreach (string name in categories1)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(0, item.Rank);
            Assert.AreEqual(10.5, item.Price);
            Assert.AreEqual(new HashSet<Keyword>() { }, item.Keywords);
        }

        [Test]
        public void EditItem_RenameItemName_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.name, "newName" }
            };

            Assert.DoesNotThrow(() => storeInventory.EditItem(item.Id, newDetails, context));
            item = storeInventory.GetItemById(item.Id);

            Assert.AreEqual("newName", item.Name);
            Assert.AreEqual(3, item.Amount);
            Assert.AreEqual(categories1.Count, item.Categories.Count);
            foreach (string name in categories1)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(0, item.Rank);
            Assert.AreEqual(10.5, item.Price);
            Assert.AreEqual(new HashSet<string>() { }, item.Keywords);
        }

        [Test]
        public void EditItem_RenameToExistedItemName_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item1 = SetUpInvetoryWithOneItem(context);

            HashSet<string> newItemCategories = new HashSet<string>() { "newCat" };
            storeInventory.AddItem("newItem", 2, newItemCategories, 2, context: context);


            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.name, "newItem" }
            };

            Assert.Throws(Is.TypeOf<ItemAlreadyExistsException>()
            .And.Message.EqualTo(string.Format("Item with name {0} already exists", "newItem")),
            () => storeInventory.EditItem(item1.Id, newDetails, context));
        }

        [Test]
        public void EditItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();
            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.EditItem(fakeId, new Dictionary<StoresUtils.ItemEditDetails, object>() { }, context));

        }

        [Test]
        public void ReduceItemAmount_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 10, categories1, 3.55, context: context);

            storeInventory.ReduceItemAmount(item.Id, 6, context);

            Assert.AreEqual(4, item.Amount);
        }

        [Test]
        public void ReduceItemAmount_UnavailableAmount1_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 9, categories1, 3.55, context: context);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: Item {0} cannot have non positive amount", item.Id)),
             () => storeInventory.ReduceItemAmount(item.Id, 10, context));
        }

        [Test]
        public void ReduceItemAmount_UnavailableAmount2_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 10, categories1, 3.55, context: context);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: Item {0} cannot have non positive amount", item.Id)),
             () => storeInventory.ReduceItemAmount(item.Id, 20, context));
        }

        [Test]
        public void ReduceItemAmount_NegativeAmount_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 10, categories1, 3.55, context: context);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: amount to reduce is not positive value", item.Id)),
             () => storeInventory.ReduceItemAmount(item.Id, -4, context));
        }

        [Test]
        public void ReduceItemAmount_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.ReduceItemAmount(fakeId, 10, context));
        }

        [Test]
        public  void AddCategoryItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsTrue(storeInventory.AddCategoryItem(item.Id, "cat2", context));
            Assert.AreEqual(2, item.Categories.Count);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));
        }

        [Test]
        public void AddCategoryItem_SameCategoryTwice_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsTrue(storeInventory.AddCategoryItem(item.Id, "cat1", context));
            Assert.AreEqual(1, item.Categories.Count);
            Assert.IsTrue(item.Categories.Contains(new Category("cat1")));
        }

        [Test]
        public void AddCategoryItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.AddCategoryItem(fakeId, "cat", context));
        }

        [Test]
        public void UpdateCategoryItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsTrue(storeInventory.UpdateCategoryItem(item.Id, "cat1", "cat2", context));
            Assert.AreEqual(1, item.Categories.Count);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));

            Assert.IsFalse(item.Categories.Contains(new Category("cat1")));
        }

        [Test]
        public void UpdateCategoryItem_OriginalCategoryDoesNotExist_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsFalse(storeInventory.UpdateCategoryItem(item.Id, "cat33", "cat2", context));
            Assert.AreEqual(0, item.Keywords.Count);
        }

        [Test]
        public void UpdateCategoryItem_UpdatedCategoryAlreadyExists_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddCategoryItem(item.Id, "cat2", context);

            Assert.IsTrue(storeInventory.UpdateCategoryItem(item.Id, "cat1", "cat2", context));

            Assert.AreEqual(1, item.Categories.Count);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));

            Assert.IsFalse(item.Categories.Contains(new Category("cat1")));
        }

        [Test]
        public void UpdateCategoryItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.UpdateCategoryItem(fakeId, "cat", "cat222", context));
        }

        [Test]
        public void DeleteCategoryItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddCategoryItem(item.Id, "cat2", context);

            Assert.IsTrue(storeInventory.DeleteCategoryItem(item.Id, "cat1", context));

            Assert.AreEqual(1, item.Categories.Count);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));
            Assert.IsFalse(item.Categories.Contains(new Category("cat1")));
        }

        [Test]
        // deleting a non existing category should succeeded without any change to categories
        public void DeleteCategoryItem_CategoryDoesNotExist_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsTrue(storeInventory.DeleteCategoryItem(item.Id, "cat2", context));

            Assert.AreEqual(1, item.Categories.Count);
            Assert.IsTrue(item.Categories.Contains(new Category("cat1")));
            Assert.IsFalse(item.Categories.Contains(new Category("cat2")));
        }

        [Test]
        public void DeleteCategoryItem_TheOnlyCategory_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo("Cannot delete the only category of item with id:" + item.Id),
            () => storeInventory.DeleteCategoryItem(item.Id, "cat1", context));
        }

        [Test]
        public void DeleteCategoryItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.DeleteCategoryItem(fakeId, "cat1", context));
        }

        [Test]
        public void AddKeyWordItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsTrue(storeInventory.AddKeyWordItem(item.Id, "word", context));

            Assert.IsTrue(item.Keywords.Contains(new Keyword("word")));
            Assert.AreEqual(1, item.Keywords.Count);
        }

        [Test]
        public void AddKeyWordItem_AddSameKeyWordTwice_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);

            Assert.IsTrue(storeInventory.AddKeyWordItem(item.Id, "word", context));
            Assert.IsTrue(storeInventory.AddKeyWordItem(item.Id, "word", context));

            Assert.IsTrue(item.Keywords.Contains(new Keyword("word")));
            Assert.AreEqual(1, item.Keywords.Count);
        }

        [Test]
        public void AddKeyWordItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.AddKeyWordItem(fakeId, "word", context));
        }

        [Test]
        public void UpdateKeyWordItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddKeyWordItem(item.Id, "word1", context);
            storeInventory.AddKeyWordItem(item.Id, "word2", context);

            Assert.IsTrue(storeInventory.UpdateKeyWordItem(item.Id, "word2", "word3", context));

            Assert.AreEqual(2, item.Keywords.Count);
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word3")));
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word1")));

            Assert.IsFalse(item.Keywords.Contains(new Keyword("word2")));
        }

        [Test]
        public void UpdateKeyWordItem_OriginalKeyWordDoesNotExist_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddKeyWordItem(item.Id, "word1", context);

            Assert.IsFalse(storeInventory.UpdateKeyWordItem(item.Id, "word33", "word2", context));

            Assert.AreEqual(1, item.Keywords.Count);
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word1")));
        }

        [Test]
        public void UpdateKeyWordItem_UpdatedKeyWordAlreadyExists_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddKeyWordItem(item.Id, "word1", context);
            storeInventory.AddKeyWordItem(item.Id, "word2", context);

            Assert.IsTrue(storeInventory.UpdateKeyWordItem(item.Id, "word1", "word2", context));

            Assert.AreEqual(1, item.Keywords.Count);
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word2")));
        }

        [Test]
        public void UpdateKeyWordItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.UpdateKeyWordItem(fakeId, "word1", "word2", context));
        }

        [Test]
        public void DeleteKeyWordItem_ItemHasOnlyOneKeyWord_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddKeyWordItem(item.Id, "word1", context);

            Assert.IsTrue(storeInventory.DeleteKeyWordItem(item.Id, "word1", context));

            Assert.AreEqual(0, item.Keywords.Count);
        }

        [Test]
        public void DeleteKeyWordItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddKeyWordItem(item.Id, "word1", context);
            storeInventory.AddKeyWordItem(item.Id, "word2", context);
            storeInventory.AddKeyWordItem(item.Id, "word3", context);

            Assert.AreEqual(3, item.Keywords.Count);

            Assert.IsTrue(storeInventory.DeleteKeyWordItem(item.Id, "word2", context));

            Assert.AreEqual(2, item.Keywords.Count);
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word1")));
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word3")));
        }

        [Test]
        public void DeleteKeyWordItem_KeyWordDoesNotExist_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = SetUpInvetoryWithOneItem(context);
            storeInventory.AddKeyWordItem(item.Id, "word1", context);

            Assert.IsTrue(storeInventory.DeleteKeyWordItem(item.Id, "keyword", context));
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word1")));

            Assert.IsFalse(item.Keywords.Contains(new Keyword("keyword")));
        }

        [Test]
        public void DeleteKeyWordItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.DeleteKeyWordItem(fakeId, "keyword", context));
        }

        [Test]
        public void SearchItems_ByItemName_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, name: "item1");

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains(items[0]));
        }

        [Test]
        public void SearchItems_ByItemName_ShouldBeZeroResults_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, name: "noSuchItem");

            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SearchItems_ByCategory_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, category: "cat2");


            Assert.AreEqual(4, results.Count);

            Assert.IsFalse(results.Contains(items[0]));
            Assert.IsFalse(results.Contains(items[3]));

            Assert.IsTrue(results.Contains(items[1]));
            Assert.IsTrue(results.Contains(items[2]));
            Assert.IsTrue(results.Contains(items[4]));
            Assert.IsTrue(results.Contains(items[5]));
        }


        [Test]
        public void SearchItems_ByCategory_ShouldBeZeroResults_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, category: "noSuchCategory");

            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SearchItems_ByKeyWords_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            List<string> keywordsForSearch = new List<string>() { "word2" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, keywords: keywordsForSearch);


            Assert.AreEqual(2, results.Count);

            Assert.IsFalse(results.Contains(items[3]));

            Assert.IsTrue(results.Contains(items[4]));
            Assert.IsTrue(results.Contains(items[5]));

        }

        [Test]
        public void SearchItems_ByKeyWords_ShouldBeZeroResults_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, keywords: new List<string>() { "noSuchKeyword" });

            Assert.IsNotNull(results);
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SearchItems_AllSearchConds_NoFilters_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            List<string> keywordsForSearch = new List<string>() { "word3", "word2" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, name: "item6",
                category: "cat2", keywords: keywordsForSearch);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains(items[5]));
        }

        [Test]
        public void SearchItems_AllSearchConds_WithPriceFilter_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);
            SearchFilter priceFilter = new FilterByPrice(11, 12);
            List<SearchFilter> filters = new List<SearchFilter>() { priceFilter };

            List<string> keywordsForSearch = new List<string>() { "word1", "word2" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, name: "item5", category: "cat1",
                keywords: keywordsForSearch, itemFilters: filters);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains(items[4]));
        }

        [Test]
        public void SearchItems_AllSearchConds_NotStandsInPriceFilter_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);
            SearchFilter priceFilter = new FilterByPrice(22, null);
            List<SearchFilter> filters = new List<SearchFilter>() { priceFilter };

            List<string> keywordsForSearch = new List<string>() { "word3", "word2" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, name: "item5", category: "cat2",
                keywords: keywordsForSearch, itemFilters: filters);

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void SearchItems_ByCategoryAndKeywords__ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);

            List<string> keywordsForSearch = new List<string>() { "word1", "word2" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, category: "cat2", keywords: keywordsForSearch);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains(items[4]));
            Assert.IsTrue(results.Contains(items[5]));
        }

        [Test]
        public void SearchItems_ByCategoryAndKeywords_WithItemRankFilterAndPriceFilter_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);
            items[4].Rank = 3.5;
            items[5].Rank = 8;

            SearchFilter rankFilter = new FilterByItemRank(2);
            SearchFilter priceFilter = new FilterByPrice(13, null);
            List<SearchFilter> filters = new List<SearchFilter>() {rankFilter, priceFilter };

            List<string> keywordsForSearch = new List<string>() { "word1" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, category: "cat1",
                keywords: keywordsForSearch, itemFilters: filters);

            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains(items[5]));
        }

        [Test]
        public void SearchItems_ByKeywords_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Item> items = SetUpInventoryForSearchTests(context);
            items[4].Rank = 3.5;
            items[5].Rank = 8;


            List<string> keywordsForSearch = new List<string>() { "word1" };
            ReadOnlyCollection<Item> results = storeInventory.SearchItems(context: context, keywords: keywordsForSearch);

            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Contains(items[4]));
            Assert.IsTrue(results.Contains(items[5]));
        }

        [Test]
        public void IsItemAmountAvailableForPurchase_Yes_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 10, categories1, 3.55, context: context);

            Assert.DoesNotThrow(() => storeInventory.IsItemAmountAvailableForPurchase(item.Id, 4));
        }

        [Test]
        public void IsItemAmountAvailableForPurchase_No1_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            
            Item item = storeInventory.AddItem("newItem", 9, categories1, 3.55, context: context);

            Assert.Throws<ItemAmountException>(() => storeInventory.IsItemAmountAvailableForPurchase(item.Id, 10));
        }


        [Test]
        public void IsItemAmountAvailableForPurchase_No2_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 10, categories1, 3.55, context: context);

            Assert.Throws<ItemAmountException>(() => storeInventory.IsItemAmountAvailableForPurchase(item.Id, 12));
        }

        [Test]
        public void IsItemAmountAvailableForPurchase_NegativeAmount_No3_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item = storeInventory.AddItem("newItem", 10, categories1, 3.55, context: context);

            Assert.Throws<ArgumentOutOfRangeException>(() => storeInventory.IsItemAmountAvailableForPurchase(item.Id, -3));
        }

        [Test]
        public void IsItemAmountAvailableForPurchase_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeInventory.IsItemAmountAvailableForPurchase(fakeId, 10));
        }

        [Test]
        public void GetItemByName_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item1 = storeInventory.AddItem("item1", 2, categories1, 3.55, context: context);
            Item item2 = storeInventory.AddItem("item2", 444, categories1, 30, context: context);

            Assert.AreSame(item1, storeInventory.GetItemByName("item1"));
        }

        [Test]
        public void GetItemByName_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item1 = storeInventory.AddItem("item1", 2, categories1, 3.55, context: context); // just initialization

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item name: {0}", "noSuchItem")),
            () => storeInventory.GetItemByName("noSuchItem"));
        }

        [Test]
        public void GetItemByName_NoSuchItem_EmptyInventory_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item name: {0}", "noSuchItem")),
            () => storeInventory.GetItemByName("noSuchItem"));
        }

        [Test]
        public void GetItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Item item1 = storeInventory.AddItem("item1", 2, categories1, 3.55, context: context);
            Item item2 = storeInventory.AddItem("item2", 444, categories1, 30, context: context);

            ReadOnlyCollection<Item> items = storeInventory.GetStoreItems();

            Assert.IsTrue(items.Contains(item1));
            Assert.IsTrue(items.Contains(item2));
            Assert.AreEqual(2, items.Count);
        }

        [Test]
        public void GetItems_NoItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.AreEqual(0, storeInventory.GetStoreItems().Count);
        }

        [Test]
        public void GetStoreId_shouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.AreEqual(storeID, storeID);
        }

        private Item SetUpInvetoryWithOneItem(MarketDbContext context)
        {
            return storeInventory.AddItem("item1", 3, categories1, 10.5, context: context);
            /*item2 = new Item(id2, storeID, "item2", 10, categories2, 1, 20);
            item3 = new Item(id3, storeID, "item3", 20, categories3, 2, 2);*/
        }

        private List<Item> SetUpInventoryForSearchTests(MarketDbContext context)
        {
            Item item1 = storeInventory.AddItem("item1", 3, categories1, 10.5, context: context);
            Item item2 = storeInventory.AddItem("item2", 4, categories2, 11.22, context: context);
            Item item3 = storeInventory.AddItem("item3", 5, categories3, 13.30, context: context);

            Item item4 = storeInventory.AddItem("item4", 40, categories1, 10.5, context: context, keywordNames: keywords1);
            Item item5 = storeInventory.AddItem("item5", 30, categories2, 11.22, context: context, keywordNames: keywords2);
            Item item6 = storeInventory.AddItem("item6", 5, categories3, 13.30, context: context, keywordNames: keywords3);

            List<Item> items = new List<Item>() { item1, item2, item3, item4, item5, item6 };
            
            return items;
        }
    }
}
