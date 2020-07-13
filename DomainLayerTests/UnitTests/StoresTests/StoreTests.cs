using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using Effort;
using Effort.Provider;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests
{
    //TODO: Add tests for policies!
    class StoreTests
    {
        private Guid TEST_FIRST_OWNER;
        private Guid TEST_OWNER;
        private Guid TEST_SUB_OWNER;
        private Guid TEST_SUB_SUB_OWNER;
        private Guid TEST_SUB_MANAGER;
        private Guid TEST_SUB_SUB_MANAGER;
        private Guid TEST_MANAGER;
        private Guid TEST_NEW_USER;
        private NotificationSubjectMock NOTIFICATION_SUBJECT_MOCK = new NotificationSubjectMock();
        private string STUB_USER_NAME = "STUB_USER";
        static HashSet<string> categories1;
        Store store;
        Guid storeID;
        private EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            storeID = Guid.NewGuid();
            categories1 = new HashSet<string>() { "cat1" };
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using MarketDbContext context = new MarketDbContext(inMemoryConnection);
            context.Init();
            RegisteredUser first_owner = new RegisteredUser("FIRST_OWNER", new byte[] { });
            context.Users.Add(first_owner);
            TEST_FIRST_OWNER = first_owner.ID;
            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            TEST_OWNER = owner.ID;
            RegisteredUser sub_owner = new RegisteredUser("SUB_OWNER", new byte[] { });
            context.Users.Add(sub_owner);
            TEST_SUB_OWNER = sub_owner.ID;
            RegisteredUser sub_sub_owner = new RegisteredUser("SUB_SUB_OWNER", new byte[] { });
            context.Users.Add(sub_sub_owner);
            TEST_SUB_SUB_OWNER = sub_sub_owner.ID;
            RegisteredUser sub_manager = new RegisteredUser("SUB_MANAGER", new byte[] { });
            context.Users.Add(sub_manager);
            TEST_SUB_MANAGER = sub_manager.ID;
            RegisteredUser sub_sub_manager = new RegisteredUser("SUB_SUB_MANAGER", new byte[] { });
            context.Users.Add(sub_sub_manager);
            TEST_SUB_SUB_MANAGER = sub_sub_manager.ID;
            RegisteredUser manager = new RegisteredUser("MANAGER", new byte[] { });
            context.Users.Add(manager);
            TEST_MANAGER = manager.ID;
            RegisteredUser new_user = new RegisteredUser("NEW_USER", new byte[] { });
            context.Users.Add(new_user);
            TEST_NEW_USER = new_user.ID;
            store = new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), new DiscountPolicy(storeID), TEST_FIRST_OWNER, context); // TODO: null until implementation
            context.Stores.Add(store);
            context.SaveChanges();
        }

        [Test]
        public void Store_AddItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            
            Assert.AreEqual(item.StoreId, store.Id);

            ReadOnlyCollection<Item> items = store.GetStoreItems();
            if ((item != null) && items.Contains(item) && items.Count == 1)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Store_AddItemWithSameNameTwice_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            ReadOnlyCollection<Item> items = store.GetStoreItems();
            if ((item != null) && items.Contains(item) && items.Count == 1)
            {
                Assert.Throws(Is.TypeOf<ItemAlreadyExistsException>()
                .And.Message.EqualTo(string.Format("Item with name {0} already exists", "newItem")),
              () => store.AddItem("newItem", 33, categories1, 20.55, context: context));

            }
            else
            {
                Assert.Fail();
            }
        }

        [Test]
        public void Store_DeleteItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            ReadOnlyCollection<Item> items = store.GetStoreItems();

            Assert.IsTrue(items.Count == 1);

            Assert.DoesNotThrow(() => store.DeleteItem(items[0].Id, context));
            Assert.AreEqual(0, store.GetStoreItems().Count);
        }

        [Test]
        public void Store_DeleteItem_Twice_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);

            store.DeleteItem(item.Id, context);

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
               .And.Message.EqualTo(string.Format("Invalid Item id: {0}", item.Id)),
             () => store.DeleteItem(item.Id, context));
        }

        [Test]
        public void Store_DeleteItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
               .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
             () => store.DeleteItem(fakeId, context));
        }

        [Test]
        //all other full tests for edit item in storeInventory
        public void Store_EditItem_JustCategories_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            List<string> newCategories = new List<string>() { "newCat" };
            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);

            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.categories, newCategories }
            };

            store.EditItem(item.Id, newDetails, context);

            item = store.GetItemById(item.Id);

            Assert.AreEqual("newItem", item.Name);
            Assert.AreEqual(2, item.Amount);
            Assert.AreEqual(newCategories.Count, item.Categories.Count); // the only new value expected
            foreach (string name in newCategories)
            {
                Assert.IsTrue(item.Categories.Contains(new Category(name)));
            }
            Assert.AreEqual(0, item.Rank);
            Assert.AreEqual(3.55, item.Price);
        }

        [Test]
        public void Store_EditItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();
            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.EditItem(fakeId, new Dictionary<StoresUtils.ItemEditDetails, object>() {}, context));

        }

        [Test]
        public void Store_AddCategoryItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);

            Assert.IsTrue(store.AddCategoryItem(item.Id, "cat2", context));
            Assert.IsTrue(item.Categories.Count == 2);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));
        }

        [Test]
        public void Store_AddCategoryItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.AddCategoryItem(fakeId, "cat", context));
        }

        [Test]
        public void UpdateCategoryItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);

            Assert.IsTrue(store.UpdateCategoryItem(item.Id, "cat1", "cat2", context));
            Assert.IsTrue(item.Categories.Count == 1);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));

            Assert.IsFalse(item.Categories.Contains(new Category("cat1")));
        }

        [Test]
        public void UpdateCategoryItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.UpdateCategoryItem(fakeId, "cat", "cat222", context));
        }

        [Test]
        public void Store_DeleteCategoryItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            store.AddCategoryItem(item.Id, "cat2", context);

            Assert.IsTrue(store.DeleteCategoryItem(item.Id, "cat1", context));

            Assert.IsTrue(item.Categories.Count == 1);
            Assert.IsTrue(item.Categories.Contains(new Category("cat2")));
            Assert.IsFalse(item.Categories.Contains(new Category("cat1")));
        }

        [Test]
        public void Store_DeleteCategoryItem_TheOnlyCategory_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo("Cannot delete the only category of item with id:" + item.Id),
            () => store.DeleteCategoryItem(item.Id, "cat1", context));
        }

        [Test]
        public void Store_DeleteCategoryItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.DeleteCategoryItem(fakeId, "cat1", context));
        }

        [Test]
        public void Store_AddKeyWordItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);

            Assert.IsTrue(store.AddKeyWordItem(item.Id, "word", context));

            Assert.IsTrue(item.Keywords.Contains(new Keyword("word")));
            Assert.AreEqual(1, item.Keywords.Count);
        }

        [Test]
        public void Store_AddKeyWordItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.AddKeyWordItem(fakeId, "word", context));
        }

        [Test]
        public void Store_UpdateKeyWordItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            store.AddKeyWordItem(item.Id, "word1", context);
            store.AddKeyWordItem(item.Id, "word2", context);

            Assert.IsTrue(store.UpdateKeyWordItem(item.Id, "word2", "word3", context));

            Assert.AreEqual(2, item.Keywords.Count);
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word3")));
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word1")));

            Assert.IsFalse(item.Keywords.Contains(new Keyword("word2")));
        }

        [Test]
        public void Store_UpdateKeyWordItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.UpdateKeyWordItem(fakeId, "word1", "word2", context));
        }

        [Test]
        public void Store_DeleteKeyWordItem_ItemHasOnlyOneKeyWord_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            store.AddKeyWordItem(item.Id, "word1", context);

            Assert.IsTrue(store.DeleteKeyWordItem(item.Id, "word1", context));

            Assert.AreEqual(0, item.Keywords.Count);
        }

        [Test]
        public void Store_DeleteKeyWordItem_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item = store.AddItem("newItem", 2, categories1, 3.55, context: context);
            store.AddKeyWordItem(item.Id, "word1",context);
            store.AddKeyWordItem(item.Id, "word2", context);
            store.AddKeyWordItem(item.Id, "word3", context);

            Assert.AreEqual(3, item.Keywords.Count);

            Assert.IsTrue(store.DeleteKeyWordItem(item.Id, "word2", context));

            Assert.AreEqual(2, item.Keywords.Count);
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word1")));
            Assert.IsTrue(item.Keywords.Contains(new Keyword("word3")));
        }

        [Test]
        public void Store_DeleteKeyWordItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.DeleteKeyWordItem(fakeId, "keyword", context));
        }

        [Test]
        public void GetId_ShouldPass()
        {
            Assert.AreEqual(storeID, store.Id);
        }

        [Test]
        public void GetStoreItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item1 = store.AddItem("item1", 2, categories1, 3.55, context: context);
            Item item2 = store.AddItem("item2", 2, categories1, 40, context: context);
            ReadOnlyCollection<Item> items = store.GetStoreItems();

            Assert.IsTrue(items.Contains(item1));
            Assert.IsTrue(items.Contains(item2));
            Assert.IsTrue(items.Count == 2);
        }

        [Test]
        public void GetStoreItems_NoItems_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            ReadOnlyCollection<Item> items = store.GetStoreItems();

            Assert.IsTrue(items.Count == 0);
        }

        [Test]
        public void GetStoreContactDetails_ShouldPass()
        {
            Assert.AreEqual(DataForTests.CreateTestContactDetails(), store.ContactDetails);
        }

        [Test]
        public void SetRank_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            store.Rank = 6.5;
            Assert.AreEqual(store.Rank, 6.5);
        }

        [Test]
        public void EditStoreContactDetails_AllEditableDetails_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);
 
            Dictionary<StoresUtils.StoreEditContactDetails, object> newDetails = new Dictionary<StoresUtils.StoreEditContactDetails, object>()
            {
                {StoresUtils.StoreEditContactDetails.name, "newStore"},
                {StoresUtils.StoreEditContactDetails.email, "new email"},
                {StoresUtils.StoreEditContactDetails.address, "newAddress"},
                {StoresUtils.StoreEditContactDetails.phone, "0522222222"},
                {StoresUtils.StoreEditContactDetails.bankAccountNumber, "888-222222/20"},
                {StoresUtils.StoreEditContactDetails.bank, "Poalim"},
                {StoresUtils.StoreEditContactDetails.description, "New store description"}
            };

            Assert.IsTrue(store.EditStoreContactDetails(newDetails, context));

            Store updated = context.Stores.Find(storeID);
            StoreContactDetails storeContactDetails = updated.ContactDetails;

            Assert.AreEqual(storeContactDetails.Name, "newStore");
            Assert.AreEqual(storeContactDetails.Email, "new email");
            Assert.AreEqual(storeContactDetails.Address, "newAddress");
            Assert.AreEqual(storeContactDetails.Phone, "0522222222");
            Assert.AreEqual(storeContactDetails.BankAccountNumber, "888-222222/20");
            Assert.AreEqual(storeContactDetails.Bank, "Poalim");
            Assert.AreEqual(storeContactDetails.Description, "New store description");
        }

        [Test]
        public void EditStoreContactDetails_JustName_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Dictionary<StoresUtils.StoreEditContactDetails, object> newDetails = new Dictionary<StoresUtils.StoreEditContactDetails, object>()
            {
                {StoresUtils.StoreEditContactDetails.name, "newStore"}
            };

            Assert.IsTrue(store.EditStoreContactDetails(newDetails, context));

            Store updated = context.Stores.Find(storeID);
            StoreContactDetails storeContactDetails = updated.ContactDetails;

            Assert.AreEqual(storeContactDetails.Name, "newStore");
            Assert.AreEqual(storeContactDetails.Email, "store@gmail.com");
            Assert.AreEqual(storeContactDetails.Address, "Address");
            Assert.AreEqual(storeContactDetails.Phone, "0544444444");
            Assert.AreEqual(storeContactDetails.BankAccountNumber, "888-444444/34");
            Assert.AreEqual(storeContactDetails.Bank, "Leumi");
            Assert.AreEqual(storeContactDetails.Description, "Store description");
        }

        [Test]
        public void EditStoreContactDetails_NoChangeToDetails_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Dictionary<StoresUtils.StoreEditContactDetails, object> newDetails = new Dictionary<StoresUtils.StoreEditContactDetails, object>(){};

            Assert.IsTrue(store.EditStoreContactDetails(newDetails, context));

            Store updated = context.Stores.Find(storeID);
            StoreContactDetails storeContactDetails = updated.ContactDetails;

            Assert.AreEqual(storeContactDetails.Name, "store");
            Assert.AreEqual(storeContactDetails.Email, "store@gmail.com");
            Assert.AreEqual(storeContactDetails.Address, "Address");
            Assert.AreEqual(storeContactDetails.Phone, "0544444444");
            Assert.AreEqual(storeContactDetails.BankAccountNumber, "888-444444/34");
            Assert.AreEqual(storeContactDetails.Bank, "Leumi");
            Assert.AreEqual(storeContactDetails.Description, "Store description");
        }

        [Test]
        public void IsOrderItemsAmountAvailable_Yes_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 9);
            storeCart.AddToStoreCart(ids[1], 19);
            
            Assert.DoesNotThrow(() => store.IsOrderItemsAmountAvailable(storeCart, context));
        }

        [Test]
        public void IsOrderItemsAmountAvailable_No_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 11);
            storeCart.AddToStoreCart(ids[1], 4);

            Assert.Throws<ItemAmountException>(() => store.IsOrderItemsAmountAvailable(storeCart, context));
        }

        [Test]
        public void IsOrderItemsAmountAvailable_No2_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 11);
            storeCart.AddToStoreCart(ids[1], 21);

            Assert.Throws<ItemAmountException>(() => store.IsOrderItemsAmountAvailable(storeCart, context));
        }

        [Test]
        public void IsOrderItemsAmountAvailable__AmountBigger_No3_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 12);
            storeCart.AddToStoreCart(ids[1], 20);

            Assert.Throws<ItemAmountException>(() => store.IsOrderItemsAmountAvailable(storeCart, context));
        }

        [Test]
        public void IsOrderItemsAmountAvailable_EmptyList_Yes_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            StoreCart storeCart = new StoreCart(Guid.NewGuid());

            Assert.DoesNotThrow(() => store.IsOrderItemsAmountAvailable(storeCart, context));
        }

        [Test]
        public void IsOrderItemsAmountAvailable_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid fakeId = Guid.NewGuid();
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(fakeId, 2000);

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.IsOrderItemsAmountAvailable(storeCart, context));
        }

        [Test]
        public void ReduceStoreInventoryDueToOrder_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 9);
            storeCart.AddToStoreCart(ids[1], 19);

            Assert.IsTrue(store.ReduceStoreInventoryDueToOrder(storeCart, context));
            ReadOnlyCollection<Item> items = store.GetStoreItems();
            foreach(Item item in items)
            {
                if(item.Id == ids[0])
                {
                    Assert.AreEqual(1, item.Amount);

                }
                if(item.Id == ids[1])
                {
                    Assert.AreEqual(1, item.Amount);
                }
            }
        }

        [Test]
        public void ReduceStoreInventoryDueToOrder_AmountToReduceNegative_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], -9);
            storeCart.AddToStoreCart(ids[1], 19);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: amount to reduce is not positive value", ids[0])),
            () => store.ReduceStoreInventoryDueToOrder(storeCart, context));
        }

        [Test]
        public void ReduceStoreInventoryDueToOrder_FirstAmountToReduceTooBig_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 11);
            storeCart.AddToStoreCart(ids[1], 19);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: Item {0} cannot have non positive amount", ids[0])),
            () => store.ReduceStoreInventoryDueToOrder(storeCart, context));
        }

        [Test]
        public void ReduceStoreInventoryDueToOrder_SecondAmountToReduceTooBig_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 3);
            storeCart.AddToStoreCart(ids[1], 21);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: Item {0} cannot have non positive amount", ids[1])),
            () => store.ReduceStoreInventoryDueToOrder(storeCart, context));
        }

        [Test]
        public void ReduceStoreInventoryDueToOrder_BothAmountsToReduceTooBig_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 30);
            storeCart.AddToStoreCart(ids[1], 40);

            Assert.Throws(Is.TypeOf<InvalidOperationOnItemException>()
            .And.Message.EqualTo(string.Format("ReduceItemAmount: Item {0} cannot have non positive amount", ids[0])),
            () => store.ReduceStoreInventoryDueToOrder(storeCart, context));
        }

        [Test]
        public void ReduceStoreInventoryDueToOrder_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid[] ids = setUpInventoryForPurchasingTests(context);
            StoreCart storeCart = new StoreCart(Guid.NewGuid());
            storeCart.AddToStoreCart(ids[0], 2);
            storeCart.AddToStoreCart(ids[1], 1);
            Guid fakeId = Guid.NewGuid();
            storeCart.AddToStoreCart(fakeId, 3);

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.ReduceStoreInventoryDueToOrder(storeCart, context));
        }

        [Test]
        public void GetItemsById_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item1 = store.AddItem("item1", 2, categories1, 3.55, context: context);
            Item item2 = store.AddItem("item2", 2, categories1, 40, context: context);
            Item item3 = store.AddItem("item3", 2, categories1, 40, context: context);

            List<Item> results = store.GetItemsById(new List<Guid>() { item1.Id, item2.Id }, context);

            Assert.AreEqual(2, results.Count);

            Assert.IsTrue(results.Contains(item1));
            Assert.IsTrue(results.Contains(item2));

            Assert.IsFalse(results.Contains(item3));
        }

        [Test]
        public void GetItemsById_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item1 = store.AddItem("item1", 2, categories1, 3.55, context);
            Item item2 = store.AddItem("item2", 2, categories1, 40, context);
            Item item3 = store.AddItem("item3", 2, categories1, 40, context);

            Guid fakeId = Guid.NewGuid();

            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
            .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => store.GetItemsById(new List<Guid>() { item1.Id, fakeId, item2.Id }, context));
        }

        [Test]
        public void GetItemsById_EmptyList_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Item item1 = store.AddItem("item1", 2, categories1, 3.55, context); // just for initialization


            List<Item> results = store.GetItemsById(new List<Guid>() { }, context);

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Stores_Equals_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Store storeFromDB = context.Stores.Find(store.Id);
            Assert.AreSame(store, storeFromDB);

            //NOT Possible scenario in this system as no two stores with same guid id
            Assert.AreEqual(store, new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), new DiscountPolicy(storeID), TEST_FIRST_OWNER, context));
        }

        [Test]
        public void Stores_NotEquals_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Guid storeID = Guid.NewGuid();
            Store store2 = new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), new DiscountPolicy(storeID), TEST_FIRST_OWNER, context);

            Assert.AreNotEqual(store, store2);
        }

        [Test]
        public void Stores_nullPurchasePolicy_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid storeID = Guid.NewGuid();
            Assert.Throws<ArgumentNullException>(() => new Store(Guid.NewGuid(), DataForTests.CreateTestContactDetails(), null, new DiscountPolicy(storeID), TEST_FIRST_OWNER, context));
        }

        [Test]
        public void Stores_nullDiscountPolicy_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            Guid storeID = Guid.NewGuid();
            Assert.Throws<ArgumentNullException>(() => new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), null, TEST_FIRST_OWNER, context));
        }

        [Test]
        public void Store_VS_NotStore_Not_Equals_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.AreNotEqual(store, new Item(Guid.NewGuid(), storeID, "item", 5, new HashSet<Category>() {new Category("cat")}, 50));
        }

        private Guid[] setUpInventoryForPurchasingTests(MarketDbContext context)
        {

            Item item1 = store.AddItem("item1", 10, categories1, 15.55, context);
            Item item2 = store.AddItem("item2", 20, categories1, 25.55, context);

            Guid[] ids = { item1.Id, item2.Id};

            return ids;
        }

        [Test]
        public void AppointStaff()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddOwner(TEST_OWNER, TEST_FIRST_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.DoesNotThrow(() => store.AddManager(TEST_MANAGER, TEST_OWNER, context));
        }
        [Test]
        public void AppointStaffWithSubStaff()
        {
            AppointStaff();

            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);
            Assert.DoesNotThrow(() => store.AddOwner(TEST_SUB_OWNER, TEST_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.True(store.HasAwaitingContract(TEST_SUB_OWNER));
            Assert.True(store.isApproverOf(TEST_FIRST_OWNER, TEST_SUB_OWNER, context));
            Assert.DoesNotThrow(() => store.ApproveContract(TEST_SUB_OWNER, TEST_FIRST_OWNER, context));
            Assert.True(store.IsOwnerOfStore(TEST_SUB_OWNER, context));
            Assert.DoesNotThrow(() => store.AddManager(TEST_SUB_MANAGER, TEST_OWNER, context));
            Assert.DoesNotThrow(() => store.AddOwner(TEST_SUB_SUB_OWNER, TEST_SUB_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.True(store.HasAwaitingContract(TEST_SUB_SUB_OWNER));
            Assert.True(store.isApproverOf(TEST_FIRST_OWNER, TEST_SUB_SUB_OWNER, context));
            Assert.True(store.HasAwaitingContract(TEST_SUB_SUB_OWNER));
            Assert.True(store.isApproverOf(TEST_OWNER, TEST_SUB_SUB_OWNER, context));
            Assert.DoesNotThrow(() => store.ApproveContract(TEST_SUB_SUB_OWNER, TEST_FIRST_OWNER, context));
            Assert.DoesNotThrow(() => store.ApproveContract(TEST_SUB_SUB_OWNER, TEST_OWNER, context));
            Assert.False(store.HasAwaitingContract(TEST_SUB_SUB_OWNER));
            Assert.True(store.IsOwnerOfStore(TEST_SUB_SUB_OWNER, context));
            Assert.DoesNotThrow(() => store.AddManager(TEST_SUB_SUB_MANAGER, TEST_SUB_OWNER, context));
        }

        [Test]
        public void RemoveOwnerAlsoValidatesContractsWaitingForHim_success()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddOwner(TEST_SUB_OWNER, TEST_FIRST_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.True(store.HasAwaitingContract(TEST_SUB_OWNER));
            Assert.DoesNotThrow(() => store.RemoveOwner(TEST_OWNER, TEST_FIRST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.True(store.IsOwnerOfStore(TEST_SUB_OWNER, context));
        }

        //TODO add notification test!

        [Test]
        public void RemoveOwnerRemovesContractsGrantedByHim_success()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddOwner(TEST_SUB_OWNER, TEST_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.True(store.HasAwaitingContract(TEST_SUB_OWNER));
            Assert.DoesNotThrow(() => store.RemoveOwner(TEST_OWNER, TEST_FIRST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.False(store.IsOwnerOfStore(TEST_SUB_OWNER, context));
            Assert.False(store.HasAwaitingContract(TEST_SUB_OWNER));
        }
        [Test]
        public void RemoveOwnerComplexStaff_WithContractsWaiting_success()
        {
            AppointStaffWithSubStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddOwner(TEST_NEW_USER, TEST_SUB_SUB_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.True(store.HasAwaitingContract(TEST_NEW_USER));
            Assert.True(store.isApproverOf(TEST_OWNER, TEST_NEW_USER, context));
            Assert.True(store.isApproverOf(TEST_FIRST_OWNER, TEST_NEW_USER, context));
            Assert.True(store.isApproverOf(TEST_SUB_OWNER, TEST_NEW_USER, context));
            Assert.DoesNotThrow(() => store.RemoveOwner(TEST_OWNER, TEST_FIRST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.False(store.IsOwnerOfStore(TEST_NEW_USER, context));
            Assert.False(store.HasAwaitingContract(TEST_NEW_USER));
        }

        [Test]
        public void CheckPermission_Owner_ShouldPass()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(()=>store.EnsurePermission(TEST_OWNER, Permission.APPOINT_OWNER, context));
        }

        [Test]
        public void CheckPermission_Manager_BasicPermission_ShouldPass()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.EnsurePermission(TEST_MANAGER, Permission.REQUESTS, context));
            Assert.DoesNotThrow(() => store.EnsurePermission(TEST_MANAGER, Permission.HISTORY, context));
        }

        [Test]
        public void CheckPermission_Manager_AdvancedPermission_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<PermissionException>(() => store.EnsurePermission(TEST_MANAGER, Permission.APPOINT_OWNER, context));
        }

        [Test]
        public void CheckPermission_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_NEW_USER, Permission.REQUESTS, context));
        }

        [Test]
        public void AddPermission_Owner_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<PermissionException>(() => store.AddPermission(TEST_OWNER, TEST_FIRST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void AddPermission_Manager_ExsitingPermission_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<PermissionException>(() => store.AddPermission(TEST_MANAGER, TEST_OWNER, Permission.HISTORY, context));
        }

        [Test]
        public void AddPermission_Manager_WrongGrantor_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<NonGrantorException>(() => store.AddPermission(TEST_MANAGER, TEST_FIRST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void AddPermission_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.AddPermission(TEST_NEW_USER, TEST_FIRST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void AddPermission_Manager_CorrectGrantor_NewPermission_ShouldPass()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddPermission(TEST_MANAGER, TEST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void RemovePermission_Owner_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<PermissionException>(() => store.RemovePermission(TEST_OWNER, TEST_FIRST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void RemovePermission_Manager_NewPermission_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<PermissionException>(() => store.RemovePermission(TEST_MANAGER, TEST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void RemovePermission_Manager_WrongGrantor_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<NonGrantorException>(() => store.RemovePermission(TEST_MANAGER, TEST_FIRST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void RemovePermission_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.RemovePermission(TEST_NEW_USER, TEST_FIRST_OWNER, Permission.POLICY, context));
        }

        [Test]
        public void RemovePermission_Manager_CorrectGrantor_ExistingPermission_ShouldPass()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.RemovePermission(TEST_MANAGER, TEST_OWNER, Permission.REQUESTS, context));
        }

        [Test]
        public void RemoveOwner_Manager_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.RemoveOwner(TEST_MANAGER, TEST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void RemoveOwner_WrongGrantor_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<NonGrantorException>(() => store.RemoveOwner(TEST_FIRST_OWNER, TEST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void RemoveOwner_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.RemoveOwner(TEST_NEW_USER, TEST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void RemoveOwner_CorrectGrantor_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.RemoveOwner(TEST_OWNER, TEST_FIRST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_OWNER, Permission.REQUESTS, context));
        }

        [Test]
        public void RemoveOwner_CorrectGrantor_AlsoRemovesOwnersAndManagersGrantedBy_ShouldPass()
        {
            AppointStaffWithSubStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.RemoveOwner(TEST_OWNER, TEST_FIRST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_OWNER, Permission.REQUESTS, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_MANAGER, Permission.REQUESTS, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_SUB_MANAGER, Permission.REQUESTS, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_SUB_OWNER, Permission.REQUESTS, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_SUB_SUB_OWNER, Permission.REQUESTS, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_SUB_SUB_MANAGER, Permission.REQUESTS, context));
        }

        [Test]
        public void RemoveManager_Owner_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.RemoveManager(TEST_OWNER, TEST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void RemoveManager_WrongGrantor_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<NonGrantorException>(() => store.RemoveManager(TEST_MANAGER, TEST_FIRST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void RemoveManager_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.RemoveManager(TEST_NEW_USER, TEST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void RemoveManager_CorrectGrantor_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.RemoveManager(TEST_MANAGER, TEST_OWNER, NOTIFICATION_SUBJECT_MOCK, context));
            Assert.Throws<CertificationException>(() => store.EnsurePermission(TEST_MANAGER, Permission.REQUESTS, context));
        }

        [Test]
        public void AddOwner_Existing_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.AddOwner(TEST_OWNER, TEST_FIRST_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void AddOwner_New_ShouldPass()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddOwner(TEST_NEW_USER, TEST_FIRST_OWNER, STUB_USER_NAME, NOTIFICATION_SUBJECT_MOCK, context));
        }

        [Test]
        public void AddManager_Existing_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.AddManager(TEST_OWNER, TEST_FIRST_OWNER, context));
        }

        [Test]
        public void AddManager_New_ShouldPass()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.DoesNotThrow(() => store.AddManager(TEST_NEW_USER, TEST_FIRST_OWNER, context));
        }

        [Test]
        public void GetPermissions_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.GetPermissions(TEST_NEW_USER, TEST_OWNER, context));
        }

        [Test]
        public void GetPermissions_Manager_Limited()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            List<Permission> permissions = store.GetPermissions(TEST_MANAGER, TEST_OWNER, context);
            Assert.AreEqual(2, permissions.Count);
            Assert.True(permissions.Contains(Permission.HISTORY) & permissions.Contains(Permission.REQUESTS));
        }

        [Test]
        public void GetPermissions_Owner_All()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            List<Permission> permissions = store.GetPermissions(TEST_OWNER, TEST_FIRST_OWNER, context);
            Assert.AreEqual(Enum.GetValues(typeof(Permission)).Length, permissions.Count);
            foreach (Permission p in Enum.GetValues(typeof(Permission)))
            {
                Assert.IsTrue(permissions.Contains(p));
            }
        }

        [Test]
        public void GetOwnPermissions_Manager_Limited()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            List<Permission> permissions = store.GetPermissions(TEST_MANAGER, context);
            Assert.AreEqual(2, permissions.Count);
            Assert.True(permissions.Contains(Permission.HISTORY) & permissions.Contains(Permission.REQUESTS));
        }

        [Test]
        public void GetOwnPermissions_Owner_All()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            List<Permission> permissions = store.GetPermissions(TEST_OWNER, context);
            Assert.AreEqual(Enum.GetValues(typeof(Permission)).Length, permissions.Count);
            foreach (Permission p in Enum.GetValues(typeof(Permission)))
            {
                Assert.IsTrue(permissions.Contains(p));
            }
        }

        [Test]
        public void GetOwnPermissions_NoCertification_ShouldFail()
        {
            AppointStaff();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Stores.Attach(store);

            Assert.Throws<CertificationException>(() => store.GetPermissions(TEST_NEW_USER, context));
        }
    }
}
