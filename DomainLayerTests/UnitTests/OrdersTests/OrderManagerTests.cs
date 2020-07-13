using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.Stores;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using Effort;
using Effort.Provider;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace DomainLayerTests.UnitTests.OrdersTests
{
    public class OrderManagerTests
    {
        private Guid Owner;
        private Guid Buyer1;
        private Guid Buyer2;
        private StoreHandler storeHandler = new StoreHandler();
        private OrderManager orderManager;
        EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            inMemoryConnection = DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();
            orderManager = new OrderManager(storeHandler, inMemoryConnection);
            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            Owner = owner.ID;
            RegisteredUser buyer1 = new RegisteredUser("BUYER1", new byte[] { });
            context.Users.Add(buyer1);
            Buyer1 = buyer1.ID;
            RegisteredUser buyer2 = new RegisteredUser("BUYER2", new byte[] { });
            context.Users.Add(buyer2);
            Buyer2 = buyer2.ID;
            context.SaveChanges();
        }

        [Test]
        public void OrderManager_InvalidOrder_Guest_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            
            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            Store s = storeHandler.OpenStore(details, Owner, context);

            string itemName = "testItemD";
            Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            orderManager.LogOrder(order.userKey, order.OrderId, context);
            Assert.AreEqual(orderManager.GetStoreOrdersHistory(s.Id, context).Count, 1);
        }

        [Test]
        public void OrderManager_InvalidOrder_EmptyShoppingCart_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<OrderNotValidException>(() => orderManager.DiscountAndReserve(Buyer1, new Dictionary<Guid, StoreCart>(), context));
        }

        [Test]
        public void OrderManager_InvalidOrder_ShoppingCartNull_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<OrderNotValidException>(() => orderManager.DiscountAndReserve(Buyer1, null, context));
        }

        [Test]
        public void OrderManager_ValidOrder_StoreNotFound_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(Guid.NewGuid());
            cart.AddToStoreCart(Guid.NewGuid(), 1);
            shoppingCart.Add(Guid.NewGuid(), cart);

            // test
            Assert.Throws<StoreNotFoundException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_ValidOrder_ItemNotFound_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            //StoreHandler storeHandler = new StoreHandler();
            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            s.AddItem("testItemA", 1, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(Guid.NewGuid(), 1);
            shoppingCart.Add(s.Id, cart);

            // test
            Assert.Throws<ItemNotFoundException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_ValidOrder_ItemNotInInventory_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();

            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");
            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemB";
            Item item = s.AddItem(itemName, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            // test
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TooManyItems_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();

            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemC";
            Item item = s.AddItem(itemName, 1, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 3);
            shoppingCart.Add(s.Id, cart);

            // test
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_ValidOrder_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemD";
            Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_TwoShoppingCarts_ShuoldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart1 = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart1.Add(store1.Id, cart1);

            Dictionary<Guid, StoreCart> shoppingCart2 = new Dictionary<Guid, StoreCart>();
            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart2.Add(store2.Id, cart2);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart1, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));

            order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart2, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_TwoShoppingCarts_OneDoesntHaveEnoughItems_ShuoldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart1 = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart1.Add(store1.Id, cart1);

            Dictionary<Guid, StoreCart> shoppingCart2 = new Dictionary<Guid, StoreCart>();
            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart2.Add(store2.Id, cart2);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart1, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart2, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_TwoShoppingCarts_TwoDontHaveEnoughItems_ShuoldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart1 = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart1.Add(store1.Id, cart1);

            Dictionary<Guid, StoreCart> shoppingCart2 = new Dictionary<Guid, StoreCart>();
            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart2.Add(store2.Id, cart2);

            // test
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart1, context));
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart2, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_OneShoppingCarts_ShuoldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart.Add(store2.Id, cart2);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_OneShoppingCarts_FirstStoreNoItems_ShuoldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart.Add(store2.Id, cart2);

            // test
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_OneShoppingCarts_SecondStoreNoItems_ShuoldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart.Add(store2.Id, cart2);

            // test
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_ValidOrder_TwoStores_OneShoppingCarts_BothStoresNoItems_ShuoldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 0, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart.Add(store2.Id, cart2);

            // test
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        [Test]
        public void OrderManager_UserHistory_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemD";
            Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));

            List<Order> userHistory = orderManager.GetUserOrdersHistory(Buyer1, context);
            Assert.AreEqual(1, userHistory.Count());
            Order orderHistory = userHistory.First();
            Assert.AreEqual(orderHistory.StoreOrdersDict.Count, 1);
            KeyValuePair<Guid, StoreOrder> orderedItemFromStore = orderHistory.StoreOrdersDict.First();
            Assert.AreEqual(orderedItemFromStore.Value.StoreOrderItems.Count, 1);
            OrderItem orderedItem = orderedItemFromStore.Value.OrderItemsDict[item.Id];
            Assert.AreEqual(orderedItem.ItemId, item.Id);
        }

        [Test]
        public void OrderManager_UserHistory_2UsersSameStore_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemD";
            Item item = s.AddItem(itemName, 4, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));

            order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer2, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));

            List<Order> userHistory1 = orderManager.GetUserOrdersHistory(Buyer1, context);
            Assert.AreEqual(1, userHistory1.Count());
            Order orderHistory1 = userHistory1.First();
            Assert.AreEqual(orderHistory1.StoreOrdersDict.Count, 1);
            KeyValuePair<Guid, StoreOrder> orderedItemFromStore1 = orderHistory1.StoreOrdersDict.First();
            Assert.AreEqual(orderedItemFromStore1.Value.StoreOrderItems.Count, 1);
            OrderItem orderedItem1 = orderedItemFromStore1.Value.OrderItemsDict[item.Id];
            Assert.AreEqual(orderedItem1.ItemId, item.Id);

            List<Order> userHistory2 = orderManager.GetUserOrdersHistory(Buyer1, context);
            Assert.AreEqual(1, userHistory2.Count());
            Order orderHistory2 = userHistory2.First();
            Assert.AreEqual(orderHistory2.StoreOrdersDict.Count, 1);
            KeyValuePair<Guid, StoreOrder> orderedItemFromStore2 = orderHistory2.StoreOrdersDict.First();
            Assert.AreEqual(orderedItemFromStore2.Value.StoreOrderItems.Count, 1);
            OrderItem orderedItem2 = orderedItemFromStore2.Value.OrderItemsDict[item.Id];
            Assert.AreEqual(orderedItem2.ItemId, item.Id);
        }

        [Test]
        public void OrderManager_UserHistory_NoUser_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Order> userHistory = orderManager.GetUserOrdersHistory(Guid.NewGuid(), context);
            Assert.AreEqual(userHistory.Count, 0);
        }

        [Test]
        public void OrderManager_StoreHistory_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store store = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemD";
            Item item = store.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(store.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(store.Id, cart);

            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));

            List<StoreOrder> storeHistory = orderManager.GetStoreOrdersHistory(store.Id, context);
            Assert.AreEqual(1, storeHistory.Count());
            Assert.AreEqual(storeHistory[0].StoreId, store.Id);
        }

        [Test]
        public void OrderManager_StoreHistory_TwoStores_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 1);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 1);
            shoppingCart.Add(store2.Id, cart2);
            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.DELIVERED;
            Assert.DoesNotThrow(() => orderManager.LogOrder(order.UserId, order.OrderId, context));

            List<StoreOrder> storeHistory1 = orderManager.GetStoreOrdersHistory(store1.Id, context);
            Assert.AreEqual(1, storeHistory1.Count());
            Assert.AreEqual(storeHistory1[0].OrderItemsDict[item1.Id].Name, itemName1);
            Assert.AreEqual(storeHistory1[0].StoreId, store1.Id);

            List<StoreOrder> storeHistory2 = orderManager.GetStoreOrdersHistory(store2.Id, context);
            Assert.AreEqual(1, storeHistory2.Count());
            Assert.AreEqual(storeHistory2[0].OrderItemsDict[item2.Id].Name, itemName2);
            Assert.AreEqual(storeHistory2[0].StoreId, store2.Id);
        }

        [Test]
        public void OrderManager_StoreHistory_NoUser_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<StoreOrder> storeHistory = orderManager.GetStoreOrdersHistory(Guid.NewGuid(), context);
            Assert.AreEqual(storeHistory.Count, 0);
        }

        /* [Test]
         public void OrderManager_FailDelivery_Guest_ShouldFail()
         {
             using var context = new MarketDbContext(inMemoryConnection);

             string storeName = GetStoreName();
             // setup test
             StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

             storeHandler.OpenStore(details, Owner, context);
             Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

             string itemName = "testItemD";
             Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

             Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
             StoreCart cart = new StoreCart(s.Id);
             cart.AddToStoreCart(item.Id, 1);
             shoppingCart.Add(s.Id, cart);

             DeliveryReal realDelivery = new DeliveryReal(new HttpClient());
             orderManager.Delivery.Real = realDelivery;
             Order order = null;
             Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
             Assert.IsNotNull(order);
             order.Status = OrderStatus.PAYED_FOR;
             Assert.Throws<DeliveryFailedException>(() => orderManager.ScheduleDelivery(order.userKey, order.OrderId, "", "", "","", "", context));
         }*/

        /* [Test]
         public void OrderManager_FailPayment_Guest_ShouldFail()
         {
             using var context = new MarketDbContext(inMemoryConnection);

             string storeName = GetStoreName();
             // setup test
             StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

             storeHandler.OpenStore(details, Owner, context);
             Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

             string itemName = "testItemD";
             Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context, keyWords: new HashSet<string>() { "test" });

             Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
             StoreCart cart = new StoreCart(s.Id);
             cart.AddToStoreCart(item.Id, 1);
             shoppingCart.Add(s.Id, cart);

             PaymentReal realPayment = new PaymentReal(new System.Net.Http.HttpClient());
             orderManager.Payment.Real = realPayment;
             Order order = null;
             Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
             Assert.IsNotNull(order);
             Assert.Throws<PaymentFailedException>(() => orderManager.CollectPayment(order.userKey, order.OrderId, "", "", "","","", context));
         }*/

         [Test]
         public void OrderManager_Delivery_Guest_ShouldPass()
         {
             using var context = new MarketDbContext(inMemoryConnection);

             string storeName = GetStoreName();
             // setup test
             StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

             storeHandler.OpenStore(details, Owner, context);
             Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

             string itemName = "testItemD";
             Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

             Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
             StoreCart cart = new StoreCart(s.Id);
             cart.AddToStoreCart(item.Id, 1);
             shoppingCart.Add(s.Id, cart);

             DeliveryReal realDelivery = new DeliveryReal(new HttpClient());
             orderManager.Delivery.Real = realDelivery;
             Order order = null;
             Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
             Assert.IsNotNull(order);
             order.Status = OrderStatus.PAYED_FOR;
             Assert.DoesNotThrow(() => orderManager.ScheduleDelivery(order.userKey, order.OrderId, "address", "city", "country","22222", "name", context));
         }
        /* Failure TEST
        [Test]
        public void OrderManager_Delivery_WrongDetails_Guest_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemD";
            Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            DeliveryReal realDelivery = new DeliveryReal(new HttpClient());
            orderManager.Delivery.Real = realDelivery;
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
            Assert.IsNotNull(order);
            order.Status = OrderStatus.PAYED_FOR;
            Assert.Throws< DeliveryFailedException>(() => orderManager.ScheduleDelivery(order.userKey, order.OrderId, "", "", "", "22222", "name", context));
        }
        */

        [Test]
         public void OrderManager_Payment_Guest_ShouldPass()
         {
             using var context = new MarketDbContext(inMemoryConnection);

             string storeName = GetStoreName();
             // setup test
             StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

             storeHandler.OpenStore(details, Owner, context);
             Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

             string itemName = "testItemD";
             Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context, keyWords: new HashSet<string>() { "test" });

             Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
             StoreCart cart = new StoreCart(s.Id);
             cart.AddToStoreCart(item.Id, 1);
             shoppingCart.Add(s.Id, cart);

             PaymentReal realPayment = new PaymentReal(new System.Net.Http.HttpClient());
             orderManager.Payment.Real = realPayment;
             Order order = null;
             Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
             Assert.IsNotNull(order);
             Assert.DoesNotThrow(() => orderManager.CollectPayment(order.userKey, order.OrderId, "1111222233334444", "12/2021", "111","name","222333231", context));
         }
        /* Failure TEST
        [Test]
        public void OrderManager_Payment_WrongDetails_Guest_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName = GetStoreName();
            // setup test
            StoreContactDetails details = new StoreContactDetails(storeName, "a@a.com", "location", "222", "222", "bank", "cool store");

            storeHandler.OpenStore(details, Owner, context);
            Store s = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName = "testItemD";
            Item item = s.AddItem(itemName, 2, new HashSet<string>() { "CategoryA" }, 1.22, context, keyWords: new HashSet<string>() { "test" });

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart = new StoreCart(s.Id);
            cart.AddToStoreCart(item.Id, 1);
            shoppingCart.Add(s.Id, cart);

            PaymentReal realPayment = new PaymentReal(new System.Net.Http.HttpClient());
            orderManager.Payment.Real = realPayment;
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(GetGuestId(), shoppingCart, context));
            Assert.IsNotNull(order);
            Assert.Throws<PaymentFailedException>(() => orderManager.CollectPayment(order.userKey, order.OrderId, "", "12/2021", "", "name", "222333231", context));
        }
        */

        [Test]
        public void OrderManager_ReviewPending_Timeout_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 2);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 2);
            shoppingCart.Add(store2.Id, cart2);
            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            context.SaveChanges();
            context.Dispose();

            Thread.Sleep(1000 * 60 * 4);// sleep max time for delete

            using var context2 = new MarketDbContext(inMemoryConnection);
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context2));
        }

        [Test]
        public void OrderManager_ReviewPending_Activate_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            orderManager.Timer.Enabled = false;
            string storeName1 = GetStoreName();
            string storeName2 = GetStoreName();

            // setup test
            StoreContactDetails details1 = new StoreContactDetails(storeName1, "a@a.com", "location", "222", "222", "bank", "cool store");
            StoreContactDetails details2 = new StoreContactDetails(storeName2, "b@b.com", "location", "222", "222", "bank", "very cool store");

            storeHandler.OpenStore(details1, Owner, context);
            storeHandler.OpenStore(details2, Owner, context);

            Store store1 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName1, StringComparison.InvariantCultureIgnoreCase)).First();
            Store store2 = storeHandler.Stores(context).Where(s => s.ContactDetails.Name.Equals(storeName2, StringComparison.InvariantCultureIgnoreCase)).First();

            string itemName1 = "testItemD";
            string itemName2 = "testItemE";

            Item item1 = store1.AddItem(itemName1, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);
            Item item2 = store2.AddItem(itemName2, 2, new HashSet<string>() { "CategoryA" }, 1.22, context);

            Dictionary<Guid, StoreCart> shoppingCart = new Dictionary<Guid, StoreCart>();
            StoreCart cart1 = new StoreCart(store1.Id);
            cart1.AddToStoreCart(item1.Id, 2);
            shoppingCart.Add(store1.Id, cart1);

            StoreCart cart2 = new StoreCart(store2.Id);
            cart2.AddToStoreCart(item2.Id, 2);
            shoppingCart.Add(store2.Id, cart2);
            // test
            Order order = null;
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            Assert.IsNotNull(order);
            Assert.Throws<ItemAmountException>(() => orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
            orderManager.RemovePendings(context, DateTime.Now.AddMinutes(4));
            Assert.DoesNotThrow(() => order = orderManager.DiscountAndReserve(Buyer1, shoppingCart, context));
        }

        private string GetStoreName()
        {
            return $"store_{Guid.NewGuid()}";
        }

        private Guid? GetGuestId()
        {
            return null;
        }
    }
}
