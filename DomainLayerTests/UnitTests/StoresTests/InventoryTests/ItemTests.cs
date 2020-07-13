using System;
using System.Collections.Generic;
using System.Linq;
using DomainLayer.DbAccess;
using DomainLayer.Stores;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using Effort;
using Effort.Provider;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests.InventoryTests
{
    class ItemTests
    {
        Guid id;
        Guid storeID;
        HashSet<string> stringCategories = new HashSet<string>() { "cat1", "cat2", "cat3" };
        HashSet<Category> categories = new HashSet<Category>() { new Category("cat1"), new Category("cat2"), new Category("cat3") };
        Item item;
        private EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();

            RegisteredUser opener = new RegisteredUser("OPENER", new byte[] { });
            context.Users.Add(opener);

            Guid openerID = opener.ID;
            storeID = Guid.NewGuid();
            Store store = new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), new DiscountPolicy(storeID), openerID, context);// only for consistency in Db
            context.Stores.Add(store);

            ////context.Categories.AddRange(categories);
            //foreach (Category category in categories)
            //{
            //    context.Categories.Add(category);
            //}

            id = Guid.NewGuid();
            //item = new Item(id, storeID, "item", 5, categories, 50);
            item = store.AddItem("item", 5, stringCategories, 50, context);
            id = item.Id;
            //store.storeInventory.StoreItems.Add(item);
            //context.Items.Add(item);
            context.SaveChanges();
            //StoreInventory storeInventory = store.storeInventory;
            //storeInventory.StoreItems.Add(item);
            //context.SaveChanges();
        }

        [Test]
        public void ItemCreation_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            item = context.Items.Find(id);

            Assert.AreEqual(item.Id, id);
            Assert.AreEqual(item.StoreId, storeID);
            Assert.AreEqual(item.Name, "item");
            Assert.AreEqual(item.Amount, 5);
            Assert.AreEqual(item.Categories, categories);
            Assert.AreEqual(item.Price, 50.00);
            Assert.AreEqual(item.Keywords.Count,0);
        }

        [Test]
        public void ItemCreation_WithKeyWords_ShouldPass()
        {
            HashSet<Keyword> keyWords = new HashSet<Keyword>() { new Keyword("word1"), new Keyword("key word 2") };

            item = new Item(id, storeID, "item", 5, categories, 50, keyWords);

            Assert.AreEqual(item.Id, id);
            Assert.AreEqual(item.StoreId, storeID);
            Assert.AreEqual(item.Name, "item");
            Assert.AreEqual(item.Amount, 5);
            Assert.AreEqual(item.Categories, categories);
            Assert.AreEqual(item.Price, 50.00);
            Assert.AreEqual(item.Keywords.Count, 2);
            Assert.AreEqual(item.Keywords, keyWords);
        }

        [Test]
        public void Item_setProperties_ShouldPass()
        {

            HashSet<Category> new_categories = new HashSet<Category>() { new Category("cat4"), new Category("cat5"), new Category("cat6") };

            item.Amount = 100;
            item.Categories = new_categories;
            item.Categories.Add(new Category("cat7"));
            item.Price = 200.5550;
            Keyword keyword1 = new Keyword("word1");
            Keyword keyword2 = new Keyword("word2");
            item.Keywords = new HashSet<Keyword>() { keyword1, keyword2 };

            Assert.AreEqual(item.Amount, 100);
            Assert.AreEqual(item.Categories, new_categories);
            Assert.AreEqual(item.Price, 200.5550);
            Assert.AreEqual(item.Keywords.Count, 2);
            Assert.IsTrue(item.Keywords.Contains(keyword1));
            Assert.IsTrue(item.Keywords.Contains(keyword2));
        }

        [Test]
        public void Items_Equals_ShouldPass()
        {
            Assert.AreSame(item, item);

            //NOT Possible scenario in this system as no two items with same guid id
            Item copy = new Item(id, storeID, "item", 5, categories, 50);
            Assert.AreEqual(item, copy);
        }

        [Test]
        public void Items_DifferentIds_Not_Equals_ShouldPass()
        {
            Item copy = new Item(Guid.NewGuid(), storeID, "item", 5, categories, 50);

            Assert.AreNotEqual(item, copy);
        }

        [Test]
        public void Item_VS_NotItem_Not_Equals_ShouldPass()
        {
            Assert.AreNotEqual(item, 3);
        }
    }
}
