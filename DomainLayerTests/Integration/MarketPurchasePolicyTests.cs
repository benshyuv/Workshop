using System;
using System.Collections.Generic;
using DomainLayer.Market;
using DomainLayer.Stores.PurchasePolicies;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DomainLayerTests.Integration
{
    internal class MarketPurchasePolicyTests : IMarketFacadeTests
    {

        private readonly List<string> allowed_PurchaseType_strings =
          new List<string> { "item", "store", "days", "composite" };
        [SetUp]
        public void SetUp()
        {
            SetupUsers();
            SetupStores();
            CreateItemsAndAddToCart(REGISTERED_SESSION_ID, true);
        }

        [Test]
        public void IsPurchasePolicyTypeAllowed_Success()
        {
            foreach (string policy in allowed_PurchaseType_strings)
            {
                Assert.True(marketFacade.IsPurchaseTypeAllowed(FIRST_STORE_ID, policy));
            }
        }

        [Test]
        public void MakePolicyTypeNotAllowed_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            foreach (string policy in allowed_PurchaseType_strings)
            {
                Assert.True(DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy));
            }
        }

        [Test]
        public void MakePolicyTypeAllowed_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            foreach (string policy in allowed_PurchaseType_strings)
            {
                Assert.True(DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy));
            }
            foreach (string policy in allowed_PurchaseType_strings)
            {
                Assert.True(DeserializeMakePurchasePolicyAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy));
            }

        }

        [Test]
        public void ChangePolicyTypesAndValidate_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            foreach (string policy in allowed_PurchaseType_strings)
            {
                Assert.True(DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy));
                Assert.False(marketFacade.IsPurchaseTypeAllowed(FIRST_STORE_ID, policy));
            }
            foreach (string policy in allowed_PurchaseType_strings)
            {
                Assert.True(DeserializeMakePurchasePolicyAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy));
                Assert.True(marketFacade.IsPurchaseTypeAllowed(FIRST_STORE_ID, policy));
            }

        }

        [Test]
        public void GetPurchasePolicyTypes_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            List<string> allowedPolicy = DeserializeGetAllowedPurchasePolicySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID);

            CollectionAssert.AreEquivalent(allowed_PurchaseType_strings, allowedPolicy.ConvertAll((s) => s.ToLower().Trim()));

            DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "item");
            allowedPolicy = DeserializeGetAllowedPurchasePolicySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID);
            allowedPolicy.Add("item");
            CollectionAssert.AreEquivalent(allowed_PurchaseType_strings, allowedPolicy.ConvertAll((s) => s.ToLower().Trim()));
        }

        [Test]
        public void AddItemMinMax_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "item");

            Assert.False(DeserialzeItemMinMax(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID,
                5, 10).Success);

        }

        [Test]
        public void AddStoreMinMax_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "store");

            Assert.False(DeserialzeStoreMinMax(REGISTERED_SESSION_ID, FIRST_STORE_ID,
                5, 10).Success);

        }

        [Test]
        public void AddDaysNotAllowed_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "days");

            Assert.False(DeserialzeDaysNotAllowed(REGISTERED_SESSION_ID, FIRST_STORE_ID,
                new int[] { 1, 5, 7 }).Success);

        }

        [Test]
        public void AddCompositePurchaePolicy_typeNotAllowed_failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            DeserializeMakePurchasePolicyNotAllowedSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, "composite");


            ItemMinMaxPurchasePolicy left = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);

            StoreMinMaxPurchasePolicy right = DeserialzeStoreMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, 5, 10);

            Assert.False(DeserializeCompositePurchasePolicy(REGISTERED_SESSION_ID, FIRST_STORE_ID, left.policyID,
                right.policyID, "&").Success);

        }

        [Test]
        public void IsValidToCreateItemMinMax_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            Assert.True(marketFacade.IsValidToCreateItemMinMaxPurchasePolicy(FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID));
            DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);

            Assert.False(marketFacade.IsValidToCreateItemMinMaxPurchasePolicy(FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID));
            Assert.True(marketFacade.IsValidToCreateItemOpenedDiscount(FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateStoreMinMax_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            Assert.True(marketFacade.IsValidToCreateStoreMinMaxPurchasePolicy(FIRST_STORE_ID));
            DeserialzeStoreMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, 5, 10);

            Assert.False(marketFacade.IsValidToCreateStoreMinMaxPurchasePolicy(FIRST_STORE_ID));
        }

        [Test]
        public void IsValidToCreateDaysMinMax_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            Assert.True(marketFacade.IsValidToCreateDaysNotAllowedPurchasePolicy(FIRST_STORE_ID));
            DeserialzeDaysNotAllowed(REGISTERED_SESSION_ID, FIRST_STORE_ID,
                new int[] { 1, 5, 7 });

            Assert.False(marketFacade.IsValidToCreateDaysNotAllowedPurchasePolicy(FIRST_STORE_ID));
        }


        [Test]
        public void PolicyExistInStore_success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            Assert.True(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy.policyID));
        }

        [Test]
        public void PolicyExistInStore_no_policy_success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            Assert.False(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, Guid.NewGuid()));
        }

        [Test]
        public void RemoveBasicPolicy_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            Assert.True(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy.policyID));
            Assert.True(DeserializeRemovePurchasePolicySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy.policyID));
            Assert.False(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy.policyID));
        }

        [Test]
        public void RemoveBasicPolicy_noSuchPolicy_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            Assert.True(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy.policyID));
            Assert.True(DeserializeRemovePurchasePolicySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy.policyID));
            Assert.False(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy.policyID));
            Assert.False(DeserializeRemovePurchasePolicy(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy.policyID).Success);
        }

        [Test]
        public void RemoveCompositeDiscount_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy1 = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            ItemMinMaxPurchasePolicy policy2 = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, 5, 10);
            CompositePurchasePolicy composite = DeserializeCompositePurchasePolicySucces(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy1.policyID,
                policy2.policyID, "&");

            Assert.True(DeserializeRemovePurchasePolicySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, composite.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, policy1.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, policy2.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, composite.policyID));

        }

        [Test]
        public void RemoveCompositeDiscount_composedOfComposites_Success()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy1 = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            ItemMinMaxPurchasePolicy policy2 = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, 5, 10);
            CompositePurchasePolicy composite1 = DeserializeCompositePurchasePolicySucces(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy1.policyID,
                policy2.policyID, "&");

            StoreMinMaxPurchasePolicy policy3 = DeserialzeStoreMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, 5, 10);
            DaysNotAllowedPurchasePolicy policy4 = DeserialzeDaysNotAllowedSucces(REGISTERED_SESSION_ID, FIRST_STORE_ID,new int[] { 1, 5, 7 } );
            CompositePurchasePolicy composite2 = DeserializeCompositePurchasePolicySucces(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy3.policyID,
                policy4.policyID, "|");

            CompositePurchasePolicy composite3 = DeserializeCompositePurchasePolicySucces(REGISTERED_SESSION_ID, FIRST_STORE_ID, composite1.policyID,
                composite2.policyID, "xor");

            Assert.True(DeserializeRemovePurchasePolicySuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, composite3.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, policy1.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, policy2.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, policy3.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, policy4.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, composite1.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, composite2.policyID));
            Assert.False(marketFacade.DiscountExistsInStore(FIRST_STORE_ID, composite3.policyID));
        }

        [Test]
        public void RemoveBasicPolicy_thatIsPartOfComposite_Failure()
        {
            LoginSessionSuccess(REGISTERED_SESSION_ID, FIRST_OPENER_USERNAME, PASSWORD);
            ItemMinMaxPurchasePolicy policy1 = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, FIRST_ITEM_FIRST_STORE_ID, 5, 10);
            ItemMinMaxPurchasePolicy policy2 = DeserialzeItemMinMaxSuccess(REGISTERED_SESSION_ID, FIRST_STORE_ID, SECOND_ITEM_FIRST_STORE_ID, 5, 10);
            CompositePurchasePolicy composite1 = DeserializeCompositePurchasePolicySucces(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy1.policyID,
                policy2.policyID, "&");

            Assert.False(DeserializeRemovePurchasePolicy(REGISTERED_SESSION_ID, FIRST_STORE_ID, policy1.policyID).Success);
            Assert.True(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy1.policyID));
            Assert.True(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, policy2.policyID));
            Assert.True(marketFacade.PurchasePolicyExistsInStore(FIRST_STORE_ID, composite1.policyID));

        }



    }
}
