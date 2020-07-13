using System;
using System.Collections.Generic;
using AcceptanceTests.DataObjects;
using AcceptanceTests.OperationsAPI;
using NUnit.Framework;
namespace AcceptanceTests.tests
{
    public class SystemAdminTests


    {
        public StoreOwnerTests ownerTests;
        User registeredUserDetails;
        public ISystemOperationsBridge systemOperations;
        User randomUser;

        [SetUp]
        public void Setup()
        {
            ownerTests = new StoreOwnerTests();
            ownerTests.Setup();
            systemOperations = ownerTests.systemOperations;
            registeredUserDetails = systemOperations.getNewUserForTests();
            randomUser = systemOperations.getNewUserForTests();        
        }
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_6_4_systemAdminWatchesPurchaseHistoryOfStore_withPriceChanges_successScenario(int numOfPurchases)
        {

            (List<PurchaseRecord> purchasesMade, Store store1) = ownerTests.viewStorePurchaseHistory_withPriceChanges_helper(numOfPurchases, true);
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistoryByAdmin(store1));

        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_6_4_systemAdminWatchesPurchaseHistoryOfStore_withItemDeletions_successScenario(int numOfPurchases)
        {
            (List<PurchaseRecord> purchasesMade, Store store1) = ownerTests.viewStorePurchaseHistory_withItemDeletions_helper(numOfPurchases, true);
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistoryByAdmin(store1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_6_4_systemAdminWatchesPurchaseHistoryOfStore_successScenario(int numOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = ownerTests.makeRandomPurchases(numOfPurchases,
               ownerTests.store1, ownerTests.loggedInStore1Owner, true);
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getStoreHistoryByAdmin(ownerTests.store1));
        }

        [Test]
        public void UC_6_4_systemAdminWatchesPurchaseHistoryOfStore_noPurchases_successScenario()
        {
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            Assert.AreEqual(0, systemOperations.getStoreHistoryByAdmin(ownerTests.store1).Count);
        }

        [Test]
        public void UC_6_4_systemAdminWatchesPurchaseHistoryOfStore_notAdmin_failureScenario()
        {
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            Assert.AreEqual(0,systemOperations.getStoreHistoryByAdmin(ownerTests.store1).Count);
        }


        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_6_4_systemAdminwatchPersonalPurchaseHistory_shouldSucceed(int numOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = makePurchasesFromRegisteredUserAndReturn(numOfPurchases);
            Assert.True(systemOperations.loginSystemAdmin());
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getUserHistory(registeredUserDetails.Username));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void UC_6_4_systemAdminwatchPersonalPurchaseHistory_withPriceChanges_shouldSucceed(int numOfPurchases)
        {
            List<PurchaseRecord> purchasesMade = makePurchasesFromRegisteredUserAndReturn(numOfPurchases);
            systemOperations.changePricesOfItemsInPurchaseRecord(purchasesMade);
            systemOperations.loginSystemAdmin();
            CollectionAssert.AreEquivalent(purchasesMade, systemOperations.getUserHistory(registeredUserDetails.Username));
        }
        [Test]
        public void UC_6_4_systemAdminwatchPersonalPurchaseHistory_notByAdmin_shouldFail()
        {
            systemOperations.login(randomUser.Username, randomUser.Pw);
            Assert.Null( systemOperations.getUserHistory(registeredUserDetails.Username));
        }

        private List<PurchaseRecord> makePurchasesFromRegisteredUserAndReturn(int numOfPurchases)
        {
            systemOperations.logout();
            systemOperations.login(registeredUserDetails.Username, registeredUserDetails.Pw);
            List<PurchaseRecord> ret = systemOperations.buyRandomItemsFromCurrentUser(numOfPurchases);
            systemOperations.logout();
            return ret;
        }

        private int guestIndex = 0;
        private int registeredIndex = 1;
        private int managersIndex = 2;
        private int ownersIndex = 3;
        private int adminsIndex = 4;

        [Test]
        public void UC_6_5_systemAdminStatistics_success()
        {
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            Dictionary<DateTime, int[]> statistics = systemOperations.GetStatisticsSuccess(DateTime.Today, null);
            systemOperations.doActionFromGuest();
            Dictionary<DateTime, int[]> newStatistics = systemOperations.GetStatisticsSuccess(DateTime.Today, null);
            Assert.True(ValidateIndexUpBy(statistics[DateTime.Today], newStatistics[DateTime.Today], guestIndex, 1));

            statistics = newStatistics;
            systemOperations.doActionFromUser(ownerTests.registeredUser);
            newStatistics = systemOperations.GetStatisticsSuccess(DateTime.Today, null);
            Assert.True(ValidateIndexUpBy(statistics[DateTime.Today], newStatistics[DateTime.Today], registeredIndex, 1));

            statistics = newStatistics;
            systemOperations.doActionFromUser(ownerTests.manager1);
            systemOperations.doActionFromUser(ownerTests.manager2);
            newStatistics = systemOperations.GetStatisticsSuccess(DateTime.Today, null);
            Assert.True(ValidateIndexUpBy(statistics[DateTime.Today], newStatistics[DateTime.Today], managersIndex, 2));

            statistics = newStatistics;
            systemOperations.doActionFromUser(ownerTests.store2Owner);
            systemOperations.doActionFromUser(ownerTests.store1AdditionalOwner);
            systemOperations.doActionFromUser(ownerTests.loggedInStore1Owner);
            newStatistics = systemOperations.GetStatisticsSuccess(DateTime.Today, null);
            Assert.True(ValidateIndexUpBy(statistics[DateTime.Today], newStatistics[DateTime.Today], ownersIndex, 3));

            systemOperations.logout();
            systemOperations.loginSystemAdmin();

            statistics = newStatistics;
            newStatistics = systemOperations.GetStatisticsSuccess(DateTime.Today, null);
            Assert.True(ValidateIndexUpBy(statistics[DateTime.Today], newStatistics[DateTime.Today], adminsIndex, 1));
        }

        private bool ValidateIndexUpBy( int[] statistics,int[] newStatistics, int index, int value)
        {
            for(int i = 0; i<5; i++)
            {
                if (i == index)
                {
                    if(statistics[i] + value != newStatistics[i])
                    {
                        return false;
                    }

                }
                else
                {
                    if (statistics[i] != newStatistics[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        [Test]
        public void UC_6_5_systemAdminStatistics_notAdmin_fail()
        {
            string error = systemOperations.GetStatisticsError(DateTime.Today, null);

            Assert.AreEqual("Not admin", error);
        }

        [Test]
        public void UC_6_5_systemAdminStatistics_guest_fail()
        {
            systemOperations.logout();
            string error = systemOperations.GetStatisticsError(DateTime.Today, null);
            Assert.AreEqual("not logged in", error);
        }

        [Test]
        public void UC_6_5_systemAdminStatistics_from_after_to_fail()
        {
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            string error = systemOperations.GetStatisticsError(DateTime.Today, DateTime.Today.AddDays(-2));
            Assert.AreEqual("Invalid input", error);
        }
        [Test]
        public void UC_6_5_systemAdminStatistics_to_future_fail()
        {
            systemOperations.logout();
            systemOperations.loginSystemAdmin();
            string error = systemOperations.GetStatisticsError(DateTime.Today, DateTime.Today.AddDays(10));
            Assert.AreEqual("Invalid input", error);
        }
    }
}
