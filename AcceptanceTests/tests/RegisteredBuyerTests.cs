using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;
using AcceptanceTests.OperationsAPI;
using NUnit.Framework;

namespace AcceptanceTests.tests
{
    public class RegisteredBuyerTests : GuestBuyerTests
    {

        private string[] registeredUserDetails = new string[] { "Registered", "p@sSw0rD" };

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            storeOperations.registerNewUser(registeredUserDetails[0], registeredUserDetails[1]);
            storeOperations.login(registeredUserDetails[0], registeredUserDetails[1]);

        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public override void UC_2_2_register_successScenario(String userName, String pw)
        {
            Assert.False(storeOperations.registerNewUser(userName, pw));
        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public override void UC_2_2_register_existingUserName_failureScenario(String userName, String pw)
        {
            storeOperations.registerNewUser(userName, pw);
            Assert.False(storeOperations.registerNewUser(userName, pw));
        }

        [Test, TestCaseSource("UserNameAndPasswords")]
        public override void UC_2_3_login_existingUserName_successScenario(String userName, String pw)
        {
            storeOperations.registerNewUser(userName, pw);
            //login should fail if already logged in
            Assert.False(storeOperations.login(userName, pw));
        }

        [Test, TestCaseSource("addItemToShoppingCart_firstBuyFromStore_cases")]
        public override void UC_2_6_addItemToShoppingCart_firstBuyFromStore_successAndFailureScenario(Item itemToBuywithoutGUID, int amountToBuy, Store storeToGenerateAndBuyFrom,
            bool success)
        {
            base.UC_2_6_addItemToShoppingCart_firstBuyFromStore_successAndFailureScenario(itemToBuywithoutGUID, amountToBuy, storeToGenerateAndBuyFrom,
                success);
            logoutThenLogin();
            Assert.True(storeOperations.validateCartHas(itemToBuywithoutGUID, amountToBuy));
        }

        [Test, TestCaseSource("addItemToShoppingCart_secondBuyFromStore_Cases")]
        public override void UC_2_6_addItemToShoppingCart_secondBuyFromStore_successAndFailureScenario(Item[] itemsToBuywithoutGUID, int amountToBuy, Store storeToGenerateAndBuyFrom,
            bool success)
        {
            base.UC_2_6_addItemToShoppingCart_secondBuyFromStore_successAndFailureScenario(itemsToBuywithoutGUID, amountToBuy, storeToGenerateAndBuyFrom,
                success);
            logoutThenLogin();
            Assert.True(storeOperations.validateCartHas(itemsToBuywithoutGUID[0], 1));
            Assert.AreEqual(success,storeOperations.validateCartHas(itemsToBuywithoutGUID[1], amountToBuy));
        }

        private void logoutThenLogin()
        {
            storeOperations.logout();
            storeOperations.login(registeredUserDetails[0], registeredUserDetails[1]);
        }

        [Test, TestCaseSource("updateAmountInShoppingCartSuccessOrFailCases")]
        public override void UC_2_7_updateAmountInShoppingCart_successAndFailureScenario(Item itemToSetupWith, int amountSetUp,
            Item itemToUpdate, int amountUpdate, Store storeToGenerateAndBuyFrom, bool success)
        {
            base.UC_2_7_updateAmountInShoppingCart_successAndFailureScenario(itemToSetupWith, amountSetUp,
             itemToUpdate, amountUpdate, storeToGenerateAndBuyFrom, success);
            logoutThenLogin();
            Assert.AreEqual(success, storeOperations.validateCartHas(itemToUpdate, amountUpdate));
        }

        [Test, TestCaseSource("buyProductsCases")]
        public override void UC_2_8_buyProductImmediate_successAndFailure(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCartWithoutGUID,
            Store[] storeToGenerateAndBuyFrom, Item[] itemsToBeBoughtBetweenCartAddAndPurchaseWithoutGUID,
            int[] amountToBeBoughtBetweenCartAddAndPurchase, string[] paymentDetails, string[] deliveryDetails, bool success)
        {
            base.UC_2_8_buyProductImmediate_successAndFailure(itemsToAddToCartWithoutGUID, amountToAddToCartWithoutGUID,
                storeToGenerateAndBuyFrom, itemsToBeBoughtBetweenCartAddAndPurchaseWithoutGUID,
                amountToBeBoughtBetweenCartAddAndPurchase, paymentDetails, deliveryDetails, success);
            logoutThenLogin();
            if(success)
                Assert.True(storeOperations.validateCurrentUserPurchaseHistoryIncludesAndPersistent(itemsToAddToCartWithoutGUID, amountToAddToCartWithoutGUID));
        }
 
        [Test, TestCaseSource("buyProductsForPaymentSystemFailCases")]
        public override void UC_2_8_buyProductImmediate_PaymentFailedOnCreditCard(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart,
            Store[] storesToGenerateAndBuyFrom, string[] paymentDetails, string[] deliveryDetails)
        {
            int numOdersOfUserBefore = storeOperations.getCurrentUserHistoryOrdersIds().Count;
            base.UC_2_8_buyProductImmediate_PaymentFailedOnCreditCard(itemsToAddToCartWithoutGUID, amountToAddToCart, storesToGenerateAndBuyFrom, paymentDetails, deliveryDetails);
            logoutThenLogin();

            int numOdersOfUserAfter = storeOperations.getCurrentUserHistoryOrdersIds().Count;

            //validate order not saved
            Assert.AreEqual(numOdersOfUserBefore, numOdersOfUserAfter);
        }


        [Test, TestCaseSource("buyProductsForDeliverySystemFailCases")]
        public override void UC_2_8_buyProductImmediate_DeliveryFailed(Item[] itemsToAddToCartWithoutGUID, int[] amountToAddToCart,
            Store[] storesToGenerateAndBuyFrom, string[] paymentDetails, string[] deliveryDetails)
        {
            int numOdersOfUserBefore = storeOperations.getCurrentUserHistoryOrdersIds().Count;

            base.UC_2_8_buyProductImmediate_DeliveryFailed(itemsToAddToCartWithoutGUID, amountToAddToCart, storesToGenerateAndBuyFrom, paymentDetails, deliveryDetails);
            logoutThenLogin();

            int numOdersOfUserAfter = storeOperations.getCurrentUserHistoryOrdersIds().Count;

            //validate order not saved
            Assert.AreEqual(numOdersOfUserBefore, numOdersOfUserAfter);
        }

        [Test]
        public override void UC_3_1_logout_shouldFailONGuestPassONRegistered()
        {
            Assert.True(storeOperations.logout());
        }

        public static StoreContactDetails notValidStoreContactName = new StoreContactDetails("", "valid@mail.com", "valid", "valid", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreContactEmail = new StoreContactDetails("valid", "", "valid", "valid", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreContactAddress = new StoreContactDetails("valid", "valid@mail.com", "", "valid", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreContactPhone = new StoreContactDetails("valid", "valid@mail.com", "valid", "", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreContactBank = new StoreContactDetails("valid", "valid@mail.com", "valid",  "valid", "", "valid", "valid");
        public static StoreContactDetails notvalidStoreContactBankNumber = new StoreContactDetails("valid", "valid@mail.com", "valid", "valid", "valid", "", "valid");
        public static StoreContactDetails notvalidStoreContactDescription = new StoreContactDetails("valid", "valid@mail.com", "valid", "valid", "valid", "valid", "");
        public static StoreContactDetails notValidStoreNullContactName = new StoreContactDetails(null, "valid@mail.com", "valid", "valid", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreNullContactEmail = new StoreContactDetails("valid", null, "valid", "valid", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreNullContactAddress = new StoreContactDetails("valid", "valid@mail.com", null, "valid", "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreNullContactPhone = new StoreContactDetails("valid", "valid@mail.com", "valid", null, "valid", "valid", "valid");
        public static StoreContactDetails notvalidStoreNullContactBank = new StoreContactDetails("valid", "valid@mail.com", "valid", "valid", null, "valid", "valid");
        public static StoreContactDetails notvalidStoreNullContactBankNumber = new StoreContactDetails("valid", "valid@mail.com", "valid", "valid", "valid", null, "valid");
        public static StoreContactDetails notvalidStoreNullContactDescription = new StoreContactDetails("valid", "valid@mail.com", "valid", "valid", "valid", "valid", null);

        public static object[] openStoreCases =    
            {
                new object[] { validStoreContact, true},
                new object[] { notValidStoreContactName, false},
                new object[] { notvalidStoreContactEmail, false},
                new object[] { notvalidStoreContactAddress, false},
                new object[] { notvalidStoreContactPhone, false},
                new object[] { notvalidStoreContactBank, false},
                new object[] { notvalidStoreContactBankNumber, false},
                new object[] { notvalidStoreContactDescription, false},
                new object[] { notValidStoreNullContactName, false},
                new object[] { notvalidStoreNullContactEmail, false},
                new object[] { notvalidStoreNullContactAddress, false},
                new object[] { notvalidStoreNullContactPhone, false},
                new object[] { notvalidStoreNullContactBank, false},
                new object[] { notvalidStoreNullContactBankNumber, false},
                new object[] { notvalidStoreNullContactDescription, false},
            };

        [Test, TestCaseSource("openStoreCases")]
        public override void UC_3_2_openingAStore_SuccessAndFailureScenarios(StoreContactDetails contactDetails, bool success)
        {
            Assert.AreEqual(success, storeOperations.openStore(contactDetails));

        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_3_7_whatchPersonalPurchaseHistory_shouldSucceed(int numOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = storeOperations.buyRandomItemsFromCurrentUser(numOfPurchases);
            logoutThenLogin();
            CollectionAssert.AreEquivalent(purchasesMade, storeOperations.getCurrentUserHistory());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_3_7_whatchPersonalPurchaseHistory_withPriceChanges_shouldSucceed(int numOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = storeOperations.buyRandomItemsFromCurrentUser(numOfPurchases);
            storeOperations.changePricesOfItemsInPurchaseRecord(purchasesMade);
            logoutThenLogin();
            CollectionAssert.AreEquivalent(purchasesMade, storeOperations.getCurrentUserHistory());
        }

        [Test]
        public override void UC_4_2_AddOpenedDiscount_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddOpenedDiscountError(s.Id, item.Id, 0.20, 30);

            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public override void UC_4_2_AddItemConditionalDiscountOnAll_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnAllError(s.Id, item.Id, 30, 5, 0.20);

            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public override void UC_4_2_AddItemConditional_DiscountOnExtraItems_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddItemConditionalDiscount_MinItems_ToDiscountOnExtraItemsError(s.Id, item.Id, 30, 3, 1, 0.5);

            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public override void UC_4_2_AddStoreConditionalDiscount_NoPermmision_ShouldFail()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];
            Dictionary<Item, int> items = s.ItemsWithAmount;
            Item item = null;
            foreach (KeyValuePair<Item, int> keyValue in items)
            {
                item = keyValue.Key;
                break;
            }

            string answer = storeOperations.AddStoreConditionalDiscountError(s.Id, 60, 270.50, 0.10);

            Assert.AreEqual("no permission", answer);
        }

        [Test]
        public override void UC_4_2_RemoveDiscount_NoPermmision_ShouldFail()
        {
            Tuple<Store, ItemOpenedDiscount> tuple = storeOperations.GenerateStoreWithItemAndOpenedDiscountLogin(new Store(new Item[] { bannana, iphone, galaxy }, new int[] { 2, 1, 1 }));
            Assert.AreEqual("no permission", storeOperations.RemoveDiscountError(tuple.Item1.Id, tuple.Item2.DiscountID));
        }

        [Test]
        public override void UC_4_2_MakeDiscountAllowed_NoPermission_FailureScenario()
        {

            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];

            Assert.AreEqual("no permission", storeOperations.MakeDiscountAllowedError(s.Id, "opened"));
        }

        [Test]
        public override void UC_4_2_MakeDiscountNotAllowed_NoPermission_FailureScenario()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];

            Assert.AreEqual("no permission", storeOperations.MakeDiscountNotAllowedError(s.Id, "opened"));
        }

        [Test]
        public override void UC_4_2_GetAllowedDiscounts_NoPermission_FailureScenario()
        {
            List<Store> storesWithItems = storeOperations.generateRandomListOfStoresWithItems();
            Store s = storesWithItems[0];

            Assert.AreEqual("no permission", storeOperations.GetAllowedDiscountsError(s.Id));
        }

        [Test]
        public override void UC_4_6_GetStoresWithPermissions_NoPermissions_FailureScenario()
        {

            List<Tuple<Store, List<string>>> result = storeOperations.GetStoresWithPermissionsSuccess();

            Assert.AreEqual(0, result.Count);
        }
    }
}
