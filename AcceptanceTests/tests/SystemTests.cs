using System;
using System.Collections.Generic;
using System.IO;
using AcceptanceTests.DataObjects;
using AcceptanceTests.OperationsAPI;
using NUnit.Framework;

namespace AcceptanceTests.tests
{
    public class SystemTests
    {
        public ISystemOperationsBridge systemOperations;

        [SetUp]
        public virtual void Setup()
        {
            systemOperations = new SystemOperationsProxy();
        }

        [TestCase("admin","password",true)]
        [TestCase("randomname1", "password", true)]
        [TestCase("", "password", false)]
        [TestCase("randomname1", "", false)]
        [TestCase("", "", false)]
        [TestCase(null, "password", false)]
        [TestCase("randomname1", null, false)]
        public void UC_1_1_systemStartup_successAndFailScenario(String userName, String pw, bool success)
        {
            Assert.AreEqual(success, systemOperations.startupWithValidation(userName, pw));
        }

        [Test]
        public void UC_1_1_systemStartup_with_operation_file_success()
        {
            string path = "temp_op_file.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                //register,login, openstore, additem
                sw.WriteLine(@"userActions.Register(sessionID, ""user1"", ""Aa1234"");");
                sw.WriteLine(@"userActions.Login(sessionID, ""user1"", ""Aa1234""); ");
                sw.WriteLine(@"Guid storeID = Json.storeID_from_store_json(storeActions.OpenStore(sessionID, ""st1"", ""tomer@gmail.com"", ""tel aviv"", ""052 - 5555555"", ""1232112"", ""poalim"", ""desc1"", """", """"));");
                sw.WriteLine(@"Guid itemID = Json.itemID_from_item_json(storeActions.AddItem(sessionID, storeID, ""item1"", 10, ""['cat1']"", 5, ""['key1']""));");
                // add discounts
                sw.WriteLine(@"Guid LdiscountID = Json.discountID_from_open_discount_json(storeActions.AddOpenDiscount(sessionID, storeID, itemID, 0.5, 10));");
                sw.WriteLine(@"Guid RdiscountID = Json.discountID_from_store_discount_json(storeActions.AddStoreConditionalDiscount(sessionID, storeID, 10, 1, 0.5));");
                sw.WriteLine(@"storeActions.ComposeTwoDiscounts(sessionID, storeID, LdiscountID, RdiscountID, ""&"");");
                sw.WriteLine(@"userActions.Logout(sessionID);");
                sw.WriteLine(@"userActions.Register(sessionID, ""user2"", ""Aa1234"");");
                sw.WriteLine(@"userActions.Login(sessionID, ""user2"", ""Aa1234""); ");
                //buy cart
                sw.WriteLine(@"userActions.AddToCart(sessionID, storeID, itemID, 1);");
                sw.WriteLine(@"Guid orderID = Json.orderID_from_order_json(purchaseActions.DisplayBeforeCheckout(sessionID));");
                sw.WriteLine(@"purchaseActions.CheckOut(sessionID, orderID, ""1234 - 3333 - 2222 - 1111"", ""10 / 24"", ""666"",""Yossii"",""2122122121"", ""TLV"", ""TLV"", ""IL"", ""1234567"");");
                sw.WriteLine(@"userActions.Logout(sessionID);");
                //add owner, add manager
                sw.WriteLine(@"userActions.Register(sessionID, ""user3"", ""Aa1234"");");
                sw.WriteLine(@"userActions.Login(sessionID, ""user1"", ""Aa1234""); ");
                sw.WriteLine(@"storeActions.AppointManager(sessionID, storeID, ""user2"");");
                sw.WriteLine(@"storeActions.AppointOwner(sessionID, storeID, ""user3"");");

            }

            Assert.AreEqual(true, systemOperations.startupWithInitFileAndValidation("ADMIN", "ADMIN",path));
            List<Store> stores = systemOperations.getAllStoresAndItems();
            Assert.AreEqual(1, stores.Count);
            Assert.AreEqual("st1", stores[0].Name);
            Assert.AreEqual(1, stores[0].ItemsWithAmount.Count);
            systemOperations.login("ADMIN","ADMIN");
            Assert.AreEqual(1, systemOperations.getStoreHistoryByAdmin(stores[0]).Count);
            Assert.AreEqual(5*0.5*0.5, systemOperations.getStoreHistoryByAdmin(stores[0])[0].DiscountedPriceOnPurchase);
            systemOperations.logout();
            systemOperations.isManagerOfStore(stores[0], new User("user2", "Aa1234"));
            systemOperations.isOwnerOfStore(stores[0], new User("user3", "Aa1234"));

        }
        [Test]
        public void UC_1_1_systemStartup_with_operation_file_file_not_exist_fail()
        {
            Assert.AreEqual(false, systemOperations.startupWithInitFileAndValidation("ADMIN", "ADMIN", "no_file_here"));
        }
        [Test]
        public void UC_1_1_systemStartup_with_operation_file_file_format_not_good_fail()
        {
            string path = "temp_op_file.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(@"userActions.thisIsNotAnOperation();");
            }

            Assert.AreEqual(false, systemOperations.startupWithInitFileAndValidation("ADMIN", "ADMIN", path));
        }
    }
}
