
using System;
using System.Collections.Generic;
using DomainLayer.Stores.Discounts;
using DomainLayer.Stores.PurchasePolicies;
using DomainLayer.Users;
using NUnit.Framework;

namespace DomainLayerTests.UnitTests.StoresTests.PurchasePolicyTests
{
    [TestFixture]
    public class PurchasePolicyTests
    {
        private Guid Item1;
        private Guid Item2;
        private Guid Item3;
        private StoreCart cart1With5;
        private StoreCart cart2With5;
        private StoreCart multyItemsAmount10;
        private readonly string MOCK_NAME_FOR_DESCRIPTION = "mock";
        private readonly Guid MOCK_GUID_FOR_STORE = Guid.NewGuid();
        [SetUp]
        public void Setup()
        {
            Item1 = Guid.NewGuid();
            Item3 = Guid.NewGuid();
            Item2 = Guid.NewGuid();
            cart1With5 = new StoreCart(MOCK_GUID_FOR_STORE);
            cart1With5.AddToStoreCart(Item1, 5);
            cart2With5 = new StoreCart(MOCK_GUID_FOR_STORE);
            cart2With5.AddToStoreCart(Item2, 5);
            multyItemsAmount10 = new StoreCart(MOCK_GUID_FOR_STORE);
            multyItemsAmount10.AddToStoreCart(Item1, 2);
            multyItemsAmount10.AddToStoreCart(Item2, 3);
            multyItemsAmount10.AddToStoreCart(Item3, 5);
        }

        public void itemMinMaxPolicyTest_runner(Guid itemID, int? minAmount, int? maxAmount, StoreCart cart, bool valid)
        {
            ItemMinMaxPurchasePolicy policy = new ItemMinMaxPurchasePolicy(itemID, minAmount, maxAmount, MOCK_NAME_FOR_DESCRIPTION);
            Assert.AreEqual(valid, policy.IsValidPurchase(cart));
        }

        [Test]
        public void itemMinMaxPolicyTest_min_only_success()
        {
            itemMinMaxPolicyTest_runner(Item1, 5, null, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, 6, null, cart1With5, false);
            itemMinMaxPolicyTest_runner(Item1, 4, null, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, 6, null, cart2With5, true);
        }

        [Test]
        public void itemMinMaxPolicyTest_max_only_success()
        {
            itemMinMaxPolicyTest_runner(Item1, null, 5, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, null, 6, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, null, 4, cart1With5, false);
            itemMinMaxPolicyTest_runner(Item1, null, 4, cart2With5, true);
        }

        [Test]
        public void itemMinMaxPolicyTest_max_min_success()
        {
            itemMinMaxPolicyTest_runner(Item1, 3, 6, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, 5, 5, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, 6, 8, cart1With5, false);
            itemMinMaxPolicyTest_runner(Item1, 3, 4, cart1With5, false);
            itemMinMaxPolicyTest_runner(Item1, 3, 5, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, 5, 7, cart1With5, true);
            itemMinMaxPolicyTest_runner(Item1, 3, 4, cart2With5, true);
            itemMinMaxPolicyTest_runner(Item1, 3, 4, cart2With5, true);

        }

        [Test]
        public void itemMinMaxPolicyTest_illeagalValues()
        {
            Assert.Throws<ArgumentException>(() => new ItemMinMaxPurchasePolicy(Item1, 5, 4, MOCK_NAME_FOR_DESCRIPTION));
            Assert.Throws<ArgumentException>(() => new ItemMinMaxPurchasePolicy(Item1, null, null, MOCK_NAME_FOR_DESCRIPTION));
        }

        public void storeMinMaxPurchasePolicy_runner(int? minAmount, int? maxAmount, StoreCart cart, bool valid)
        {
            StoreMinMaxPurchasePolicy policy = new StoreMinMaxPurchasePolicy(minAmount, maxAmount, MOCK_NAME_FOR_DESCRIPTION);
            Assert.AreEqual(valid, policy.IsValidPurchase(cart));
        }

        [Test]
        public void storeMinMaxPurchasePolicy_min_only_success()
        {
            storeMinMaxPurchasePolicy_runner(5, null, cart1With5, true);
            storeMinMaxPurchasePolicy_runner(6, null, cart1With5, false);
            storeMinMaxPurchasePolicy_runner(4, null, cart1With5, true);
            storeMinMaxPurchasePolicy_runner(10, null, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(9, null, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(11, null, multyItemsAmount10, false);
        }

        [Test]
        public void storeMinMaxPurchasePolicy_max_only_success()
        {
            storeMinMaxPurchasePolicy_runner( null, 5, cart1With5, true);
            storeMinMaxPurchasePolicy_runner( null, 6, cart1With5, true);
            storeMinMaxPurchasePolicy_runner( null, 4, cart1With5, false);
            storeMinMaxPurchasePolicy_runner(null, 10, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(null, 11, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(null, 9, multyItemsAmount10, false);
        }

        [Test]
        public void storeMinMaxPurchasePolicy_max_min_success()
        {
            storeMinMaxPurchasePolicy_runner( 3, 6, cart1With5, true);
            storeMinMaxPurchasePolicy_runner( 5, 5, cart1With5, true);
            storeMinMaxPurchasePolicy_runner( 6, 8, cart1With5, false);
            storeMinMaxPurchasePolicy_runner( 3, 4, cart1With5, false);
            storeMinMaxPurchasePolicy_runner( 3, 5, cart1With5, true);
            storeMinMaxPurchasePolicy_runner( 5, 7, cart1With5, true);

            storeMinMaxPurchasePolicy_runner(7, 12, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(10, 10, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(11, 13, multyItemsAmount10, false);
            storeMinMaxPurchasePolicy_runner(7, 9, multyItemsAmount10, false);
            storeMinMaxPurchasePolicy_runner(7, 10, multyItemsAmount10, true);
            storeMinMaxPurchasePolicy_runner(10, 13, multyItemsAmount10, true);


        }

        [Test]
        public void storeMinMaxPurchasePolicy_illeagalValues()
        {
            Assert.Throws<ArgumentException>(() => new StoreMinMaxPurchasePolicy( 5, 4, MOCK_NAME_FOR_DESCRIPTION));
            Assert.Throws<ArgumentException>(() => new StoreMinMaxPurchasePolicy( null, null, MOCK_NAME_FOR_DESCRIPTION));
        }

        [Test]
        public void daysNotAllowed_validTest()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int day = (int)today + 2 == 8 ? 1 : (int)today + 2;
            DaysNotAllowedPurchasePolicy policy = new DaysNotAllowedPurchasePolicy(new int[] { day }, MOCK_NAME_FOR_DESCRIPTION);
            Assert.True( policy.IsValidPurchase(cart1With5));
        }
        [Test]
        public void daysNotAllowed_multpleDays_validTest()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int day = (int)today + 2 == 8 ? 1 : (int)today + 2;
            int day2 = (int)today + 3 == 8 ? 1 : (int)today + 3;
            DaysNotAllowedPurchasePolicy policy = new DaysNotAllowedPurchasePolicy(new int[] { day, day2 }, MOCK_NAME_FOR_DESCRIPTION);
            Assert.True(policy.IsValidPurchase(cart1With5));
        }

        [Test]
        public void daysNotAllowed_notValidTest()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int day = (int)today + 1;
            DaysNotAllowedPurchasePolicy policy = new DaysNotAllowedPurchasePolicy(new int[] { day }, MOCK_NAME_FOR_DESCRIPTION);
            Assert.False(policy.IsValidPurchase(cart1With5));
        }
        [Test]
        public void daysNotAllowed_multpleDays_notValidTest()
        {
            DayOfWeek today = DateTime.Now.DayOfWeek;
            int day = (int)today + 1;
            int day2 = (int)today + 2 == 8 ? 1 : (int)today + 2;
            int day3 = (int)today + 3 == 8 ? 1 : (int)today + 3;
            DaysNotAllowedPurchasePolicy policy = new DaysNotAllowedPurchasePolicy(new int[] { day, day2, day3 }, MOCK_NAME_FOR_DESCRIPTION);
            Assert.False(policy.IsValidPurchase(cart1With5));
        }

        [Test]
        public void daysNotAllowed_illeagalValues()
        {
            Assert.Throws<ArgumentException>(() => new DaysNotAllowedPurchasePolicy(new int[] { }, MOCK_NAME_FOR_DESCRIPTION));
            Assert.Throws<ArgumentException>(() => new DaysNotAllowedPurchasePolicy(null,  MOCK_NAME_FOR_DESCRIPTION));
            Assert.Throws<ArgumentException>(() => new DaysNotAllowedPurchasePolicy(new int[] {1,2,3,4,5,6,7}, MOCK_NAME_FOR_DESCRIPTION));

        }

        [TestCase(Operator.AND)]
        [TestCase(Operator.OR)]
        public void CompositePolicy_both_true_valid(Operator @operator)
        {
            ItemMinMaxPurchasePolicy policy1 = new ItemMinMaxPurchasePolicy(Item1, 1, null, MOCK_NAME_FOR_DESCRIPTION);
            StoreMinMaxPurchasePolicy policy2 = new StoreMinMaxPurchasePolicy( 10, 10, MOCK_NAME_FOR_DESCRIPTION);

            CompositePurchasePolicy compositePurchase = new CompositePurchasePolicy(policy1, policy2, @operator);
            Assert.True(compositePurchase.IsValidPurchase(multyItemsAmount10));
        }

        [TestCase(Operator.XOR)]
        public void CompositePolicy_both_true_not_valid(Operator @operator)
        {
            ItemMinMaxPurchasePolicy policy1 = new ItemMinMaxPurchasePolicy(Item1, 1, null, MOCK_NAME_FOR_DESCRIPTION);
            StoreMinMaxPurchasePolicy policy2 = new StoreMinMaxPurchasePolicy(10, 10, MOCK_NAME_FOR_DESCRIPTION);

            CompositePurchasePolicy compositePurchase = new CompositePurchasePolicy(policy1, policy2, @operator);
            Assert.False(compositePurchase.IsValidPurchase(multyItemsAmount10));
        }

        [TestCase(Operator.XOR)]
        [TestCase(Operator.OR)]
        public void CompositePolicy_one_true_one_false_valid(Operator @operator)
        {
            ItemMinMaxPurchasePolicy policy1 = new ItemMinMaxPurchasePolicy(Item1, 1, null, MOCK_NAME_FOR_DESCRIPTION);
            StoreMinMaxPurchasePolicy policy2 = new StoreMinMaxPurchasePolicy(11, 12, MOCK_NAME_FOR_DESCRIPTION);

            CompositePurchasePolicy compositePurchase = new CompositePurchasePolicy(policy1, policy2, @operator);
            Assert.True(compositePurchase.IsValidPurchase(multyItemsAmount10));
            compositePurchase = new CompositePurchasePolicy(policy2, policy1, @operator);
            Assert.True(compositePurchase.IsValidPurchase(multyItemsAmount10));
        }

        [TestCase(Operator.AND)]
        public void CompositePolicy_one_true_one_false_not_valid(Operator @operator)
        {
            ItemMinMaxPurchasePolicy policy1 = new ItemMinMaxPurchasePolicy(Item1, 1, null, MOCK_NAME_FOR_DESCRIPTION);
            StoreMinMaxPurchasePolicy policy2 = new StoreMinMaxPurchasePolicy(11, 12, MOCK_NAME_FOR_DESCRIPTION);

            CompositePurchasePolicy compositePurchase = new CompositePurchasePolicy(policy1, policy2, @operator);
            Assert.False(compositePurchase.IsValidPurchase(multyItemsAmount10));
            compositePurchase = new CompositePurchasePolicy(policy2, policy1, @operator);
            Assert.False(compositePurchase.IsValidPurchase(multyItemsAmount10));
        }

        [TestCase(Operator.AND)]
        [TestCase(Operator.XOR)]
        [TestCase(Operator.OR)]
        public void CompositePolicy_both_false_not_valid(Operator @operator)
        {
            ItemMinMaxPurchasePolicy policy1 = new ItemMinMaxPurchasePolicy(Item1, 6, null, MOCK_NAME_FOR_DESCRIPTION);
            StoreMinMaxPurchasePolicy policy2 = new StoreMinMaxPurchasePolicy(11, 12, MOCK_NAME_FOR_DESCRIPTION);

            CompositePurchasePolicy compositePurchase = new CompositePurchasePolicy(policy1, policy2, @operator);
            Assert.False(compositePurchase.IsValidPurchase(multyItemsAmount10));
            
        }

    }
}

