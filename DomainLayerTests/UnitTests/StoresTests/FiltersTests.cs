using System;
using System.Collections.Generic;
using System.Text;
using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Stores;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using Effort;
using Effort.Provider;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests
{
    class FiltersTests
    {
        FilterByStoreRank filterStoreRank;
        FilterByItemRank filterByItemRank;
        FilterByPrice filterByPrice;
        Store store;
        Item item;
        EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();

            filterStoreRank = new FilterByStoreRank(4);
            filterByItemRank = new FilterByItemRank(3);
            filterByPrice = new FilterByPrice(5, 20.66);

            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            Guid Owner = owner.ID;
            Guid storeID = Guid.NewGuid();
            store = new Store(storeID, DataForTests.CreateTestContactDetails(), new PurchasePolicy(storeID), new DiscountPolicy(storeID), Owner, context);
            context.Stores.Add(store);
            HashSet<Category> categories = new HashSet<Category>() { new Category("cat1"), new Category("cat2") };
            context.Categories.AddRange(categories);
            item = new Item(Guid.NewGuid(), store.Id, "name ", 2, categories, 3.55);
            context.Items.Add(item);
            context.SaveChanges();
       }

        //FilterByStoreRank Tests
        [Test]
        public void StoreRank_DoesStoreStandInFilter_RankLessThan_ShouldPass()
        {
            Assert.IsFalse(filterStoreRank.DoesStoreStandInFilter(store)); // store initial rank = 0
        }

        [Test]
        public void StoreRank_DoesStoreStandInFilter_RankEqual_ShouldPass()
        {
            store.Rank = 4;

            Assert.IsTrue(filterStoreRank.DoesStoreStandInFilter(store));
        }

        [Test]
        public void StoreRank_DoesStoreStandInFilter_RankBiggerThan_ShouldPass()
        {
            store.Rank = 6.7;

            Assert.IsTrue(filterStoreRank.DoesStoreStandInFilter(store));
        }

        [Test]
        public void StoreRank_DoesItemStandInFilter_DefaultResult_ShouldPass()
        {
            Assert.IsTrue(filterStoreRank.DoesItemStandInFilter(item));

            Assert.IsTrue(filterStoreRank.DoesItemStandInFilter(null));
        }



        //FilterByItemRank Tests
        [Test]
        public void ItemRank_DoesItemStandInFilter_RankLessThan_ShouldPass()
        {
            Assert.IsFalse(filterByItemRank.DoesItemStandInFilter(item)); // item initial rank = 0
        }

        [Test]
        public void ItemRank_DoesItemStandInFilter_RankEqual_ShouldPass()
        {
            item.Rank = 3;
            Assert.IsTrue(filterByItemRank.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemRank_DoesItemStandInFilter_RankBiggerThan_ShouldPass()
        {
            item.Rank = 7.66;
            Assert.IsTrue(filterByItemRank.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemRank_DoesStoreStandInFilter_DefaultResult_ShouldPass()
        {
            Assert.IsTrue(filterByItemRank.DoesStoreStandInFilter(store));

            Assert.IsTrue(filterByItemRank.DoesStoreStandInFilter(null));
        }

        //FilterByPrice Tests
        [Test]
        public void ItemPrice_InvalidPricesParams_ShouldFail()
        {
            Assert.Throws(Is.TypeOf<InvalidSearchFilterException>()
            .And.Message.EqualTo(string.Format("FilterByPrice : minPrice bigger than maxPrice")),
            () => new FilterByPrice(40,22.3));

        }
        // range 5-20.66
        [Test]
        public void ItemPrice_DoesItemStandInFilter_PriceLessThan_ShouldPass()
        {
            Assert.IsFalse(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_DoesItemStandInFilter_PriceInRange_ShouldPass()
        {
            item.Price = 20.66;

            Assert.IsTrue(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_DoesItemStandInFilter_PriceBiggerThan_ShouldPass()
        {
            item.Price = 30.44;

            Assert.IsFalse(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_DoesStoreStandInFilter_DefaultResult_ShouldPass()
        {
            Assert.IsTrue(filterByPrice.DoesStoreStandInFilter(store));

            Assert.IsTrue(filterByPrice.DoesStoreStandInFilter(null));
        }

        //only min price condition
        [Test]
        public void ItemPrice_MinPriceConditionOnly_DoesItemStandInFilter_Yes_ShouldPass()
        {
            filterByPrice = new FilterByPrice(5.75, null);
            item.Price = 600;

            Assert.IsTrue(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_MinPriceConditionOnly_DoesItemStandInFilter_Yes2_ShouldPass()
        {
            filterByPrice = new FilterByPrice(5.75, null);
            item.Price = 5.75;

            Assert.IsTrue(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_MinPriceConditionOnly_DoesItemStandInFilter_No_ShouldPass()
        {
            filterByPrice = new FilterByPrice(5.75, null);
            item.Price = 5.70;

            Assert.IsFalse(filterByPrice.DoesItemStandInFilter(item));
        }

        //only max price condition
        [Test]
        public void ItemPrice_MaxPriceConditionOnly_DoesItemStandInFilter_Yes_ShouldPass()
        {
            filterByPrice = new FilterByPrice(null, 20);

            Assert.IsTrue(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_MaxPriceConditionOnly_DoesItemStandInFilter_Yes2_ShouldPass()
        {
            filterByPrice = new FilterByPrice(null, 20);
            item.Price = 20;

            Assert.IsTrue(filterByPrice.DoesItemStandInFilter(item));
        }

        [Test]
        public void ItemPrice_MaxPriceConditionOnly_DoesItemStandInFilter_No_ShouldPass()
        {
            filterByPrice = new FilterByPrice(null, 20);
            item.Price = 20.1;

            Assert.IsFalse(filterByPrice.DoesItemStandInFilter(item));
        }

    }
}
