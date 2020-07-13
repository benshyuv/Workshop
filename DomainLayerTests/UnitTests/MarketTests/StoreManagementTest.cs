using DomainLayer.DbAccess;
using DomainLayer.Exceptions;
using DomainLayer.Orders;
using DomainLayer.StoreManagement;
using DomainLayer.Stores;
using DomainLayer.Stores.Certifications;
using DomainLayer.Stores.Inventory;
using DomainLayer.Users;
using DomainLayerTests.UnitTests.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace DomainLayerTests.UnitTests.StoreManagementTests
{
    class StoreManagementTest
	{
        private Guid TEST_NO_USER = Guid.NewGuid();
        private Guid TEST_FIRST_OWNER;
        private Guid TEST_OWNER;
        private Guid TEST_MANAGER;
        private Guid TEST_NEW_USER;
        private string STUB_USER_NAME = "STUB_NAME";
        static HashSet<string> categories1 = new HashSet<string>() { "cat1" };
        string storeName;
        StoreHandler storeHandler;
        OrderManager orderManager;
        Guid storeID;
        Guid itemID;
        StoreManagementFacade storeManagement;
        Item item;
        private Effort.Provider.EffortConnection inMemoryConnection;

        [SetUp]
        public void Setup()
        {
            inMemoryConnection = Effort.DbConnectionFactory.CreateTransient();
            using var context = new MarketDbContext(inMemoryConnection);
            context.Init();
            RegisteredUser first_owner = new RegisteredUser("FIRST_OWNER", new byte[] { });
            context.Users.Add(first_owner);
            TEST_FIRST_OWNER = first_owner.ID;
            RegisteredUser owner = new RegisteredUser("OWNER", new byte[] { });
            context.Users.Add(owner);
            TEST_OWNER = owner.ID;
            RegisteredUser manager = new RegisteredUser("MANAGER", new byte[] { });
            context.Users.Add(manager);
            TEST_MANAGER = manager.ID;
            RegisteredUser new_user = new RegisteredUser("NEW_USER", new byte[] { });
            context.Users.Add(new_user);
            TEST_NEW_USER = new_user.ID;
            storeName = DataForTests.CreateTestContactDetails().Name;
            storeHandler = new StoreHandler();
            orderManager = new OrderManager(storeHandler, inMemoryConnection);
            storeManagement = new StoreManagementFacade(storeHandler, orderManager);
            storeHandler.OpenStore(DataForTests.CreateTestContactDetails(), TEST_FIRST_OWNER, context);
            Store store = storeHandler.GetStoreByName(storeName, context);
            store.AddOwner(TEST_OWNER, TEST_FIRST_OWNER, STUB_USER_NAME, storeHandler, context);
            store.AddManager(TEST_MANAGER, TEST_OWNER, context);
            item = store.AddItem("existing", 2, categories1, 3.55, context);
            itemID = item.Id;
            storeID = store.Id;
        }

        [Test]
        public void AddItem_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);
            
            Assert.NotNull(storeManagement.AddItem(storeID, TEST_FIRST_OWNER, "newItem", 1, categories1, 6.0, context));
        }

        [Test]
        public void AddItem_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<CertificationException>(() => storeManagement.AddItem(storeID, TEST_NEW_USER, "newItem", 1, categories1, 6.0, context));
        }

        [Test]
        public void AddItem_Manager_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<PermissionException>(() => storeManagement.AddItem(storeID, TEST_MANAGER, "newItem", 1, categories1, 6.0, context));
        }

        [Test]
        public void DeleteItem_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(() => storeManagement.DeleteItem(storeID, TEST_FIRST_OWNER, itemID, context));
        }

        [Test]
        public void DeleteItem_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<CertificationException>(() => storeManagement.DeleteItem(storeID, TEST_NEW_USER, itemID, context));
        }

        [Test]
        public void DeleteItem_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<PermissionException>(() => storeManagement.DeleteItem(storeID, TEST_MANAGER, itemID, context));
        }

        [Test]
        public void Store_EditItem_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<string> newCategories = new List<string>() { "newCat" };
            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.categories, newCategories }
            };

            Assert.DoesNotThrow(() => storeManagement.EditItem(storeID, TEST_FIRST_OWNER, itemID, newDetails, context));
        }

        [Test]
        public void Store_EditItem_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            HashSet<string> newCategories = new HashSet<string>() { "newCat" };
            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.categories, newCategories }
            };

            Assert.Throws<CertificationException>(() => storeManagement.EditItem(storeID, TEST_NEW_USER, itemID, newDetails, context));
        }

        [Test]
        public void Store_EditItem_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            HashSet<string> newCategories = new HashSet<string>() { "newCat" };
            Dictionary<StoresUtils.ItemEditDetails, object> newDetails = new Dictionary<StoresUtils.ItemEditDetails, object>()
            {
                {StoresUtils.ItemEditDetails.categories, newCategories }
            };

            Assert.Throws<PermissionException>(() => storeManagement.EditItem(storeID, TEST_MANAGER, itemID, newDetails, context));
        }

        [Test]
        public void Store_EditItem_NoSuchItem_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Guid fakeId = Guid.NewGuid();
            Assert.Throws(Is.TypeOf<ItemNotFoundException>()
           .And.Message.EqualTo(string.Format("Invalid Item id: {0}", fakeId)),
            () => storeManagement.EditItem(storeID, TEST_FIRST_OWNER, fakeId, new Dictionary<StoresUtils.ItemEditDetails, object>() { }, context));

        }

        [Test]
        public void AppointOwner_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(() => storeManagement.AppointOwner(storeID, TEST_NEW_USER, TEST_FIRST_OWNER, STUB_USER_NAME, context));
            //Assert.DoesNotThrow(() => storeManagement.AppointOwner(storeID, TEST_NEW_USER, TEST_FIRST_OWNER, STUB_USER_NAME, context));
        }

        [Test]
        public void AppointOwner_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<CertificationException>(() => storeManagement.AppointOwner(storeID, TEST_NEW_USER, TEST_NO_USER, STUB_USER_NAME, context));
        }

        [Test]
        public void AppointOwner_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<PermissionException>(() => storeManagement.AppointOwner(storeID, TEST_NEW_USER, TEST_MANAGER, STUB_USER_NAME, context));
        }

        [Test]
        public void AppointManager_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(() => storeManagement.AppointManager(storeID, TEST_NEW_USER, TEST_FIRST_OWNER, context));
        }

        [Test]
        public void AppointManager_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<CertificationException>(() => storeManagement.AppointManager(storeID, TEST_NEW_USER, TEST_NO_USER, context));
        }

        [Test]
        public void AppointManager_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<PermissionException>(() => storeManagement.AppointManager(storeID, TEST_NEW_USER, TEST_MANAGER, context));
        }

        [Test]
        public void AddPermission_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(() => storeManagement.AddPermission(storeID, TEST_MANAGER, TEST_OWNER, Permission.INVENTORY, context));
        }

        [Test]
        public void AddPermission_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            AppointManager_Owner_ShouldPass();
            Assert.Throws<PermissionException>(() => storeManagement.AddPermission(storeID, TEST_MANAGER, TEST_NEW_USER, Permission.INVENTORY, context));
        }

        [Test]
        public void AddPermission_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            AppointManager_Owner_ShouldPass();
            Assert.Throws<PermissionException>(() => storeManagement.AddPermission(storeID, TEST_NEW_USER, TEST_MANAGER, Permission.INVENTORY, context));
        }

        [Test]
        public void RemovePermission_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            AddPermission_Owner_ShouldPass();
            Assert.DoesNotThrow(() => storeManagement.RemovePermission(storeID, TEST_MANAGER, TEST_OWNER, Permission.HISTORY, context));
        }

        [Test]
        public void RemovePermission_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            AddPermission_Owner_ShouldPass();
            Assert.Throws<CertificationException>(() => storeManagement.RemovePermission(storeID, TEST_MANAGER, TEST_NEW_USER, Permission.INVENTORY, context));
        }

        [Test]
        public void RemovePermission_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            AppointManager_Owner_ShouldPass();
            Assert.DoesNotThrow(() => storeManagement.AddPermission(storeID, TEST_NEW_USER, TEST_FIRST_OWNER, Permission.INVENTORY, context));
            Assert.Throws<PermissionException>(() => 
                    storeManagement.RemovePermission(storeID, TEST_NEW_USER, TEST_MANAGER, Permission.INVENTORY, context));
        }

        [Test]
        public void RemoveOwner_Grantor_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(() => storeManagement.RemoveOwner(storeID, TEST_OWNER, TEST_FIRST_OWNER, context));
        }

        [Test]
        public void RemoveOwner_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<CertificationException>(() => storeManagement.RemoveOwner(storeID, TEST_OWNER, TEST_NEW_USER, context));
        }

        [Test]
        public void RemoveOwner_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<PermissionException>(() => storeManagement.RemoveOwner(storeID, TEST_OWNER, TEST_MANAGER, context));
        }

        [Test]
        public void RemoveManager_Grantor_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(() => storeManagement.RemoveManager(storeID, TEST_MANAGER, TEST_OWNER, context));
        }

        [Test]
        public void RemoveManager_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<CertificationException>(() => storeManagement.RemoveOwner(storeID, TEST_MANAGER, TEST_NEW_USER, context));
        }

        [Test]
        public void RemoveManager_NoPermission_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            AppointManager_Owner_ShouldPass();
            Assert.Throws<PermissionException>(() => storeManagement.RemoveManager(storeID, TEST_NEW_USER, TEST_MANAGER, context));
        }

        [Test]
        public void GetStoreOrderHistory_Owner_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            User user = new User();
            user.AddToCart(storeID, itemID, 1);
            Order order = orderManager.DiscountAndReserve(null, user.Cart.StoreCarts, context);
            order.Status = OrderStatus.DELIVERED;
            context.SaveChanges();
            orderManager.LogOrder(order.userKey, order.OrderId, context);
            List<StoreOrder> storeOrders = null;
            Assert.DoesNotThrow(() => storeOrders = storeManagement.GetStoreOrderHistory(storeID, TEST_FIRST_OWNER, context));
            Assert.IsNotNull(storeOrders);
            Assert.AreEqual(1, storeOrders.Count);
            Assert.AreEqual(order.OrderId, storeOrders[0].OrderId);
        }

        [Test]
        public void GetStoreOrderHistory_NoCertification_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            User user = new User();
            user.AddToCart(storeID, itemID, 1);
            Order order = orderManager.DiscountAndReserve(null, user.Cart.StoreCarts, context);
            order.Status = OrderStatus.DELIVERED;
            context.SaveChanges();
            orderManager.LogOrder(order.userKey, order.OrderId, context);
            Assert.Throws<CertificationException>(() => storeManagement.GetStoreOrderHistory(storeID, TEST_NEW_USER, context));
        }

        [Test]
        public void GetStoreOrderHistory_NoPermission_ShouldFail()
        {
            RemovePermission_Owner_ShouldPass();

            using var context = new MarketDbContext(inMemoryConnection);
            User user = new User();
            user.AddToCart(storeID, itemID, 1);
            Order order = orderManager.DiscountAndReserve(null, user.Cart.StoreCarts, context);
            order.Status = OrderStatus.DELIVERED;
            context.SaveChanges();
            orderManager.LogOrder(order.userKey, order.OrderId, context);
            Assert.Throws<PermissionException>(() => storeManagement.GetStoreOrderHistory(storeID, TEST_MANAGER, context));
        }

        [Test]
        public void OpenStore_ShouldPass()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.DoesNotThrow(()=> storeManagement.OpenStore(new StoreContactDetails("store2", "store@gmail.com", "Address", "0544444444", 
                                                                                "888-444444/34", "Leumi", "Store description"),
                                                        TEST_NEW_USER, context));
        }

        [Test]
        public void GetPermissions_NotGrantor_ShouldFail()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            Assert.Throws<NonGrantorException>(() => storeManagement.GetUserPermissions(storeID, TEST_MANAGER, TEST_FIRST_OWNER, context));
        }

        [Test]
        public void GetPermissions_Manager_Limited()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Permission> permissions = storeManagement.GetUserPermissions(storeID, TEST_MANAGER, TEST_OWNER, context);
            Assert.AreEqual(2, permissions.Count);
            Assert.True(permissions.Contains(Permission.HISTORY) & permissions.Contains(Permission.REQUESTS));
        }

        [Test]
        public void GetPermissions_Owner_All()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Permission> permissions = storeManagement.GetUserPermissions(storeID, TEST_OWNER, TEST_FIRST_OWNER, context);
            Assert.AreEqual(Enum.GetValues(typeof(Permission)).Length, permissions.Count);
            foreach (Permission p in Enum.GetValues(typeof(Permission)))
            {
                Assert.IsTrue(permissions.Contains(p));
            }
        }

        [Test]
        public void GetMyPermissions_Manager_Limited()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Permission> permissions = storeManagement.GetPermissionsInStore(storeID, TEST_MANAGER, context);
            Assert.AreEqual(2, permissions.Count);
            Assert.True(permissions.Contains(Permission.HISTORY) & permissions.Contains(Permission.REQUESTS));
        }

        [Test]
        public void GetMyPermissions_Owner_All()
        {
            using var context = new MarketDbContext(inMemoryConnection);

            List<Permission> permissions = storeManagement.GetPermissionsInStore(storeID, TEST_OWNER, context);
            Assert.AreEqual(Enum.GetValues(typeof(Permission)).Length, permissions.Count);
            foreach (Permission p in Enum.GetValues(typeof(Permission)))
            {
                Assert.IsTrue(permissions.Contains(p));
            }
        }
    }
}
